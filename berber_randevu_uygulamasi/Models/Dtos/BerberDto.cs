namespace berber_randevu_uygulamasi.Models.Dtos;

public class BerberDto
{
    public int BerberId { get; set; }
    public string? BerberAdi { get; set; }
    public string? Adres { get; set; }
    public string? Acilis { get; set; }  // "09:00"
    public string? Kapanis { get; set; } // "21:00"
    public string? DukkanFotoUrl { get; set; }
}