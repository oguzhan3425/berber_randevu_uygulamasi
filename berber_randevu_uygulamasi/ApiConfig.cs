namespace berber_randevu_uygulamasi.Services
{
    public sealed class ApiConfig
    {
        public string BaseUrl { get; init; } = "";
        public int TimeoutSeconds { get; init; } = 30;

        public static ApiConfig Current
        {
            get
            {
#if DEBUG
                return new ApiConfig
                {
                    // ✅ Gerçek telefon / aynı Wi-Fi
                    BaseUrl = "http://192.168.1.104:5077/api/",
                    TimeoutSeconds = 30
                };
#else
                return new ApiConfig
                {
                    // ✅ Yayında kullanılacak
                    BaseUrl = "https://api.senin-domainin.com/api/",
                    TimeoutSeconds = 30
                };
#endif
            }
        }
    }
}