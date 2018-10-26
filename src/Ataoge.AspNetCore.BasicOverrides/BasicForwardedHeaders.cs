using System;

namespace Ataoge.AspNetCore.BasicOverrides
{

    /// <summary>
    /// BasicForwardedHeaders
    /// </summary>
    [Flags]
    public enum BasicForwardedHeaders
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,
        /// <summary>
        /// XForwardedPathBase
        /// </summary>
        XForwardedPathBase = 1 << 0,
        /// <summary>
        /// XForwardedBaseUrl
        /// </summary>
        XForwardedBaseUrl = 1 << 1,
        /// <summary>
        /// IntranetPenetration
        /// </summary>
        IntranetPenetration = 1 << 2,
        /// <summary>
        /// All
        /// </summary>
        All = XForwardedPathBase | XForwardedBaseUrl
    }
    
}