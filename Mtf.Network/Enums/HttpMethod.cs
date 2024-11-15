namespace Mtf.Network.Enums
{
    public enum HttpMethod
    {
        /// <summary>Converts the request connection to a transparent TCP/IP tunnel, usually to facilitate SSL-encrypted communication (HTTPS) through an unencrypted HTTP proxy.</summary>
        CONNECT,

        /// <summary>HTTP Debug method.</summary>
        DEBUG,

        /// <summary>Deletes the specified resource.</summary>
        DELETE,

        /// <summary>Requests a representation of the specified resource. Requests using GET (and a few other HTTP methods) "SHOULD NOT have the significance of taking an action other than retrieval". The W3C has published guidance principles on this distinction, saying, "Web application design should be informed by the above principles, but also by the relevant limitations." Safe method.</summary>
        GET,

        /// <summary>Asks for the response identical to the one that would correspond to a GET request, but without the response body. This is useful for retrieving meta-information written in response headers, without having to transport the entire content. Safe method.</summary>
        HEAD,

        /// <summary>Returns the HTTP methods that the server supports for specified URL. This can be used to check the functionality of a web server by requesting '*' instead of a specific resource. Safe method.</summary>
        OPTIONS,

        /// <summary>Is used to apply partial modifications to a resource.</summary>
        PATCH,

        /// <summary>Submits data to be processed (e.g., from an HTML form) to the identified resource. The data is included in the body of the request. This may result in the creation of a new resource or the updates of existing resources or both.</summary>
        POST,

        /// <summary>Uploads a representation of the specified resource.</summary>
        PUT,

        /// <summary>Echoes back the received request, so that a client can see what (if any) changes or additions have been made by intermediate servers. Safe method.</summary>
        TRACE,

        /// <summary>HTTP Track method.</summary>
        TRACK
    }
}
