namespace berber_randevu_uygulamasi.Models.Dtos
{
    public class BerberRandevuDto
    {
        public int RandevuID { get; set; }
        public string TarihText { get; set; } = "";
        public string SaatText { get; set; } = "";
        public string MusteriAdSoyad { get; set; } = "";
        public string MusteriFoto { get; set; } = "";
        public string HizmetAdi { get; set; } = "";
        public string DurumText { get; set; } = "";
        public decimal ToplamUcret { get; set; }
    }
}