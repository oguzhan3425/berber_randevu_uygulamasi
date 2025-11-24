namespace berber_randevu_uygulamasi.Models
{
    public class Berber
    {
        public int BerberID { get; set; }
        public string BerberAdi { get; set; } = string.Empty;
        public string Adres { get; set; } = string.Empty;
        public string Telefon { get; set; } = string.Empty;
        public string ResimYolu { get; set; } = string.Empty;
        public decimal Puan { get; set; }
        public TimeSpan AcilisSaati { get; set; }
        public TimeSpan KapanisSaati { get; set; }
    }
}
