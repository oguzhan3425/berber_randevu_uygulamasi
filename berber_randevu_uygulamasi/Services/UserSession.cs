namespace berber_randevu_uygulamasi.Services
{
    public static class UserSession
    {
        public static int KullaniciId { get; set; }
        public static string Ad { get; set; } = "";
        public static string Soyad { get; set; } = "";
        public static string KullaniciTipi { get; set; } = "";

        public static string Telefon { get; set; } = ""; 

        public static bool IsLoggedIn => KullaniciId > 0;

        public static void Clear()
        {
            KullaniciId = 0;
            Ad = "";
            Soyad = "";
            KullaniciTipi = "";
        }
    }
}