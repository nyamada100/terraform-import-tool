using Microsoft.Extensions.Logging;
using ZLogger;
using ValueTaskSupplement;
using Tommy;
using Microsoft.Extensions.DependencyInjection;

var app = ConsoleApp.CreateBuilder(args)
.ConfigureLogging(x =>
{
    x.ClearProviders();
    x.SetMinimumLevel(LogLevel.Information);
    x.AddZLoggerConsole();
    x.AddZLoggerFile("terraform-import-tool.log");
})
.ConfigureServices(x =>
{
    x.AddSingleton<ITerraform, Terraform>();
    x.AddSingleton<ITfstateLookup, TfstateLookup>();
}
)
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
    var terraform = app.Services.GetRequiredService<ITerraform>();

    using var reader = File.OpenText("settings.toml");

    var table = TOML.Parse(reader);

    var oldStateFiles = table["tfstate"]["old_state_files"].AsArray.Children.AsEnumerable().Select(node => node.ToString()!);
    var newTerraformDir = table["tfstate"]["new_terraform_dir"].AsString;
    var newStateFile = table["tfstate"]["new_state_file"].AsString;

    await foreach (var res in terraform.StateListAsync(newTerraformDir, newStateFile))
    {
        await terraform.ImportNewResourceAsync(oldStateFiles, newTerraformDir, newStateFile, res, newResourceDir);
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
    var groups = Directory.EnumerateFiles(resourceDir).GroupBy(f => Path.GetFileName(f).Substring(0, Path.GetFileName(f).IndexOf("-")));

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
async ValueTask ConcatFilesAsync(IGrouping<string, string> group, DirectoryInfo concatDir)
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