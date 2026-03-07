using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using berber_randevu_uygulamasi.Models;
using berber_randevu_uygulamasi.Services;
using berber_randevu_uygulamasi.Views.AltBarlar;

namespace berber_randevu_uygulamasi.Views;

public partial class BerberHizmetlerSayfasi : ContentPage
{
    protected readonly ApiClient _api;
    private List<Hizmet> _veri = new();

    private int _berberId;
    private int _calisanId;

    public BerberHizmetlerSayfasi(ApiClient api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await HizmetleriYukleAsync();
    }

    private async Task HizmetleriYukleAsync()
    {
        try
        {
            var response = await _api.GetBerberHizmetleriAsync(UserSession.KullaniciId);

            _berberId = response.BerberID;
            _calisanId = response.CalisanID;

            var liste = new List<Hizmet>();
            foreach (var item in response.Hizmetler)
            {
                liste.Add(new Hizmet
                {
                    HizmetID = item.HizmetID,
                    CalisanID = item.CalisanID,
                    HizmetAdi = item.HizmetAdi,
                    Fiyat = item.Fiyat,
                    SureDakika = item.SureDakika,
                    BerberID = item.BerberID,
                    Aktif = item.Aktif
                });
            }

            _veri = liste;
            listeHizmet.ItemsSource = _veri;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", "Hizmetler yüklenemedi:\n" + ex.Message, "Tamam");
        }
    }

    async void Ekle_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (_berberId <= 0 || _calisanId <= 0)
            {
                var response = await _api.GetBerberHizmetleriAsync(UserSession.KullaniciId);
                _berberId = response.BerberID;
                _calisanId = response.CalisanID;
            }

            await Navigation.PushModalAsync(new NavigationPage(
                new HizmetEkleModalSayfasi(_api, _berberId, _calisanId, async () =>
                {
                    await HizmetleriYukleAsync();
                })
            ));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", ex.Message, "Tamam");
        }
    }

    async void Duzenle_Clicked(object sender, EventArgs e)
    {
        var h = (sender as TapGestureRecognizer)?.CommandParameter as Hizmet;
        if (h == null) return;

        await DisplayAlert("Düzenle", $"{h.HizmetAdi} düzenle (sonra baðlarýz).", "Tamam");
    }

    async void AktifPasif_Clicked(object sender, EventArgs e)
    {
        var h = (sender as TapGestureRecognizer)?.CommandParameter as Hizmet;
        if (h == null) return;

        try
        {
            var result = await _api.BerberHizmetAktifPasifAsync(h.HizmetID);

            if (!result.Success)
            {
                await DisplayAlert("Hata", result.Message, "Tamam");
                return;
            }

            h.Aktif = result.Aktif;

            listeHizmet.ItemsSource = null;
            listeHizmet.ItemsSource = _veri;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", ex.Message, "Tamam");
        }
    }
}