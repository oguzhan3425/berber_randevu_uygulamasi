namespace berber_randevu_uygulamasi.Models.Dtos
{
    public class CalisanHizmetDto
    {
        public int HizmetID { get; set; }
        public int CalisanID { get; set; }
        public string HizmetAdi { get; set; } = "";
        public decimal Fiyat { get; set; }
        public int SureDakika { get; set; }
        public int BerberID { get; set; }
        public bool Aktif { get; set; }
    }

    public class CalisanHizmetListeResponse
    {
        public int CalisanID { get; set; }
        public int BerberID { get; set; }
        public List<CalisanHizmetDto> Hizmetler { get; set; } = new();
    }

    public class HizmetAktifPasifRequest
    {
        public int HizmetID { get; set; }
    }

    public class HizmetAktifPasifResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public bool Aktif { get; set; }
    }
}