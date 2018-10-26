using System.Collections.Generic;
using System.Net;
using Ataoge.AspNetCore.BasicOverrides;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// BasicForwardedHeadersOptions
    /// </summary>
    public class BasicForwardedHeadersOptions
    {
        private string _originalProtoHeaderName = BasicForwardedHeadersDefaults.XOriginalProtoHeaderName;

        /// <summary>
        /// Use this header instead of <see cref="BasicForwardedHeadersDefaults.XForwardedPathBaseHeaderName"/>
        /// </summary>
        /// <seealso cref="BasicForwardedHeadersDefaults"/>
        public string ForwardedPathBaseHeaderName { get; set; } = BasicForwardedHeadersDefaults.XForwardedPathBaseHeaderName;

        /// <summary>
        /// Use this header instead of <see cref="BasicForwardedHeadersDefaults.XForwardedBaseUrlHeaderName"/>
        /// </summary>
        /// <seealso cref="BasicForwardedHeadersDefaults"/>
        public string ForwardedBaseUrlHeaderName { get; set; } = BasicForwardedHeadersDefaults.XForwardedBaseUrlHeaderName;


        /// <summary>
        /// Use this header instead of <see cref="BasicForwardedHeadersDefaults.XOriginalForHeaderName"/>
        /// </summary>
        /// <seealso cref="BasicForwardedHeadersDefaults"/>
        public string OriginalForHeaderName { get; set; } = BasicForwardedHeadersDefaults.XOriginalForHeaderName;

        /// <summary>
        /// Use this header instead of <see cref="BasicForwardedHeadersDefaults.XOriginalHostHeaderName"/>
        /// </summary>
        /// <seealso cref="BasicForwardedHeadersDefaults"/>
        public string OriginalHostHeaderName { get; set; } = BasicForwardedHeadersDefaults.XOriginalHostHeaderName;

        /// <summary>
        /// Use this header instead of <see cref="BasicForwardedHeadersDefaults.XOriginalProtoHeaderName"/>
        /// </summary>
        /// <seealso cref="BasicForwardedHeadersDefaults"/>
        public string OriginalProtoHeaderName { get => _originalProtoHeaderName; set => _originalProtoHeaderName = value; }
        /// <summary>
        /// Use this header instead of <see cref="BasicForwardedHeadersDefaults.XOriginalPathHeaderName"/>
        /// </summary>
        /// <seealso cref="BasicForwardedHeadersDefaults"/>
        public string OriginalPathHeaderName { get; set; } = BasicForwardedHeadersDefaults.XOriginalPathHeaderName;

        /// <summary>
        /// Identifies which forwarders should be processed.
        /// </summary>
        public BasicForwardedHeaders ForwardedHeaders { get; set; }

        /// <summary>
        /// Limits the number of entries in the headers that will be processed. The default value is 1.
        /// Set to null to disable the limit, but this should only be done if
        /// KnownProxies or KnownNetworks are configured.
        /// </summary>
        public int? ForwardLimit { get; set; } = 1;

        /// <summary>
        /// Addresses of known proxies to accept forwarded headers from.
        /// </summary>
        public IList<IPAddress> KnownProxies { get; } = new List<IPAddress>();// { IPAddress.IPv6Loopback };

    }
}