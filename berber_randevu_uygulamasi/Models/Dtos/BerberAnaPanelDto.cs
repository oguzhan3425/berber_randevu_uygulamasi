namespace berber_randevu_uygulamasi.Models.Dtos
{
    public class BerberAnaPanelDto
    {
        public string AdSoyad { get; set; } = "";
        public string ProfilFoto { get; set; } = "";
        public int BugunRandevuSayisi { get; set; }
        public string SiradakiMusteriAdSoyad { get; set; } = "";
        public string SiradakiSaat { get; set; } = "";
    }
}