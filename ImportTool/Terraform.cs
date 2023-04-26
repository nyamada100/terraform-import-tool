
using Cysharp.Diagnostics;
using System.Collections.ObjectModel;
using ValueTaskSupplement;

using Microsoft.Extensions.Logging;
using ZLogger;

interface ITerraform
{
    IAsyncEnumerable<string> StateListAsync(string newTerraformDir, string newStateFile);
    ValueTask ImportNewResourceAsync(IEnumerable<string> oldStateFiles, string newTerraformDir, string newStateFile, string res, DirectoryInfo newResourceDir);
}

internal class Terraform : ITerraform
{
    private const string RESOURCE_IDENTIFIER_CSV = "resource_identifier.csv";

    private readonly ILogger<ConsoleApp> logger;

    private readonly ITfstateLookup tfStateLookup;

    private static ReadOnlyDictionary<string, string> Resources = new(File.ReadAllLines(RESOURCE_IDENTIFIER_CSV)
    .Select(l => l.Split(',')).ToDictionary(s => s[0], s => s[1]));

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="logger">ロガー</param>
    /// <param name="tfStateLookup">tfstate-lookup</param>
    public Terraform(ILogger<ConsoleApp> logger, ITfstateLookup tfStateLookup)
    {
        this.logger = logger;
        this.tfStateLookup = tfStateLookup;
    }

    /// <summary>
    /// Terraform stateリスト取得
    /// </summary>
    /// <returns>リソース定義の列挙</returns>
    public async IAsyncEnumerable<string> StateListAsync(string newTerraformDir, string newStateFile)
    {
        await foreach (var r in ProcessX.StartAsync($"terraform -chdir={newTerraformDir} state list -state {newStateFile}"))
        {
            if (r.StartsWith("data.")) { continue; }
            yield return r;
        }
    }

    /// <summary>
    /// 指定のリソースIDを古いstateファイルから探し、存在しない場合は指定のディレクトリにリソース定義を書き出す
    /// </summary>
    /// <param name="tfStateLookup">リソース定義を探すクラス</param>
    /// <param name="res">探すリソース</param>
    /// <param name="newResourceDir">書き出し先ディレクトリ</param>
    /// <returns></returns>
    public async ValueTask ImportNewResourceAsync(IEnumerable<string> oldStateFiles, string newTerraformDir, string newStateFile, string res, DirectoryInfo newResourceDir)
    {
        this.logger.ZLogDebug($"import start: {DateTime.Now}");

        var type = res.Substring(0, res.IndexOf('.'));

        var resourceAddress = $"{res}.{Resources[type]}";

        this.logger.ZLogDebug($"lookup start: {DateTime.Now}");
        var r = await ValueTaskEx.WhenAll(oldStateFiles.Select(s =>
            tfStateLookup.LookupAsync(s, resourceAddress)
        ));
        this.logger.ZLogDebug($"lookup end: {DateTime.Now}");

        if (r.All(s => s == "null"))
        {
            //存在しない(すべてnull)のでインポート
            this.logger.ZLogDebug($"write start: {DateTime.Now}");
            await WriteToFile(newTerraformDir, newStateFile, res, newResourceDir);
            this.logger.ZLogDebug($"write end: {DateTime.Now}");
        }

        this.logger.ZLogDebug($"import end: {DateTime.Now}");
    }

    /// <summary>
    /// 指定のリソース定義を指定のディレクトリにファイルを作成して書き出す
    /// </summary>
    /// <param name="res">書き出すリソース</param>
    /// <param name="newResourceDir">ファイル書き出し先ディレクトリ</param>
    /// <returns></returns>
    internal async ValueTask WriteToFile(string newTerraformDir, string newStateFile, string res, DirectoryInfo newResourceDir)
    {
        var newTfFile = $"{res.Replace('.', '-')}.tf";
        await foreach (var line in ProcessX.StartAsync($"terraform -chdir={newTerraformDir} state show -no-color -state {newStateFile} {res}"))
        {
            await File.AppendAllTextAsync(Path.Combine(newResourceDir.FullName, newTfFile), line + Environment.NewLine);
        }
    }
}



