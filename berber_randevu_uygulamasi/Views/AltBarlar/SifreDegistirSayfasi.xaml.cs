using System;
using System.Threading.Tasks;
using berber_randevu_uygulamasi.Services;

namespace berber_randevu_uygulamasi.Views.AltBarlar
{
    public partial class SifreDegistirSayfasi : ContentPage
    {
        protected readonly ApiClient _api;
        private readonly bool _showBottomBar;

        public SifreDegistirSayfasi(ApiClient api, bool showBottomBar = false)
        {
            InitializeComponent();
            _api = api;
            _showBottomBar = showBottomBar;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_showBottomBar)
                await AltBarAyarlaAsync();
            else
                BottomBarHost.IsVisible = false;
        }

        private async Task AltBarAyarlaAsync()
        {
            BottomBarHost.IsVisible = true;

            var tip = (UserSession.KullaniciTipi ?? "").Trim();

            if (string.IsNullOrWhiteSpace(tip))
            {
                var result = await _api.KullaniciTipGetirAsync(UserSession.KullaniciId);
                tip = result.KullaniciTipi ?? "";
                UserSession.KullaniciTipi = tip;
            }

            bool isCalisan = tip.Equals("Calisan", StringComparison.OrdinalIgnoreCase);
            bool isMusteri = tip.Equals("Musteri", StringComparison.OrdinalIgnoreCase);

            barCalisan.IsVisible = isCalisan && !isMusteri;
            barBerber.IsVisible = !isCalisan && !isMusteri;
        }

        private async void GeriDon_Clicked(object sender, EventArgs e)
        {
            if (Navigation.NavigationStack.Count > 1)
            {
                await Navigation.PopAsync();
                return;
            }

            if (Navigation.ModalStack.Count > 0)
            {
                await Navigation.PopModalAsync();
                return;
            }

            await DisplayAlert("Bilgi", "Geri dönülecek sayfa yok.", "Tamam");
        }

        private async void SifreGuncelle_Clicked(object sender, EventArgs e)
        {
            string guncel = GuncelSifreEntry.Text ?? "";
            string yeni = YeniSifreEntry.Text ?? "";
            string yeniTekrar = YeniSifreTekrarEntry.Text ?? "";

            if (string.IsNullOrWhiteSpace(guncel) ||
                string.IsNullOrWhiteSpace(yeni) ||
                string.IsNullOrWhiteSpace(yeniTekrar))
            {
                await DisplayAlert("Uyarý", "Lütfen tüm alanlarý doldurun.", "Tamam");
                return;
            }

            if (yeni != yeniTekrar)
            {
                await DisplayAlert("Uyarý", "Yeni þifreler eþleþmiyor.", "Tamam");
                return;
            }

            if (yeni.Length < 6)
            {
                await DisplayAlert("Uyarý", "Yeni þifre en az 6 karakter olmalý.", "Tamam");
                return;
            }

            try
            {
                var result = await _api.SifreDegistirAsync(UserSession.KullaniciId, guncel, yeni);

                if (!result.Success)
                {
                    await DisplayAlert("Hata", result.Message, "Tamam");
                    return;
                }

                await DisplayAlert("Baþarýlý", result.Message, "Tamam");
                await GeriDonGuvenliAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", ex.Message, "Tamam");
            }
        }

        private async Task GeriDonGuvenliAsync()
        {
            if (Navigation.NavigationStack.Count > 1)
                await Navigation.PopAsync();
            else if (Navigation.ModalStack.Count > 0)
                await Navigation.PopModalAsync();
        }
    }
}