using System;
using System.Threading.Tasks;
using berber_randevu_uygulamasi.Services;
using berber_randevu_uygulamasi.Views.AltBarlar;

namespace berber_randevu_uygulamasi.Views;

public partial class BerberAnaPanelSayfasi : ContentPage
{
    protected readonly ApiClient _api;

    public BerberAnaPanelSayfasi(ApiClient api)
    {
        InitializeComponent();
        _api = api;
        imgSahipFoto.Source = "default_user.png";
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await SayfaVerileriniYukleAsync();
    }

    private async Task SayfaVerileriniYukleAsync()
    {
        try
        {
            var data = await _api.GetBerberAnaPanelAsync(UserSession.KullaniciId);

            lblSahipAdSoyad.Text = string.IsNullOrWhiteSpace(data.AdSoyad)
                ? $"{UserSession.Ad} {UserSession.Soyad}".Trim()
                : data.AdSoyad;

            lblBugunRandevu.Text = data.BugunRandevuSayisi.ToString();

            lblSiradakiIsim.Text = string.IsNullOrWhiteSpace(data.SiradakiMusteriAdSoyad)
                ? "—"
                : data.SiradakiMusteriAdSoyad;

            lblSiradakiSaat.Text = string.IsNullOrWhiteSpace(data.SiradakiSaat)
                ? "—"
                : data.SiradakiSaat;

            if (!string.IsNullOrWhiteSpace(data.ProfilFoto))
            {
                if (data.ProfilFoto.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    data.ProfilFoto.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    imgSahipFoto.Source = ImageSource.FromUri(new Uri(data.ProfilFoto));
                }
                else
                {
                    imgSahipFoto.Source = data.ProfilFoto;
                }
            }
            else
            {
                imgSahipFoto.Source = "default_user.png";
            }
        }
        catch (Exception ex)
        {
            lblSahipAdSoyad.Text = $"{UserSession.Ad} {UserSession.Soyad}".Trim();
            lblBugunRandevu.Text = "0";
            lblSiradakiIsim.Text = "—";
            lblSiradakiSaat.Text = "—";
            imgSahipFoto.Source = "default_user.png";

            await DisplayAlert("Hata", ex.Message, "Tamam");
        }
    }

    private async void BugunRandevular_Tapped(object sender, TappedEventArgs e)
        => await Navigation.PushAsync(new BerberRandevularSayfasi(_api));

    private async void CalismaSaatleri_Tapped(object sender, TappedEventArgs e)
        => await Navigation.PushAsync(new BerberCalismaSaatleriSayfasi(_api));
}