using System.Text.Json.Serialization;

namespace TestTask.Models; 

public class RequestModel
{
    [JsonPropertyName("selector")]
    public string Selector { get; set; } = string.Empty;

    [JsonPropertyName("attribute")]
    public string Attribute { get; set; } = string.Empty;

    [JsonPropertyName("url_b64")]
    public string UrlB64 { get; set; } = string.Empty;

    [JsonPropertyName("encrypted_text_bytes_b64")]
    public string EncryptedTextBytesB64 { get; set; } = string.Empty;

    [JsonPropertyName("key_bytes_b64")]
    public string KeyBytesB64 { get; set; } = string.Empty;

    [JsonPropertyName("page_b64")]
    public string PageB64 { get; set; } = string.Empty;
}