
using Cysharp.Diagnostics;
using System.Collections.ObjectModel;
using ValueTaskSupplement;
using Tommy;
using Microsoft.Extensions.Logging;
using ZLogger;

internal class Terraform
{
    private static readonly string NEW_STAE_FILE;
    private const string RESOURCE_IDENTIFIER_CSV = "resource_identifier.csv";

    private static readonly string NEW_TERRAFORM_DIR;

    private static readonly IEnumerable<string> OLD_STATE_FILES;

    private readonly ILogger logger;

    private static ReadOnlyDictionary<string, string> Resources = new(File.ReadAllLines(RESOURCE_IDENTIFIER_CSV)
    .Select(l => l.Split(',')).ToDictionary(s => s[0], s => s[1]));

    /// <summary>
    /// staticコンストラクタ
    /// 設定ファイル読み込み
    /// </summary>
    static Terraform()
    {
        using var reader = File.OpenText("settings.toml");

        var table = TOML.Parse(reader);

        OLD_STATE_FILES = table["tfstate"]["old_state_files"].AsArray.Children.AsEnumerable<TomlNode>().Select(node => node.ToString()!);
        NEW_TERRAFORM_DIR = table["tfstate"]["import_dir"].AsString;
        NEW_STAE_FILE = table["tfstate"]["new_state_file"].AsString;
    }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="logger">ロガー</param>
    public Terraform(ILogger logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// Terraform stateリスト取得
    /// </summary>
    /// <returns>リソース定義の列挙</returns>
    internal async IAsyncEnumerable<string> StateListAsync()
    {
        await foreach (var r in ProcessX.StartAsync($"terraform -chdir={NEW_TERRAFORM_DIR} state list -state {NEW_STAE_FILE}"))
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
    internal async ValueTask ImportNewResourceAsync(TfstateLookup tfStateLookup, string res, DirectoryInfo newResourceDir)
    {
        this.logger.ZLogDebug($"import start: {DateTime.Now}");

        var type = res.Substring(0, res.IndexOf('.'));

        var resourceAddress = $"{res}.{Resources[type]}";

        this.logger.ZLogDebug($"lookup start: {DateTime.Now}");
        var r = await ValueTaskEx.WhenAll(OLD_STATE_FILES.Select(s =>
            tfStateLookup.LookupAsync(s, resourceAddress)
        ));
        this.logger.ZLogDebug($"lookup end: {DateTime.Now}");

        if (r.All(s => s == "null"))
        {
            //存在しない(すべてnull)のでインポート
            this.logger.ZLogDebug($"write start: {DateTime.Now}");
            await WriteToFile(res, newResourceDir);
            this.logger.ZLogDebug($"write end: {DateTime.Now}");
        }

        this.logger!.ZLogDebug($"import end: {DateTime.Now}");
    }

    /// <summary>
    /// 指定のリソース定義を指定のディレクトリにファイルを作成して書き出す
    /// </summary>
    /// <param name="res">書き出すリソース</param>
    /// <param name="newResourceDir">ファイル書き出し先ディレクトリ</param>
    /// <returns></returns>
    private static async ValueTask WriteToFile(string res, DirectoryInfo newResourceDir)
    {
        var newTfFile = $"{res.Replace('.', '-')}.tf";
        await foreach (var line in ProcessX.StartAsync($"terraform -chdir={NEW_TERRAFORM_DIR} state show -no-color -state {NEW_STAE_FILE} {res}"))
        {
            await File.AppendAllTextAsync(Path.Combine(newResourceDir.FullName, newTfFile), line + Environment.NewLine);
        }
    }
}



