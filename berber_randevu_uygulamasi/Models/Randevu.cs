namespace berber_randevu_uygulamasi.Models
{
    public class Randevu
    {
        // 🔥 Mevcut veritabanı alanları (dokunmadım)
        public int RandevuID { get; set; }
        public int KullaniciID { get; set; }
        public int BerberID { get; set; }
        public int CalisanID { get; set; }
        public int HizmetID { get; set; }
        public DateTime RandevuTarihi { get; set; }
        public TimeSpan RandevuSaati { get; set; }
        public int SureDakika { get; set; }
        public decimal ToplamUcret { get; set; }
        public string MusteriFoto { get; set; } = "default_berber.png";
        public int RandevuAraligi { get; set; } // Yeni alan: Randevu aralığı (dakika cinsinden)
    }
}
