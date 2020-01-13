using System;
using Ataoge.AspNetCore.BasicOverrides;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// BasicForwardedHeadersExtensions
    /// </summary>
    public static class BasicForwardedHeadersExtensions
    {
        /// <summary>
        /// Forwards proxied headers onto current request
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseBasicForwardedHeaders(this IApplicationBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.UseMiddleware<BasicForwardedHeadersMiddleware>();
        }

        /// <summary>
        /// Forwards proxied headers onto current request
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="options">Enables the different forwarding options.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseBasicForwardedHeaders(this IApplicationBuilder builder, BasicForwardedHeadersOptions options)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            

            builder.UseMiddleware<BasicForwardedHeadersMiddleware>(Options.Create(options));

            if ((options.ForwardedHeaders & BasicForwardedHeaders.IntranetPenetration) == BasicForwardedHeaders.IntranetPenetration)
            {
                builder.MapWhen(context => context.Request.Path.StartsWithSegments("/tunnel"),
                    appBuilder => appBuilder.UseMiddleware<GernicSockerMiddleware>());
            }

            return builder;
        }
    }
}