using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using berber_randevu_uygulamasi.Services;

namespace berber_randevu_uygulamasi.Views
{
    public partial class CalisanEkleModalSayfasi : ContentPage
    {
        protected readonly ApiClient _api;
        private readonly int _berberId;
        private readonly Func<Task>? _onAddedRefresh;

        private int _bulunanKullaniciId = 0;

        public CalisanEkleModalSayfasi(ApiClient api, int berberId, Func<Task>? onAddedRefresh = null)
        {
            InitializeComponent();
            _api = api;
            _berberId = berberId;
            _onAddedRefresh = onAddedRefresh;
        }

        private async void Ara_Clicked(object sender, EventArgs e)
        {
            lblBilgi.IsVisible = false;
            sonucKart.IsVisible = false;
            _bulunanKullaniciId = 0;

            if (!int.TryParse(txtKullaniciId.Text?.Trim(), out int kid))
            {
                await DisplayAlert("Uyarý", "Geçerli bir kullanýcý ID gir.", "Tamam");
                return;
            }

            try
            {
                var user = await _api.CalisanAdayAraAsync(kid);

                if (user != null)
                {
                    _bulunanKullaniciId = user.KullaniciID;

                    lblAdSoyad.Text = $"{user.Ad} {user.Soyad}".Trim();
                    lblTelefon.Text = string.IsNullOrWhiteSpace(user.Telefon)
                        ? "Telefon: —"
                        : $"Telefon: {user.Telefon}";
                    lblTip.Text = string.IsNullOrWhiteSpace(user.KullaniciTipi)
                        ? "Tip: —"
                        : $"Tip: {user.KullaniciTipi}";

                    sonucKart.IsVisible = true;
                }
                else
                {
                    lblBilgi.Text = "Bu ID ile kullanýcý bulunamadý.";
                    lblBilgi.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", ex.Message, "Tamam");
            }
        }

        private async void Ekle_Clicked(object sender, EventArgs e)
        {
            if (_bulunanKullaniciId <= 0)
            {
                await DisplayAlert("Uyarý", "Önce kullanýcýyý ara.", "Tamam");
                return;
            }

            try
            {
                var result = await _api.CalisanEkleAsync(_berberId, _bulunanKullaniciId);

                if (!result.Success)
                {
                    await DisplayAlert("Bilgi", result.Message, "Tamam");
                    return;
                }

                await DisplayAlert("Baþarýlý", result.Message, "Tamam");

                if (_onAddedRefresh != null)
                    await _onAddedRefresh();

                await Navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", ex.Message, "Tamam");
            }
        }

        private async void Kapat_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}