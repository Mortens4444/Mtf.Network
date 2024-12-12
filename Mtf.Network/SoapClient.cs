using Mtf.Network.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Mtf.Network
{
    public class SoapClient
    {
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// Sends a synchronous SOAP request and get the parsed response.
        /// </summary>
        public static string SendRequest(Uri uri, string function, string serviceId, string resultTagName, params SoapParameter[] soapParameters)
        {
            var soapClient = new SoapClient();
            var envelopBody = CreateSoapEnvelopeBody(serviceId, serviceId, soapParameters);
            var response = soapClient.SendRequest(uri, $"{function}#{serviceId}", CreateSoapEnvelope(envelopBody));
            return String.IsNullOrEmpty(resultTagName) ? String.Empty
                : SoapClient.ExtractSoapResponseContent(response, $"<{resultTagName}>", $"</{resultTagName}>");
        }

        /// <summary>
        /// Sends a synchronous SOAP request and get the full response.
        /// </summary>
        public string SendRequest(Uri uri, string soapAction, string xmlRequestBody, Dictionary<string, string> customHeaders = null)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            var request = CreateRequest(uri, soapAction, customHeaders);

            using (var stream = request.GetRequestStream())
            using (var writer = new StreamWriter(stream, Encoding))
            {
                writer.Write(xmlRequestBody);
            }

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var reader = new StreamReader(response.GetResponseStream() ?? Stream.Null, Encoding))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Sends an asynchronous SOAP request.
        /// </summary>
        public async Task<string> SendRequestAsync(Uri uri, string soapAction, string xmlRequestBody, Dictionary<string, string> customHeaders = null)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            var request = CreateRequest(uri, soapAction, customHeaders);

            using (var stream = await request.GetRequestStreamAsync().ConfigureAwait(false))
            using (var writer = new StreamWriter(stream, Encoding))
            {
                await writer.WriteAsync(xmlRequestBody).ConfigureAwait(false);
            }

            using (var response = (HttpWebResponse)await request.GetResponseAsync().ConfigureAwait(false))
            using (var reader = new StreamReader(response.GetResponseStream() ?? Stream.Null, Encoding))
            {
                return await reader.ReadToEndAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Creates a SOAP envelope with the specified body and namespace.
        /// </summary>
        public static string CreateSoapEnvelope(string bodyXml, string soapNamespace = "http://schemas.xmlsoap.org/soap/envelope/")
        {
            var soapEnvelope = new StringBuilder()
                .AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>")
                .AppendLine($"<soap:Envelope xmlns:soap=\"{soapNamespace}\">")
                .AppendLine("<soap:Body>")
                .AppendLine(bodyXml)
                .AppendLine("</soap:Body>")
                .AppendLine("</soap:Envelope>");

            return soapEnvelope.ToString();
        }

        public static string CreateSoapEnvelopeBody(string serviceId, string function, params SoapParameter[] parameters)
        {
            var soapEnvelopeBody = new StringBuilder();
            if (parameters != null && parameters.Length > 0)
            {
                _ = soapEnvelopeBody.Append($"<u:{function} xmlns:u=\"{serviceId}\">");
                foreach (var parameter in parameters)
                {
                    _ = soapEnvelopeBody.Append($"<{parameter.Name}>{parameter.Value}</{parameter.Name}>");
                }
                _ = soapEnvelopeBody.Append($"</u:{function}>");
            }
            else
            {
                _ = soapEnvelopeBody.Append($"<u:{function} xmlns:u=\"{serviceId}\" />");
            }
            return soapEnvelopeBody.ToString();
        }

        /// <summary>
        /// Creates and configures an HTTP web request for SOAP.
        /// </summary>
        private static HttpWebRequest CreateRequest(Uri uri, string soapAction, Dictionary<string, string> customHeaders)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Headers.Add("SOAPAction", soapAction);
            request.ContentType = "text/xml;charset=\"utf-8\"";
            request.Accept = "text/xml";
            request.Method = "POST";

            if (customHeaders != null)
            {
                foreach (var header in customHeaders)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            return request;
        }

        /// <summary>
        /// Parses a SOAP response to extract the desired content.
        /// </summary>
        public static string ExtractSoapResponseContent(string soapResponse, string startTag, string endTag)
        {
            if (soapResponse == null || startTag == null)
            {
                return String.Empty;
            }

            var startIndex = soapResponse.IndexOf(startTag, StringComparison.Ordinal);
            if (startIndex < 0)
            {
                return string.Empty;
            }

            startIndex += startTag.Length;
            var endIndex = soapResponse.IndexOf(endTag, startIndex, StringComparison.Ordinal);
            return endIndex < 0 ? string.Empty : soapResponse.Substring(startIndex, endIndex - startIndex).Trim();
        }

        /// <summary>
        /// Executes a SOAP function on a specified URI with the given parameters.
        /// </summary>
        /// <param name="timeout">The timeout for the request in milliseconds.</param>
        /// <param name="uri">The URI of the SOAP service.</param>
        /// <param name="serviceId">The service ID for the SOAP action.</param>
        /// <param name="function">The function name to invoke.</param>
        /// <param name="result">The name of the result tag to extract from the response, or null if no extraction is needed.</param>
        /// <param name="parameters">The parameters to pass to the SOAP function.</param>
        /// <returns>The extracted result if a result tag is specified; otherwise, the full SOAP response.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="uri"/>, <paramref name="serviceId"/>, or <paramref name="function"/> is null or empty.</exception>
        /// <example>
        /// Example usage:
        /// <code>
        /// var parameters = new[]
        /// {
        ///     new Parameter { Name = "Parameter1", Value = "Value1" },
        ///     new Parameter { Name = "Parameter2", Value = "Value2" }
        /// };
        ///
        /// string result = SoapService.ExecuteFunction(
        ///     timeout: 5000,
        ///     uri: new Uri("http://192.168.1.100:8080/control/MyService"),
        ///     serviceId: "urn:schemas-upnp-org:service:MyService:1",
        ///     function: "MyFunction",
        ///     result: "MyResultTag",
        ///     parameters: parameters
        /// );
        /// </code>
        /// </example>
        public static string ExecuteFunction(Uri uri, string serviceId, string function, string result, int timeout = 30000, params SoapParameter[] parameters)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            if (String.IsNullOrWhiteSpace(serviceId))
            {
                throw new ArgumentNullException(nameof(serviceId));
            }

            if (String.IsNullOrWhiteSpace(function))
            {
                throw new ArgumentNullException(nameof(function));
            }

            var action = $"{serviceId}#{function}";

            var soapEnvelope = new XmlDocument();
            var xml = new StringBuilder();
            _ = xml.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>")
                .Append("<s:Envelope s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\">")
                .Append("<s:Body>");

            if (parameters != null && parameters.Length > 0)
            {
                _ = xml.Append($"<u:{function} xmlns:u=\"{serviceId}\">");
                foreach (var parameter in parameters)
                {
                    _ = xml.Append($"<{parameter.Name}>{parameter.Value}</{parameter.Name}>");
                }
                _ = xml.Append($"</u:{function}>");
            }
            else
            {
                _ = xml.Append($"<u:{function} xmlns:u=\"{serviceId}\" />");
            }
            _ = xml.Append("</s:Body>")
                .Append("</s:Envelope>");

            soapEnvelope.LoadXml(xml.ToString());

            var webRequest = (HttpWebRequest)WebRequest.Create(uri);
            webRequest.Timeout = timeout;
            webRequest.Headers.Add("SOAPACTION", action);
            webRequest.ContentType = "text/xml; charset=\"utf-8\"";
            webRequest.Method = "POST";
            webRequest.ServicePoint.Expect100Continue = false;

            using (var stream = webRequest.GetRequestStream())
            {
                soapEnvelope.Save(stream);
            }

            var asyncResult = webRequest.BeginGetResponse(null, null);
            _ = asyncResult.AsyncWaitHandle.WaitOne();

            string soapResult;
            using (var webResponse = webRequest.EndGetResponse(asyncResult))
            using (var reader = new StreamReader(webResponse.GetResponseStream() ?? Stream.Null))
            {
                soapResult = reader.ReadToEnd();
            }

            return String.IsNullOrWhiteSpace(result) ? null : ExtractResult(soapResult, result);
        }

        private static string ExtractResult(string soapResult, string resultTag)
        {
            var startTag = $"<{resultTag}>";
            var endTag = $"</{resultTag}>";
            var startIndex = soapResult.IndexOf(startTag, StringComparison.Ordinal);
            if (startIndex < 0)
            {
                return String.Empty;
            }

            startIndex += startTag.Length;
            var endIndex = soapResult.IndexOf(endTag, startIndex, StringComparison.Ordinal);
            return endIndex < 0 ? String.Empty : soapResult.Substring(startIndex, endIndex - startIndex).Trim();
        }
    }
}
