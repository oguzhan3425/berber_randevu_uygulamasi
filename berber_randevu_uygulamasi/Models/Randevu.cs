namespace berber_randevu_uygulamasi.Models
{
    public class Randevu
    {
        public int RandevuID { get; set; }
        public int KullaniciID { get; set; }
        public int BerberID { get; set; }
        public int CalisanID { get; set; }
        public int HizmetID { get; set; }
        public DateTime RandevuTarihi { get; set; }
        public TimeSpan RandevuSaati { get; set; }
        public int SureDakika { get; set; }
        public decimal ToplamUcret { get; set; }
    }
}
