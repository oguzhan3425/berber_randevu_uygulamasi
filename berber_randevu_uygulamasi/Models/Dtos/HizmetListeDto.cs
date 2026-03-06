namespace berber_randevu_uygulamasi.Models.Dtos
{
    public class HizmetListeDto
    {
        public int HizmetID { get; set; }
        public string? HizmetAdi { get; set; }
        public decimal Fiyat { get; set; }
        public int SureDakika { get; set; }
    }
}