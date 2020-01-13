using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ataoge.AspNetCore.BasicOverrides
{
    /// <summary>
    /// BasicForwardedHeadersMiddleware
    /// </summary>
    public class BasicForwardedHeadersMiddleware
    {
        private readonly BasicForwardedHeadersOptions _options;
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        /// <summary>
        /// BasicForwardedHeadersMiddleware
        /// </summary>
        public BasicForwardedHeadersMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, IOptions<BasicForwardedHeadersOptions> options)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            // Make sure required options is not null or whitespace
            EnsureOptionNotNullorWhitespace(options.Value.ForwardedPathBaseHeaderName, nameof(options.Value.ForwardedPathBaseHeaderName));
            EnsureOptionNotNullorWhitespace(options.Value.ForwardedBaseUrlHeaderName, nameof(options.Value.ForwardedBaseUrlHeaderName));
            EnsureOptionNotNullorWhitespace(options.Value.OriginalPathHeaderName, nameof(options.Value.OriginalPathHeaderName));
            EnsureOptionNotNullorWhitespace(options.Value.OriginalForHeaderName, nameof(options.Value.OriginalForHeaderName));
            EnsureOptionNotNullorWhitespace(options.Value.OriginalHostHeaderName, nameof(options.Value.OriginalHostHeaderName));
            EnsureOptionNotNullorWhitespace(options.Value.OriginalProtoHeaderName, nameof(options.Value.OriginalProtoHeaderName));

            _options = options.Value;
            _logger = loggerFactory.CreateLogger<BasicForwardedHeadersMiddleware>();
            _next = next;

            //PreProcessHosts();
        }

        private static void EnsureOptionNotNullorWhitespace(string value, string propertyName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"options.{propertyName} is required", "options");
            }
        }

        /// <summary>
        /// Invoke
        /// </summary>
        public Task Invoke(HttpContext context)
        {
            ApplyForwarders(context);
            return _next(context);
        }

        /// <summary>
        /// ApplyForwarders
        /// </summary>
        public void ApplyForwarders(HttpContext context)
        {
            // Gather expected headers.
            string[] forwardedFor = null, forwardedPathBase = null, forwardedUrlBase = null;
            bool chenckPathBase = false, checkUrlBase = false;
            int entryCount = 0;

            
            forwardedFor = context.Request.Headers.GetCommaSeparatedValues(BasicForwardedHeadersDefaults.XForwardedForHeaderName);
            if (forwardedFor.Length == 0)  // 已经有一个代理
            {
                forwardedFor = context.Request.Headers.GetCommaSeparatedValues(BasicForwardedHeadersDefaults.XOriginalForHeaderName);
            }
            if ((_options.ForwardedHeaders & BasicForwardedHeaders.XForwardedPathBase) == BasicForwardedHeaders.XForwardedPathBase)
            {
                chenckPathBase = true;
                forwardedPathBase = context.Request.Headers.GetCommaSeparatedValues(_options.ForwardedPathBaseHeaderName);
                entryCount = Math.Max(forwardedFor.Length, entryCount);
            }
             _logger.LogWarning(1, "Parameter aaa" + forwardedPathBase.FirstOrDefault());
            if ((_options.ForwardedHeaders & BasicForwardedHeaders.XForwardedBaseUrl) == BasicForwardedHeaders.XForwardedBaseUrl)
            {
                checkUrlBase = true;
                forwardedUrlBase = context.Request.Headers.GetCommaSeparatedValues(_options.ForwardedBaseUrlHeaderName);
                if (chenckPathBase && forwardedPathBase.Length != forwardedUrlBase.Length)
                {
                    _logger.LogWarning(1, "Parameter count mismatch between X-Forwarded-For and X-Forwarded-Proto.");
                    return;
                }
                entryCount = Math.Max(forwardedUrlBase.Length, entryCount);
            }

            // Apply ForwardLimit, if any
            if (_options.ForwardLimit.HasValue && entryCount > _options.ForwardLimit)
            {
                entryCount = _options.ForwardLimit.Value;
            }

            // Group the data together.
            var sets = new SetOfForwarders[entryCount];
            for (int i = 0; i < sets.Length; i++)
            {
                // They get processed in reverse order, right to left.
                var set = new SetOfForwarders();
                if (i < forwardedFor.Length)
                {
                    set.IpAndPortText = forwardedFor[forwardedFor.Length - i - 1];
                }
                if (chenckPathBase && i < forwardedPathBase.Length)
                {
                    set.PathBase = forwardedPathBase[forwardedPathBase.Length - i - 1];
                }
                if (checkUrlBase && i < forwardedUrlBase.Length)
                {
                    var baseUrlString = forwardedUrlBase[forwardedUrlBase.Length - i - 1];
                    var baseUri = new Uri(baseUrlString);
                    set.Host = baseUri.Host;
                    set.Scheme = baseUri.Scheme;
                    if (!chenckPathBase)
                    {
                        set.PathBase = baseUri.PathAndQuery;
                    }
                }
                sets[i] = set;
            }

            // Gather initial values
            var connection = context.Connection;
            var request = context.Request;
            var currentValues = new SetOfForwarders()
            {
                RemoteIpAndPort = connection.RemoteIpAddress != null ? new IPEndPoint(connection.RemoteIpAddress, connection.RemotePort) : null,
                // Host and Scheme initial values are never inspected, no need to set them here.
            };

            var checkKnownIps = _options.KnownProxies.Count > 0;
            bool applyChanges = false;
            int entriesConsumed = 0;

            for ( ; entriesConsumed < sets.Length; entriesConsumed++)
            {
                var set = sets[entriesConsumed];
                if (currentValues.RemoteIpAndPort != null && checkKnownIps && !CheckKnownAddress(currentValues.RemoteIpAndPort.Address))
                {
                    // Stop at the first unknown remote IP, but still apply changes processed so far.
                    _logger.LogDebug(1, "Unknown proxy: {RemoteIpAndPort}", currentValues.RemoteIpAndPort);
                    break;
                }

                if (chenckPathBase)
                {
                    if (!string.IsNullOrEmpty(set.PathBase))
                    {
                        applyChanges = true;
                        currentValues.PathBase = set.PathBase;
                    }
                    else 
                    {
                        _logger.LogWarning(3, $"Forwarded Pathbase is not present");
                        return;
                    }
                }

                if (checkUrlBase)
                {
                    applyChanges = true;
                    if (!string.IsNullOrEmpty(set.Host))
                    {
                        currentValues.Host = set.Host;
                    } 
                    if (!string.IsNullOrEmpty(set.Scheme))
                    {
                        currentValues.Scheme = set.Scheme;
                    }
                    if (!chenckPathBase && !string.IsNullOrEmpty(set.PathBase)) 
                    {
                        currentValues.PathBase = set.PathBase;
                    }
                }

            }

            if (applyChanges)
            {
                if (chenckPathBase && currentValues.PathBase != null)
                {
                    // Save the original
                    request.Headers[_options.OriginalPathHeaderName] = request.PathBase.ToString();
                    if (forwardedPathBase.Length > entriesConsumed)
                    {
                        // Truncate the consumed header values
                        request.Headers[_options.ForwardedPathBaseHeaderName] = forwardedPathBase.Take(forwardedPathBase.Length - entriesConsumed).ToArray();
                    }
                    else
                    {
                        // All values were consumed
                        request.Headers.Remove(_options.ForwardedPathBaseHeaderName);
                    }
                    request.PathBase = new PathString(currentValues.PathBase);
                }

                if (checkUrlBase)
                {
                    if (!request.Headers.ContainsKey(_options.OriginalHostHeaderName))
                    {
                        request.Headers[_options.OriginalHostHeaderName] = request.Host.ToString();
                    }
                    if (!string.IsNullOrEmpty(currentValues.Host) && !currentValues.Host.Equals(request.Host.ToString(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        request.Host = HostString.FromUriComponent(currentValues.Host);
                    }

                    if (!request.Headers.ContainsKey(_options.OriginalProtoHeaderName))
                    {
                        request.Headers[_options.OriginalProtoHeaderName] = request.Scheme;
                    }
                    if (!string.IsNullOrEmpty(currentValues.Scheme) && !currentValues.Scheme.Equals(request.Scheme, StringComparison.InvariantCultureIgnoreCase))
                    {
                        request.Scheme = currentValues.Scheme;
                    }

                    if (!chenckPathBase && !string.IsNullOrEmpty(currentValues.PathBase))
                    {
                        request.PathBase = new PathString(currentValues.PathBase);
                    }

                }

            }
        }

        private bool CheckKnownAddress(IPAddress address)
        {
            if (address.IsIPv4MappedToIPv6)
            {
                var ipv4Address = address.MapToIPv4();
                if (CheckKnownAddress(ipv4Address))
                {
                    return true;
                }
            }
            if (_options.KnownProxies.Contains(address))
            {
                return true;
            }
            /* foreach (var network in _options.KnownNetworks)
            {
                if (network.Contains(address))
                {
                    return true;
                }
            }*/
            return false;
        }

        private struct SetOfForwarders
        {
            public string IpAndPortText;
            public IPEndPoint RemoteIpAndPort;
            public string Host;
            public string Scheme;

            public string PathBase;
        }
    }
}