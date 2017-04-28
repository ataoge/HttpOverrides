﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Ataoge.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Builder
{
    public static class AtaogeForwardedHeadersExtensions
    {
        /// <summary>
        /// Forwards proxied headers onto current request
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseAtaogeForwardedHeaders(this IApplicationBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.UseMiddleware<ForwardedHeadersMiddleware>();
        }

        /// <summary>
        /// Forwards proxied headers onto current request
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="options">Enables the different forwarding options.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseAtaogeForwardedHeaders(this IApplicationBuilder builder, AtaogeForwardedHeadersOptions options)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return builder.UseMiddleware<ForwardedHeadersMiddleware>(Options.Create(options));
        }
    }
}
