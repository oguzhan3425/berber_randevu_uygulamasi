using berber_randevu_uygulamasi.Services;
using berber_randevu_uygulamasi.Models.Dtos;

namespace berber_randevu_uygulamasi.Views
{
    public partial class GirisSayfasi : ContentPage
    {
        protected readonly ApiClient _api;

        public GirisSayfasi(ApiClient api)
        {
            InitializeComponent();
            _api = api;
        }

        private async void GirisYap_Clicked(object sender, EventArgs e)
        {

           // var r = await _api.GetStringAsync("ping");
            //await DisplayAlert("Ping", r, "OK");
            string kadi = kullaniciAdiEntry.Text?.Trim() ?? "";
            string sifre = SifreEntry.Text ?? "";
            var dbg = await _api.PostJsonDebugAsync("auth/login", new { KullaniciAdi = kadi, Sifre = sifre });
          //  await DisplayAlert("LOGIN", $"status: {dbg.status}\nbody: {dbg.body}", "OK");
           // await DisplayAlert("TEST", $"kadi='{kadi}' sifreLen={sifre.Length}", "OK");
            var req = new LoginRequest { KullaniciAdi = kadi, Sifre = sifre };
            var resp = await _api.PostJsonAsync<LoginRequest, LoginResponse>("auth/login", req);

            if (resp == null)
            {
                await DisplayAlert("Hata", "Sunucuya ulaşılamadı.", "Tamam");
                return;
            }

            if (!resp.Basarili)
            {
                await DisplayAlert("Hata", resp.Mesaj, "Tekrar Dene");
                return;
            }

            // ✅ Session doldur
            UserSession.KullaniciId = resp.Id;
            UserSession.Ad = resp.Ad;
            UserSession.Soyad = resp.Soyad;
            UserSession.KullaniciTipi = resp.KullaniciTipi;
            UserSession.Telefon = resp.Telefon;

            // ✅ Rol bazlı yönlendirme (aynı mantık)
            if (resp.KullaniciTipi == "Berber")
            {
                await Navigation.PushAsync(new BerberAnaPanelSayfasi(_api));
                return;
            }

            if (resp.KullaniciTipi == "Calisan")
            {
                await Navigation.PushAsync(new CalisanAnaSayfa(_api));
                return;
            }

            if (resp.KullaniciTipi == "Musteri")
            {
                await Navigation.PushAsync(new AnaSayfa(_api));
                return;
            }

            await DisplayAlert("Hata", "Kullanıcı tipi tanımlanmamış.", "Tamam");

        }

        private async void KayitOl_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new KayitOlSayfasi(_api));
        }

        private void KullaniciAdiEntry_Completed(object sender, EventArgs e)
        {
            SifreEntry.Focus();
        }

        private void SifreEntry_Completed(object sender, EventArgs e)
        {
            GirisYap_Clicked(sender, e);
        }
    }
}