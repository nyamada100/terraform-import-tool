using System.IO;
using Microsoft.Extensions.Logging;
using ZLogger;

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

async ValueTask<int> MainAsync()
{
    const string NEW_RESOURCE_DIR = "new_resource";

    if (Directory.Exists(NEW_RESOURCE_DIR))
    {
        Directory.Delete(NEW_RESOURCE_DIR, true);
    }
    var newResourceDir = Directory.CreateDirectory(NEW_RESOURCE_DIR);
    var tfStateLookup = new TfstateLookup();
    var terraform = new Terraform(app.Logger);

    await foreach (string res in terraform.StateListAsync())
    {
        await terraform.ImportNewResourceAsync(tfStateLookup, res, newResourceDir);
    }

    ConcatFiles(NEW_RESOURCE_DIR);

    return 0;
}

void ConcatFiles(string resourceDir)
{
    var concatDir = Path.Combine(resourceDir, "concat");
    if (Directory.Exists(concatDir))
    {
        Directory.Delete(concatDir);
    }
    Directory.CreateDirectory(concatDir);
    new DirectoryInfo(resourceDir).GetFiles().GroupBy(f => f.Name.Substring(0, f.Name.IndexOf("-"))).ToList().ForEach(g =>
    {
        var resource = g.Key;
        var destFile = Path.Combine(concatDir, $"{resource}.tf");
        using (Stream destStream = File.Create(destFile))
        {
            foreach (var item in g)
            {
                using (Stream srcStream = File.OpenRead(item.FullName))
                {
                    srcStream.CopyTo(destStream);
                }
            }
        }
    });
}

app.Run();