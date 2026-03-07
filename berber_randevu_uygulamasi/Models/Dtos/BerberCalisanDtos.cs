namespace berber_randevu_uygulamasi.Models.Dtos
{
    public class BerberCalisanDto
    {
        public int CalisanID { get; set; }
        public int BerberID { get; set; }
        public int KullaniciID { get; set; }

        public string Ad { get; set; } = "";
        public string Soyad { get; set; } = "";
        public string Telefon { get; set; } = "";
        public string Foto { get; set; } = "";

        public string Rol { get; set; } = "Çalışan";
        public bool Aktif { get; set; } = true;
    }

    public class BerberCalisanListeResponse
    {
        public int BerberID { get; set; }
        public List<BerberCalisanDto> Calisanlar { get; set; } = new();
    }

    public class CalisanSilRequest
    {
        public int BerberID { get; set; }
        public int KullaniciID { get; set; }
    }

    public class CalisanSilResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }
}