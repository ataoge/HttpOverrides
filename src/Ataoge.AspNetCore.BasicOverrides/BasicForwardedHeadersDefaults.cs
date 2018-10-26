namespace Ataoge.AspNetCore.BasicOverrides
{
    /// <summary>
    /// BasicForwardedHeadersDefaults
    /// </summary>
    public class BasicForwardedHeadersDefaults
    {
        /// <summary>
        /// X-Forwarded-For
        /// </summary>

        public static string XForwardedForHeaderName { get; } = "X-Forwarded-For";

        /// <summary>
        /// X-Forwarded-Path
        /// </summary>
        public static string XForwardedPathBaseHeaderName { get; } = "X-Forwarded-PathBase";

        /// <summary>
        /// X-Forwarded-BaseUrl
        /// </summary>
        public static string XForwardedBaseUrlHeaderName {get;} = "X-Forwarded-BaseUrl";

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