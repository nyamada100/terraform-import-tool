using Cysharp.Diagnostics;

internal class TfstateLookup
{
    internal async ValueTask<string> LookupAsync(string fileName, string address)
    {
        return await ProcessX.StartAsync($"tfstate-lookup -s {fileName} {address}").FirstAsync();
    }
}

