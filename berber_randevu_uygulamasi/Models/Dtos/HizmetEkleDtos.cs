namespace berber_randevu_uygulamasi.Models.Dtos
{
    public class HizmetEkleRequest
    {
        public string HizmetAdi { get; set; } = "";
        public decimal Fiyat { get; set; }
        public int SureDakika { get; set; }
        public int BerberID { get; set; }
        public int CalisanID { get; set; }
    }

    public class HizmetEkleResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }
}