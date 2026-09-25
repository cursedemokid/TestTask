public class RequestModel
{
    public string Selector { get; set; } = string.Empty;
    public string Attribute { get; set; } = string.Empty;
    public string UrlB64 { get; set; } = string.Empty;
    public string EncryptedTextBytesB64 { get; set; } = string.Empty;
    public string KeyBytesB64 { get; set; } = string.Empty;
    public string PageB64 { get; set; } = string.Empty;
}