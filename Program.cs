using Microsoft.Extensions.Logging;
using ZLogger;
using ValueTaskSupplement;

var app = ConsoleApp.CreateBuilder(args)
    .ConfigureLogging(x =>
    {
        x.ClearProviders();
        x.SetMinimumLevel(LogLevel.Information);
        x.AddZLoggerConsole();
        x.AddZLoggerFile("terraform-import-tool.log");
    })
    .Build();

app.AddRootCommand("new resource import", async () =>
{
    app.Logger.ZLogInformation($"start program: {DateTime.Now}");

    await MainAsync();

    app.Logger.ZLogInformation($"end program: {DateTime.Now}");
});

app.Run();

/// <summary>
/// メインロジック
/// 新たなTerraform stateリストから、古いstateに存在しないリソース定義を抜き出してリソース定義ファイルを作成する
/// </summary>
/// <returns></returns>
async ValueTask<int> MainAsync()
{
    const string NEW_RESOURCE_DIR = "new_resource";

    if (Directory.Exists(NEW_RESOURCE_DIR))
    {
        Directory.Delete(NEW_RESOURCE_DIR, true);
    }
    var newResourceDir = Directory.CreateDirectory(NEW_RESOURCE_DIR);
    var tfStateLookup = new TfstateLookup(app.Logger);
    var terraform = new Terraform(app.Logger);

    await foreach (string res in terraform.StateListAsync())
    {
        await terraform.ImportNewResourceAsync(tfStateLookup, res, newResourceDir);
    }

    ConcatFiles(NEW_RESOURCE_DIR);

    return 0;
}

/// <summary>
/// 同じリソースタイプの定義を1つのファイルにまとめる
/// </summary>
/// <param name="resourceDir">リソース定義ファイルのあるディレクトリ</param>
/// <returns>なし</returns>
async void ConcatFiles(string resourceDir)
{
    var concatDir = Path.Combine(resourceDir, "concat");
    if (Directory.Exists(concatDir))
    {
        Directory.Delete(concatDir);
    }
    var dir = Directory.CreateDirectory(concatDir);
    var groups = Directory.EnumerateFiles(resourceDir).GroupBy(f => f.Substring(0, f.IndexOf("-")));

    await ValueTaskEx.WhenAll(groups.Select(group =>
        ConcatFilesAsync(group, dir)
    ));
}

/// <summary>
/// ファイルグループを、グループごとに1つのファイルにまとめて指定されたディレクトリに作成する
/// </summary>
/// <param name="group">ファイルグループ</param>
/// <param name="concatDir">ファイルを作成するディレクトリ</param>
/// <returns></returns>
static async ValueTask ConcatFilesAsync(IGrouping<string, string> group, DirectoryInfo concatDir)
{
    var resource = group.Key;

    var destFile = Path.Combine(concatDir.FullName, $"{resource}.tf");

    using var destStream = File.Create(destFile);
    foreach (var item in group)
    {
        using var srcStream = File.OpenRead(item);
        await srcStream.CopyToAsync(destStream);
    }
    
}
