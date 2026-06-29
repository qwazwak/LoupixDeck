using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Logging;

namespace QCommon.Utils.Extensions.Logging;

public static class LogScopeTagExtensions
{
    extension(ILogger log)
    {
        public IDisposable? BeginScopeTags(ReadOnlySpan<KeyValuePair<string, object>> tags, [CallerMemberName] string callerName = null!)
            => log?.BeginScopeTags(callerName, tags);

        [SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "<Pending>")]
        public IDisposable? BeginScopeTags(ReadOnlySpan<char> name, ReadOnlySpan<KeyValuePair<string, object>> tags)
        {
            if (log is null || tags.IsEmpty)
                return null;
            Debug.Assert(!tags.IsEmpty, "tags should not be empty");
            Debug.Assert(!name.IsEmpty, "name should not be empty");
            StringBuilder sb = new();
            sb.Append(name).Append(" ( ");
            AppendTag(sb, tags[0]);
            foreach (KeyValuePair<string, object> tag in tags[1..])
            {
                sb.Append(", ");
                AppendTag(sb, tag);
            }
            static void AppendTag(StringBuilder sb, KeyValuePair<string, object> tag)
            {
                Debug.Assert(!string.IsNullOrWhiteSpace(tag.Key), "tag key should not be null or whitespace");
                sb.Append(tag.Key).Append(": {").Append(tag.Key).Append('}');
            }

            sb.Append(" )");
            string messageFormat = sb.ToString();

            object[] args = new object[tags.Length];
            for (int i = 0; i < tags.Length; i++)
                args[i] = tags[i].Value;
            return log.BeginScope(messageFormat, args);
        }
    }
}
