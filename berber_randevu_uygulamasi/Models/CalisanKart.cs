namespace berber_randevu_uygulamasi.Models
{
    public class CalisanKart
    {
        public string Ad { get; set; } = "";
        public string Soyad { get; set; } = "";
        public int CalisanID { get; set; }
        public int KullaniciID { get; set; }
        public int BerberID { get; set; }
        public string Uzmanlik { get; set; } = "";
        public string Foto { get; set; } = "default_berber.png";
        public string Rol { get; set; } = "";
        public string Telefon { get; set; } = "";
        public bool Aktif { get; set; }
        public string Durum => Aktif ? "Durum: Aktif" : "Durum: Pasif";
    }
}
