namespace berber_randevu_uygulamasi.Models.Dtos
{
    public class RandevuCreateRequest
    {
        public int KullaniciId { get; set; }
        public int CalisanId { get; set; }
        public int HizmetId { get; set; }

        public string Tarih { get; set; } = "";
        public string Saat { get; set; } = "";
    }

    
}