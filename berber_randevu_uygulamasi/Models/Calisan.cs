namespace berber_randevu_uygulamasi.Models
{
    public class Calisan
    {
        public int CalisanID { get; set; }
        public int BerberID { get; set; }
        public string AdSoyad { get; set; } = string.Empty;
        public int DeneyimYili { get; set; }
        public string ResimYolu { get; set; } = string.Empty;
    }
}
