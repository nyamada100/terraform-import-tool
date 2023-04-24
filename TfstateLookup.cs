using Cysharp.Diagnostics;
using Microsoft.Extensions.Logging;

internal class TfstateLookup
{
    /// <summary>
    /// ロガー
    /// </summary>
    private readonly ILogger logger;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="logger">ロガー</param>
    public TfstateLookup(ILogger logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// stateファイルから、指定のリソースを探す
    /// </summary>
    /// <param name="fileName">stateファイル</param>
    /// <param name="address">探すリソース定義IDのアドレス</param>
    /// <returns>リソース定義ID(見つからない場合はnull文字列)</returns>
    internal async ValueTask<string> LookupAsync(string fileName, string address)
    {
        return await ProcessX.StartAsync($"tfstate-lookup -s {fileName} {address}").FirstAsync();
    }
}

