namespace Reusable.Data
{
    public abstract class MimeType 
    {
        
        /// <summary>
        /// Any document that contains text and is theoretically human readable
        /// </summary>
        public static readonly Option<MimeType> Plain = Option<MimeType>.CreateWithCallerName("text/plain");
        
        public static readonly Option<MimeType> Html = Option<MimeType>.CreateWithCallerName("text/html");

        public static readonly Option<MimeType> Json = Option<MimeType>.CreateWithCallerName("application/json");

        /// <summary>
        /// Any kind of binary data, especially data that will be executed or interpreted somehow.
        /// </summary>
        public static readonly Option<MimeType> Binary = Option<MimeType>.CreateWithCallerName("application/octet-stream");
    }
}