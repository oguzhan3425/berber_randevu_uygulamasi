using System.Text.Json.Serialization;

namespace berber_randevu_uygulamasi.Models.Dtos;

public sealed class RegisterResponse
{
    [JsonPropertyName("basarili")]
    public bool Basarili { get; set; }

    [JsonPropertyName("mesaj")]
    public string Mesaj { get; set; } = "";
}