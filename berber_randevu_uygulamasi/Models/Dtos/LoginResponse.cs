using System.Text.Json.Serialization;

namespace berber_randevu_uygulamasi.Models.Dtos;

public sealed class LoginResponse
{
    [JsonPropertyName("basarili")]
    public bool Basarili { get; set; }

    [JsonPropertyName("mesaj")]
    public string Mesaj { get; set; } = "";

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("ad")]
    public string Ad { get; set; } = "";

    [JsonPropertyName("soyad")]
    public string Soyad { get; set; } = "";

    [JsonPropertyName("telefon")]
    public string Telefon { get; set; } = "";

    [JsonPropertyName("kullaniciTipi")]
    public string KullaniciTipi { get; set; } = "";
}