
using Cysharp.Diagnostics;
using System.Collections.ObjectModel;
using ValueTaskSupplement;
using Tommy;
using Microsoft.Extensions.Logging;
using ZLogger;

internal class Terraform
{
    static readonly string NEW_STATE_FILE;
    const string RESOURCE_IDENTIFIER_CSV = "resource_identifier.csv";

    static readonly string NEW_STATE_DIR;

    static readonly IEnumerable<string> STATE_FILES;

    private readonly ILogger logger;

    private static ReadOnlyDictionary<string, string> Resources = new(File.ReadAllLines(RESOURCE_IDENTIFIER_CSV)
    .Select(l => l.Split(',')).ToDictionary(s => s[0], s => s[1]));

    static Terraform()
    {
        using StreamReader reader = File.OpenText("settings.toml");

        TomlTable table = TOML.Parse(reader);

        STATE_FILES = table["tfstate"]["old_state_files"].AsArray.Children.AsEnumerable<TomlNode>().Select(node => node.ToString()!);
        
        var newStateFile = table["tfstate"]["new_state_file"].AsString.ToString();

        NEW_STATE_DIR=Path.GetDirectoryName(newStateFile)!;
        NEW_STATE_FILE=Path.GetFileName(newStateFile)!;
    }

    public Terraform(ILogger logger)
    {
        this.logger = logger;
    }

    internal async IAsyncEnumerable<string> StateListAsync()
    {
        await foreach (var r in ProcessX.StartAsync($"terraform -chdir={NEW_STATE_DIR} state list -state {NEW_STATE_FILE}"))
        {
            yield return r;
        }
    }

    internal async ValueTask ImportNewResourceAsync(TfstateLookup tfStateLookup, string res, DirectoryInfo newResourceDir)
    {
        this.logger.ZLogDebug($"import start: {DateTime.Now}");

        var type = res.Substring(0, res.IndexOf('.'));

        var resourceAddress = $"{res}.{Resources[type]}";

        this.logger.ZLogDebug($"lookup start: {DateTime.Now}");
        var r = await ValueTaskEx.WhenAll(STATE_FILES.Select(s =>
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

    private static async ValueTask WriteToFile(string res, DirectoryInfo newResourceDir)
    {
        var newTfFile = $"{res.Replace('.', '-')}.tf";
        await foreach (var line in ProcessX.StartAsync($"terraform -chdir={NEW_STATE_DIR} state show -no-color -state {NEW_STATE_FILE} {res}"))
        {
            await File.AppendAllTextAsync(Path.Combine(newResourceDir.FullName, newTfFile), line + Environment.NewLine);
        }
    }
}



