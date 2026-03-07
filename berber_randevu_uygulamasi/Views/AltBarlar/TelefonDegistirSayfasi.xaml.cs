using System;
using Microsoft.Maui.Controls;
using berber_randevu_uygulamasi.Services;

namespace berber_randevu_uygulamasi.Views.AltBarlar
{
    public partial class TelefonDegistirSayfasi : ContentPage
    {
        protected readonly ApiClient _api;

        public TelefonDegistirSayfasi(ApiClient api)
        {
            InitializeComponent();
            _api = api;
        }

        private async void TelefonGuncelle_Clicked(object sender, EventArgs e)
        {
            string tel = TelefonEntry.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(tel))
            {
                await DisplayAlert("Uyarý", "Telefon numarasýný girin.", "Tamam");
                return;
            }

            if (tel.Length < 10)
            {
                await DisplayAlert("Uyarý", "Telefon numarasý çok kýsa.", "Tamam");
                return;
            }

            try
            {
                var result = await _api.TelefonGuncelleAsync(UserSession.KullaniciId, tel);

                if (!result.Success)
                {
                    await DisplayAlert("Hata", result.Message, "Tamam");
                    return;
                }

                await DisplayAlert("Baþarýlý", result.Message, "Tamam");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", ex.Message, "Tamam");
            }
        }
    }
}