using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using berber_randevu_uygulamasi.Services;
using berber_randevu_uygulamasi.Models.Dtos;
using berber_randevu_uygulamasi.Views.AltBarlar;

namespace berber_randevu_uygulamasi.Views;

public partial class BerberAyarlarSayfasi : ContentPage
{
    protected readonly ApiClient _api;

    private int _berberId;                 // dükkan foto deðiþtir için lazým
    private string _dukkanFotoUrl = "";    // opsiyonel
    private string _profilFotoUrl = "";    // opsiyonel

    public BerberAyarlarSayfasi(ApiClient api)
    {
        InitializeComponent();
        _api = api;

        imgDukkan.Source = "default_shop.png";
        imgSahip.Source = "default_user.png";
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await FotolariYukleAsync();
    }

    private async Task FotolariYukleAsync()
    {
        try
        {
            // 1) Kullanýcý profil (profil foto url buradan)
            var user = await _api.GetAsync<UserProfileDto>($"users/{UserSession.KullaniciId}/profile");
            if (user != null)
            {
                _profilFotoUrl = (user.ProfilFotoUrl ?? "").Trim();
                SetImageFromUrl(imgSahip, _profilFotoUrl, "default_user.png");

                // Session tipi boþsa doldur (lazým olabilir)
                if (string.IsNullOrWhiteSpace(UserSession.KullaniciTipi))
                    UserSession.KullaniciTipi = (user.KullaniciTipi ?? "").Trim();
            }
            else
            {
                imgSahip.Source = "default_user.png";
            }

            // 2) Berber bilgisi (dükkan foto url + berberId buradan)
            var berber = await _api.GetAsync<BerberDto>($"berberler/by-kullanici/{UserSession.KullaniciId}");
            if (berber != null)
            {
                _berberId = berber.BerberId;
                _dukkanFotoUrl = (berber.DukkanFotoUrl ?? "").Trim();
                SetImageFromUrl(imgDukkan, _dukkanFotoUrl, "default_shop.png");
            }
            else
            {
                _berberId = 0;
                imgDukkan.Source = "default_shop.png";
            }
        }
        catch (Exception ex)
        {
            imgDukkan.Source = "default_shop.png";
            imgSahip.Source = "default_user.png";
            await DisplayAlert("Hata", ex.Message, "Tamam");
        }
    }

    private static void SetImageFromUrl(Image img, string? url, string defaultImage)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(url) &&
                Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                img.Source = ImageSource.FromUri(uri);
            }
            else
            {
                img.Source = defaultImage;
            }
        }
        catch
        {
            img.Source = defaultImage;
        }
    }

    // ------------------ Navigations ------------------

    private async void KisiselBilgiler_Tapped(object sender, TappedEventArgs e)
        => await Navigation.PushAsync(new BerberKisiselBilgilerSayfasi(_api));

    private async void CalismaSaatleri_Tapped(object sender, TappedEventArgs e)
        => await Navigation.PushAsync(new BerberCalismaSaatleriSayfasi(_api));

    private async void SifreDegistir_Tapped(object sender, TappedEventArgs e)
        => await Navigation.PushAsync(new SifreDegistirSayfasi(_api));

    private async void CikisYap_Tapped(object sender, TappedEventArgs e)
        => await Navigation.PushAsync(new GirisSayfasi(_api));

    // ------------------ Foto Deðiþtir ------------------

    private async void DukkanFotoDegistir_Clicked(object sender, EventArgs e)
    {
        try
        {
            // BerberId daha önce gelmemiþ olabilir (ilk açýlýþ vs.)
            if (_berberId <= 0)
            {
                var berber = await _api.GetAsync<BerberDto>($"berberler/by-kullanici/{UserSession.KullaniciId}");
                if (berber == null)
                {
                    await DisplayAlert("Hata", "Berber kaydý bulunamadý.", "Tamam");
                    return;
                }
                _berberId = berber.BerberId;
            }

            await Navigation.PushAsync(new ProfilFotoDegistirSayfasi(_api, FotoHedefi.DukkanFoto, _berberId));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", ex.Message, "Tamam");
        }
    }

    private async void SahipFotoDegistir_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ProfilFotoDegistirSayfasi(_api, FotoHedefi.SahipProfilFoto, UserSession.KullaniciId));
    }
}