using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using berber_randevu_uygulamasi.Models;
using berber_randevu_uygulamasi.Models.Dtos;
using berber_randevu_uygulamasi.Services;

namespace berber_randevu_uygulamasi.Views;

public partial class CalisanRandevularSayfasi : ContentPage
{
    protected readonly ApiClient _api;
    bool _gelecekSecili = true;
    double _panTotalY = 0;

    public CalisanRandevularSayfasi(ApiClient api)
    {
        InitializeComponent();
        _api = api;
        SegmentAyarla(true);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RandevulariYukleAsync();
    }

    private async Task RandevulariYukleAsync()
    {
        try
        {
            int kullaniciId = UserSession.KullaniciId;

            var gelecekDto = await _api.GetCalisanGelecekRandevularAsync(kullaniciId);
            var gecmisDto = await _api.GetCalisanGecmisRandevularAsync(kullaniciId);

            var gelecekListe = new List<RandevuKart>();
            foreach (var item in gelecekDto)
            {
                gelecekListe.Add(new RandevuKart
                {
                    RandevuID = item.RandevuID,
                    TarihText = item.TarihText,
                    SaatText = item.SaatText,
                    MusteriAdSoyad = item.MusteriAdSoyad,
                    MusteriFoto = string.IsNullOrWhiteSpace(item.MusteriFoto)
                        ? "default_user.png"
                        : item.MusteriFoto,
                    HizmetAdi = item.HizmetAdi,
                    DurumText = item.DurumText,
                    ToplamUcret = item.ToplamUcret
                });
            }

            var gecmisListe = new List<RandevuKart>();
            foreach (var item in gecmisDto)
            {
                gecmisListe.Add(new RandevuKart
                {
                    RandevuID = item.RandevuID,
                    TarihText = item.TarihText,
                    SaatText = item.SaatText,
                    MusteriAdSoyad = item.MusteriAdSoyad,
                    MusteriFoto = string.IsNullOrWhiteSpace(item.MusteriFoto)
                        ? "default_user.png"
                        : item.MusteriFoto,
                    HizmetAdi = item.HizmetAdi,
                    DurumText = string.IsNullOrWhiteSpace(item.DurumText)
                        ? "Tamamlandý"
                        : item.DurumText,
                    ToplamUcret = item.ToplamUcret
                });
            }

            listeGelecek.ItemsSource = gelecekListe;
            listeGecmis.ItemsSource = gecmisListe;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", ex.Message, "Tamam");
        }
    }

    void SegmentAyarla(bool gelecek)
    {
        _gelecekSecili = gelecek;

        listeGelecek.IsVisible = gelecek;
        listeGecmis.IsVisible = !gelecek;

        segGelecek.BackgroundColor = gelecek ? Color.FromArgb("#232323") : Color.FromArgb("#1A1A1A");
        segGecmis.BackgroundColor = !gelecek ? Color.FromArgb("#232323") : Color.FromArgb("#1A1A1A");

        segGelecek.Opacity = gelecek ? 1 : 0.65;
        segGecmis.Opacity = !gelecek ? 1 : 0.65;
    }

    void Gelecek_Tapped(object sender, TappedEventArgs e) => SegmentAyarla(true);
    void Gecmis_Tapped(object sender, TappedEventArgs e) => SegmentAyarla(false);

    void Segment_PanUpdated(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _panTotalY = 0;
                break;

            case GestureStatus.Running:
                _panTotalY = e.TotalY;
                break;

            case GestureStatus.Completed:
                if (_panTotalY > 25) SegmentAyarla(false);
                else if (_panTotalY < -25) SegmentAyarla(true);
                break;
        }
    }

    private async void Yenile_Clicked(object sender, EventArgs e)
    {
        await RandevulariYukleAsync();
    }
}