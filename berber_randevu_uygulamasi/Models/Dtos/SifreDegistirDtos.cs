namespace berber_randevu_uygulamasi.Models.Dtos
{
    public class KullaniciTipDto
    {
        public string KullaniciTipi { get; set; } = "";
    }

    public class SifreDegistirRequest
    {
        public int KullaniciId { get; set; }
        public string GuncelSifre { get; set; } = "";
        public string YeniSifre { get; set; } = "";
    }

    public class SifreDegistirResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }
}