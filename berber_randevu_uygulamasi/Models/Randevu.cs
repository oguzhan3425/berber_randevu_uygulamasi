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


        // ======================================================
        // 🔥 TASARIMIN İSTEDİĞİ ALANLAR (XAML BUNLARA BİND EDİYOR)
        // ======================================================

        // UI için string formatlı saat (ör: "14:30")
        public string RandevuSaatiText
            => RandevuSaati.ToString(@"hh\:mm");

        // UI’da gösterilecek hizmet adı
        public string HizmetAdi { get; set; } = string.Empty;

        // Randevuyu alan kişinin adı + soyadı
        public string MusteriAdi { get; set; } = string.Empty;

        // Görsel için gerekli fotoğraf yolu
        public string MusteriFoto { get; set; } = "default_user.png";
    }
}
