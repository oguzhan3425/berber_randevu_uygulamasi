using berber_randevu_uygulamasi.Services;
using berber_randevu_uygulamasi.Views.AltBarlar;
using System;
using System.Threading.Tasks;

namespace berber_randevu_uygulamasi.Views;

public partial class CalisanAyarlarSayfasi : ContentPage
{
    protected readonly ApiClient _api;

    public CalisanAyarlarSayfasi(ApiClient api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ProfilFotoYukleAsync();
    }

    private async Task ProfilFotoYukleAsync()
    {
        try
        {
            var profil = await _api.KullaniciMiniProfilGetirAsync(UserSession.KullaniciId);

            if (!string.IsNullOrWhiteSpace(profil.ProfilFoto))
            {
                if (profil.ProfilFoto.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    profil.ProfilFoto.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    imgProfil.Source = ImageSource.FromUri(new Uri(profil.ProfilFoto));
                }
                else
                {
                    imgProfil.Source = profil.ProfilFoto;
                }
            }
            else
            {
                imgProfil.Source = "default_user.png";
            }
        }
        catch
        {
            imgProfil.Source = "default_user.png";
        }
    }

    private async void ProfilFotoDegistir_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(
            new ProfilFotoDegistirSayfasi(_api, FotoHedefi.SahipProfilFoto, UserSession.KullaniciId)
        );
    }

    private async void KisiselBilgiler_Tapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new BerberKisiselBilgilerSayfasi(_api));
    }

    private async void SifreDegistir_Tapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new SifreDegistirSayfasi(_api));
    }

    private async void CalismaSaatleri_Tapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new BerberCalismaSaatleriSayfasi(_api));
    }

    private async void CikisYap_Tapped(object sender, TappedEventArgs e)
    {
        bool ok = await DisplayAlert("Çýkýþ", "Çýkýþ yapmak istiyor musun?", "Evet", "Hayýr");
        if (!ok) return;

        UserSession.KullaniciId = 0;
        UserSession.Ad = "";
        UserSession.Soyad = "";

        Application.Current!.Windows[0].Page = new NavigationPage(new GirisSayfasi(_api));
    }
}