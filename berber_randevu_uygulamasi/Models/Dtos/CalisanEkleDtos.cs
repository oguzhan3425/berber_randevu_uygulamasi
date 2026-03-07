namespace berber_randevu_uygulamasi.Models.Dtos
{
    public class CalisanAdayDto
    {
        public int KullaniciID { get; set; }
        public string Ad { get; set; } = "";
        public string Soyad { get; set; } = "";
        public string Telefon { get; set; } = "";
        public string KullaniciTipi { get; set; } = "";
    }

    public class CalisanEkleRequest
    {
        public int BerberID { get; set; }
        public int KullaniciID { get; set; }
    }

    public class CalisanEkleResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }
}