namespace berber_randevu_uygulamasi.Models.Dtos
{
    public class BerberListeDto
    {
        public int BerberId { get; set; }
        public string? BerberAdi { get; set; }
        public string? Adres { get; set; }
        public string? Telefon { get; set; }
        public string? DukkanFotoUrl { get; set; }
        public decimal Puan { get; set; }
        public string? Acilis { get; set; }
        public string? Kapanis { get; set; }
    }
}