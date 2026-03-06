using berber_randevu_uygulamasi.Services;
using berber_randevu_uygulamasi.Views.AltBarlar;
using berber_randevu_uygulamasi.Models.Dtos;
using Microsoft.Maui.Controls;
using System;

namespace berber_randevu_uygulamasi.Views
{
    public partial class ProfilDuzenleSayfasi : ContentPage
    {
        protected readonly ApiClient _api;

        public ProfilDuzenleSayfasi(ApiClient api)
        {
            InitializeComponent();
            _api = api;

            // Ýlk açýlýþta isim yaz
            if (this.FindByName<Label>("lblAdSoyad") is Label lbl)
                lbl.Text = $"{UserSession.Ad} {UserSession.Soyad}".Trim();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Geri dönünce foto yenilensin
            await KullaniciBilgileriniYukleAsync();
        }

        private async System.Threading.Tasks.Task KullaniciBilgileriniYukleAsync()
        {
            try
            {
                var user = await _api.GetAsync<UserProfileDto>($"users/{UserSession.KullaniciId}/profile");

                var fotoUrl = (user?.ProfilFotoUrl ?? "").Trim();

                if (!string.IsNullOrWhiteSpace(fotoUrl) &&
                    Uri.TryCreate(fotoUrl, UriKind.Absolute, out var uri))
                {
                    imgProfil.Source = ImageSource.FromUri(uri);
                }
                else
                {
                    imgProfil.Source = "default_user.png";
                }

                // Ýsim/telefon vs. istersen burada da güncelleyebilirsin:
                // lblAdSoyad.Text = $"{user?.Ad} {user?.Soyad}".Trim();
            }
            catch (Exception ex)
            {
                imgProfil.Source = "default_user.png";
                await DisplayAlert("Hata", ex.Message, "Tamam");
            }
        }

        // Alt bar - Butonlar
        private async void AnaSayfaClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AnaSayfa(_api));
        }

        private async void RandevularClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RandevuAlSayfasi(_api));
        }

        private async void ProfilClicked(object sender, EventArgs e)
        {
            await System.Threading.Tasks.Task.CompletedTask;
        }

        // Ayarlar
        private async void SifreDegistir_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SifreDegistirSayfasi(_api));
        }

        private async void ProfilFotoDegistir_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ProfilFotoDegistirSayfasi(
                _api,
                FotoHedefi.SahipProfilFoto,
                UserSession.KullaniciId
            ));
        }

        private async void TelefonDegistir_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new TelefonDegistirSayfasi(_api));
        }
    }
}