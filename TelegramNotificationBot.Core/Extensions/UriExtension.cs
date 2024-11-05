using Microsoft.AspNetCore.WebUtilities;

namespace TelegramNotificationBot.Core.Extensions;
public static class UriExtension
{
    public static Uri CreateWithQuery(this Uri uri, IDictionary<string, string?> queries)
      => new(QueryHelpers.AddQueryString(uri.OriginalString, queries));
}
