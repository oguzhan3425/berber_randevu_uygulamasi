namespace berber_randevu_uygulamasi.Models
{
    public class Hizmet
    {
        public int HizmetID { get; set; }
        public int CalisanID { get; set; }
        public string HizmetAdi { get; set; } = string.Empty;
        public decimal Fiyat { get; set; }
        public int SureDakika { get; set; }
        public string Durum { get; set; } = string.Empty;
    }
}