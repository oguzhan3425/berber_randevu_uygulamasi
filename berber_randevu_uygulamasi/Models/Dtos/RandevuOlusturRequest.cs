namespace berber_randevu_uygulamasi.Models.Dtos
{
    public class RandevuOlusturRequest
    {
        public int KullaniciId { get; set; }
        public int BerberId { get; set; }
        public int CalisanId { get; set; }
        public int HizmetId { get; set; }
        public string? RandevuTarihi { get; set; }   // "2026-03-06"
        public string? RandevuSaati { get; set; }    // "14:30"
        public int SureDakika { get; set; }
        public decimal ToplamUcret { get; set; }
    }
}