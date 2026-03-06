using System;
using System.Linq;
using Microsoft.Maui.Controls;
using berber_randevu_uygulamasi.Services;
using berber_randevu_uygulamasi.Models.Dtos;

namespace berber_randevu_uygulamasi.Views
{
    public partial class KayitOlSayfasi : ContentPage
    {
        protected readonly ApiClient _api;

        public KayitOlSayfasi(ApiClient api)
        {
            InitializeComponent();
            _api = api;
        }

        private async void KayitOl_Clicked(object sender, EventArgs e)
        {
            string Ad = AdEntry.Text?.Trim() ?? "";
            string Soyad = SoyadEntry.Text?.Trim() ?? "";
            string KullaniciAdi = KullaniciAdiEntry.Text?.Trim() ?? "";
            string Eposta = EmailEntry.Text?.Trim() ?? "";
            string Sifre = SifreEntry.Text ?? "";
            string Telefon = TelefonEntry.Text?.Trim() ?? "";

            Telefon = new string(Telefon.Where(char.IsDigit).ToArray());

            // ✅ Telefon kontrolü
            if (Telefon.Length != 11 || !Telefon.StartsWith("05"))
            {
                await DisplayAlert("Geçersiz Telefon",
                    "Telefon numarası 05 ile başlamalı ve 11 haneli olmalıdır. (Örn: 05xx xxx xx xx)",
                    "Tamam");
                return;
            }

            // Boş alan kontrolü
            if (string.IsNullOrWhiteSpace(Ad) ||
                string.IsNullOrWhiteSpace(Soyad) ||
                string.IsNullOrWhiteSpace(KullaniciAdi) ||
                string.IsNullOrWhiteSpace(Eposta) ||
                string.IsNullOrWhiteSpace(Sifre) ||
                string.IsNullOrWhiteSpace(Telefon))
            {
                await DisplayAlert("Uyarı", "Lütfen tüm alanları doldurunuz.", "Tamam");
                return;
            }

            // ✅ Mail uzantı kontrolü
            if (!(Eposta.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase) ||
                  Eposta.EndsWith("@hotmail.com", StringComparison.OrdinalIgnoreCase) ||
                  Eposta.EndsWith("@icloud.com", StringComparison.OrdinalIgnoreCase)))
            {
                await DisplayAlert("Geçersiz E-posta", "Lütfen geçerli bir eposta adresi giriniz.", "Tamam");
                return;
            }

            try
            {
                var req = new RegisterRequest
                {
                    Ad = Ad,
                    Soyad = Soyad,
                    KullaniciAdi = KullaniciAdi,
                    Eposta = Eposta,
                    Sifre = Sifre,
                    Telefon = Telefon
                };

                var resp = await _api.PostJsonAsync<RegisterRequest, RegisterResponse>("auth/register", req);

                if (resp == null)
                {
                    await DisplayAlert("Hata", "Sunucuya ulaşılamadı.", "Tamam");
                    return;
                }

                if (!resp.Basarili)
                {
                    await DisplayAlert("Hata", resp.Mesaj, "Tamam");
                    return;
                }

                await DisplayAlert("Başarılı", resp.Mesaj, "Tamam");
                await Navigation.PushAsync(new GirisSayfasi(_api));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", ex.Message, "Tamam");
            }
        }

        private async void GirisYap_Tapped(object sender, TappedEventArgs e)
        {
            await Navigation.PushAsync(new GirisSayfasi(_api));
        }
    }
}