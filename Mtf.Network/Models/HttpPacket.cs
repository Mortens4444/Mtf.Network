using Mtf.Network.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using HttpMethod = Mtf.Network.Enums.HttpMethod;

namespace Mtf.Network.Models
{
    public class HttpPacket
    {
        public HttpPacket(Uri uri)
        {
            Uri = uri;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            _ = sb.AppendLine($"{HttpMethod} {Uri.PathAndQuery} {HttpProtocol}/{(int)HttpProtocolVersion / 10}.{(int)HttpProtocolVersion % 10}");

#if NET462_OR_GREATER
            foreach (var (Description, Value) in GetPropertiesWithDescriptions())
            {
                if (!String.IsNullOrEmpty(Value))
                {
                    _ = sb.AppendLine($"{Description}: {Value}");
                }
            }
#else
            foreach (var tuple in GetPropertiesWithDescriptions())
            {
                if (!String.IsNullOrEmpty(tuple.Item2))
                {
                    _ = sb.AppendLine($"{tuple.Item1}: {tuple.Item2}");
                }
            }
#endif

            if (KeepAliveConnection)
            {
                _ = sb.AppendLine("Connection: Keep-Alive");
            }

            _ = sb.AppendLine();
            return sb.ToString();
        }

        public Uri Uri { get; set; }

        public HttpMethod HttpMethod { get; set; } = HttpMethod.GET;

        public HttpProtocol HttpProtocol => Uri.Scheme == "http" ? HttpProtocol.HTTP : HttpProtocol.HTTPS;

        public HttpProtocolVersion HttpProtocolVersion { get; set; } = HttpProtocolVersion.Http11;

        public bool KeepAliveConnection { get; set; }

        [Description("Accept")]
        public string Accept { get; set; } = "*/*";

        [Description("Accept-Charset")]
        public string AcceptCharset { get; set; } = "UTF-8";

        [Description("Accept-Encoding")]
        public string AcceptEncoding { get; set; } = "gzip, deflate";

        [Description("Accept-Language")]
        public string AcceptLanguage { get; set; } = "en-US";

        [Description("Accept-Ranges")]
        public string AcceptRanges { get; set; } = "none";

        [Description("Age")]
        public string Age { get; set; } = "none";

        [Description("Allow")]
        public string Allow { get; set; } = "GET, POST, HEAD";

        [Description("Authorization")]
        public string Authorization { get; set; } = String.Empty;

        [Description("Cache-Control")]
        public string CacheControl { get; set; } = "no-cache";

        [Description("Connection")]
        public string Connection { get; set; } = "Keep-Alive";

        [Description("Content-Encoding")]
        public string ContentEncoding { get; set; } = "gzip";

        [Description("Content-Language")]
        public string ContentLanguage { get; set; } = "en";

        [Description("Content-Length")]
        public string ContentLength { get; set; } = "0";

        [Description("Content-Location")]
        public string ContentLocation { get; set; } = String.Empty;

        [Description("Content-Range")]
        public string ContentRange { get; set; } = String.Empty;

        [Description("Content-Type")]
        public string ContentType { get; set; } = "text/html";

        [Description("Date")]
        public string Date { get; set; } = String.Empty;

        [Description("ETag")]
        public string ETag { get; set; } = String.Empty;

        [Description("Expect")]
        public string Expect { get; set; } = String.Empty;

        [Description("Expires")]
        public string Expires { get; set; } = String.Empty;

        [Description("From")]
        public string From { get; set; } = String.Empty;

        [Description("Host")]
        public string Host => Uri.Host;

        [Description("If-Match")]
        public string IfMatch { get; set; } = String.Empty;

        [Description("If-Modified-Since")]
        public string IfModifiedSince { get; set; } = String.Empty;

        [Description("If-None-Match")]
        public string IfNoneMatch { get; set; } = String.Empty;

        [Description("If-Range")]
        public string IfRange { get; set; } = String.Empty;

        [Description("If-Unmodified-Since")]
        public string IfUnmodifiedSince { get; set; } = String.Empty;

        [Description("Last-Modified")]
        public string LastModified { get; set; } = String.Empty;

        [Description("Location")]
        public string Location { get; set; } = String.Empty;

        [Description("Max-Forwards")]
        public string MaxForwards { get; set; } = "10";

        [Description("Pragma")]
        public string Pragma { get; set; } = "no-cache";

        [Description("Proxy-Authenticate")]
        public string ProxyAuthenticate { get; set; } = String.Empty;

        [Description("Proxy-Authorization")]
        public string ProxyAuthorization { get; set; } = String.Empty;

        [Description("Range")]
        public string Range { get; set; } = String.Empty;

        [Description("Referer")]
        public string Referer { get; set; } = String.Empty;

        [Description("Retry-After")]
        public string RetryAfter { get; set; } = String.Empty;

        [Description("Server")]
        public string Server { get; set; } = String.Empty;

        [Description("TE")]
        public string TE { get; set; } = String.Empty;

        [Description("Trailer")]
        public string Trailer { get; set; } = String.Empty;

        [Description("Transfer-Encoding")]
        public string TransferEncoding { get; set; } = String.Empty;

        [Description("Upgrade")]
        public string Upgrade { get; set; } = String.Empty;

        [Description("User-Agent")]
        public string UserAgent { get; set; } = String.Empty;

        [Description("Vary")]
        public string Vary { get; set; } = String.Empty;

        [Description("Via")]
        public string Via { get; set; } = String.Empty;

        [Description("Warning")]
        public string Warning { get; set; } = String.Empty;

        [Description("WWW-Authenticate")]
        public string WWWAuthenticate { get; set; } = String.Empty;

#if NET462_OR_GREATER
        private IEnumerable<(string Description, string Value)> GetPropertiesWithDescriptions()
#else
        private IEnumerable<Tuple<string, string>> GetPropertiesWithDescriptions()
#endif
        {
            var properties = GetType().GetProperties();
            foreach (var property in properties)
            {
                var descriptionAttribute = property.GetCustomAttribute<DescriptionAttribute>();
                if (descriptionAttribute != null)
                {
                    var value = property.GetValue(this)?.ToString();
#if NET462_OR_GREATER
                    yield return (descriptionAttribute.Description, value);
#else
                    yield return new Tuple<string, string>(descriptionAttribute.Description, value);
#endif
                }
            }
        }
    }
}
