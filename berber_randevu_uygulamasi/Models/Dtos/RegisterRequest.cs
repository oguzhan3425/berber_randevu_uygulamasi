namespace berber_randevu_uygulamasi.Models.Dtos;

public sealed class RegisterRequest
{
    public string Ad { get; set; } = "";
    public string Soyad { get; set; } = "";
    public string KullaniciAdi { get; set; } = "";
    public string Eposta { get; set; } = "";
    public string Sifre { get; set; } = "";
    public string Telefon { get; set; } = "";
}