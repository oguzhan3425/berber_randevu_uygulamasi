namespace berber_randevu_uygulamasi.Models
{
    public class Calisan
    {
        public int CalisanID { get; set; }
        public int KullaniciID { get; set; }
        public int BerberID { get; set; }

        public string Uzmanlik { get; set; } = string.Empty;
        
        // Kullanıcı bilgileri
        public string Ad { get; set; } = string.Empty;
        public string Soyad { get; set; } = string.Empty;
        public string Foto { get; set; } = "default_berber.png";
    }
}
