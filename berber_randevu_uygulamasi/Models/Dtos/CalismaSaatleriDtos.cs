namespace berber_randevu_uygulamasi.Models.Dtos
{
    public class CalismaSaatiGunDto
    {
        public short Gun { get; set; }
        public bool AcikMi { get; set; }
        public string Acilis { get; set; } = "";
        public string Kapanis { get; set; } = "";
    }

    public class CalismaSaatleriGetResponse
    {
        public int CalisanID { get; set; }
        public List<CalismaSaatiGunDto> Gunler { get; set; } = new();
    }

    public class CalismaSaatleriKaydetRequest
    {
        public int KullaniciId { get; set; }
        public List<CalismaSaatiGunDto> Gunler { get; set; } = new();
    }

    public class CalismaSaatleriKaydetResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }
}