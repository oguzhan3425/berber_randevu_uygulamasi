namespace berber_randevu_uygulamasi.Models.Dtos
{
    public class TelefonGuncelleRequest
    {
        public int KullaniciId { get; set; }
        public string Telefon { get; set; } = "";
    }

    public class TelefonGuncelleResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }
}