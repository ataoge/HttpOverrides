﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Ataoge.AspNetCore.HttpOverrides
{
    /// <summary>
    /// Default values related to <see cref="ForwardedHeadersMiddleware"/> middleware
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Builder.AtaogeForwardedHeadersOptions"/>
    public static class ForwardedHeadersDefaults
    {
        /// <summary>
        /// X-Forwarded-For
        /// </summary>
        public static string XForwardedForHeaderName { get; } = "X-Forwarded-For";

        /// <summary>
        /// X-Forwarded-Host
        /// </summary>
        public static string XForwardedHostHeaderName { get; } = "X-Forwarded-Host";

        /// <summary>
        /// X-Forwarded-Proto
        /// </summary>
        public static string XForwardedProtoHeaderName { get; } = "X-Forwarded-Proto";


        /// <summary>
        /// X-Forwarded-Path
        /// </summary>
        public static string XForwardedPathHeaderName { get; } = "X-Forwarded-PathBase";

        /// <summary>
        /// X-Original-For
        /// </summary>
        public static string XOriginalForHeaderName { get; } = "X-Original-For";

        /// <summary>
        /// X-Original-Host
        /// </summary>
        public static string XOriginalHostHeaderName { get; } = "X-Original-Host";

        /// <summary>
        /// X-Original-Proto
        /// </summary>
        public static string XOriginalProtoHeaderName { get; } = "X-Original-Proto";

        /// <summary>
        /// X-Original-Path
        /// </summary>
        public static string XOriginalPathHeaderName { get; } = "X-Original-PathBase";
    }
}
