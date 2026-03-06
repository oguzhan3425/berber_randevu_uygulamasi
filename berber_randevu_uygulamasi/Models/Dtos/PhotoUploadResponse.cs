using System.Text.Json.Serialization;

namespace berber_randevu_uygulamasi.Models.Dtos;

public sealed class PhotoUploadResponse
{
    [JsonPropertyName("basarili")]
    public bool Basarili { get; set; }

    [JsonPropertyName("mesaj")]
    public string Mesaj { get; set; } = "";

    [JsonPropertyName("yol")]
    public string Yol { get; set; } = "";
    public string? Path { get; set; }
    public string? Url { get; set; }
}