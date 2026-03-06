namespace berber_randevu_uygulamasi.Models.Dtos
{
    public class CalisanListeDto
    {
        public int CalisanID { get; set; }
        public int BerberID { get; set; }
        public string? Ad { get; set; }
        public string? Soyad { get; set; }
        public string? FotoUrl { get; set; }
        public string? Uzmanlik { get; set; }
    }
}