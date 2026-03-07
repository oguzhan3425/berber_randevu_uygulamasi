using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using berber_randevu_uygulamasi.Models;
using berber_randevu_uygulamasi.Models.Dtos;
using berber_randevu_uygulamasi.Services;

namespace berber_randevu_uygulamasi.Views;

public partial class BerberCalisanlarSayfasi : ContentPage
{
    protected readonly ApiClient _api;
    private List<CalisanKart> _veri = new();
    private int _berberId;

    public BerberCalisanlarSayfasi(ApiClient api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CalisanlariYukleAsync();
    }

    private async Task CalisanlariYukleAsync()
    {
        try
        {
            var response = await _api.GetBerberCalisanlariAsync(UserSession.KullaniciId);
            _berberId = response.BerberID;

            var liste = new List<CalisanKart>();

            foreach (var item in response.Calisanlar)
            {
                liste.Add(new CalisanKart
                {
                    CalisanID = item.CalisanID,
                    BerberID = item.BerberID,
                    KullaniciID = item.KullaniciID,
                    Ad = item.Ad,
                    Soyad = item.Soyad,
                    Telefon = item.Telefon,
                    Rol = item.Rol,
                    Aktif = item.Aktif,
                    Foto = string.IsNullOrWhiteSpace(item.Foto) ? "default_user.png" : item.Foto
                });
            }

            _veri = liste;
            listeCalisan.ItemsSource = _veri;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", "Çalýþanlar yüklenemedi:\n" + ex.Message, "Tamam");
        }
    }

    async void Ekle_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (_berberId <= 0)
            {
                var response = await _api.GetBerberCalisanlariAsync(UserSession.KullaniciId);
                _berberId = response.BerberID;
            }

            await Navigation.PushModalAsync(new NavigationPage(
                new CalisanEkleModalSayfasi(_api, _berberId, async () =>
                {
                    await CalisanlariYukleAsync();
                })
            ));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", ex.Message, "Tamam");
        }
    }

    async void CalisaniSil_Clicked(object sender, EventArgs e)
    {
        if (sender is not Button b || b.CommandParameter is not CalisanKart c)
            return;

        if (c.KullaniciID == UserSession.KullaniciId)
        {
            await DisplayAlert("Uyarý", "Kendi hesabýný çalýþanlýktan çýkaramazsýn.", "Tamam");
            return;
        }

        bool ok = await DisplayAlert(
            "Onay",
            $"{c.Ad} {c.Soyad} çalýþanlýktan çýkarýlsýn mý?\n(Kullanýcý tipi Müþteri yapýlacak)",
            "Evet",
            "Vazgeç");

        if (!ok) return;

        try
        {
            var result = await _api.BerberCalisaniSilAsync(c.BerberID, c.KullaniciID);

            if (!result.Success)
            {
                await DisplayAlert("Hata", result.Message, "Tamam");
                return;
            }

            await DisplayAlert("Baþarýlý", result.Message, "Tamam");
            await CalisanlariYukleAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", ex.Message, "Tamam");
        }
    }
}