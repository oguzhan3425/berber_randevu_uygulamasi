using System;
using System.Threading.Tasks;
using berber_randevu_uygulamasi.Services;
using berber_randevu_uygulamasi.Views.AltBarlar;

namespace berber_randevu_uygulamasi.Views;

public partial class CalisanAnaSayfa : ContentPage
{
    protected readonly ApiClient _api;

    public CalisanAnaSayfa(ApiClient api)
    {
        InitializeComponent();
        _api = api;
        imgCalisanFoto.Source = "default_user.png";
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
            var data = await _api.GetCalisanAnaSayfaAsync(UserSession.KullaniciId);

            lblCalisanAdSoyad.Text = string.IsNullOrWhiteSpace(data.AdSoyad)
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
                    imgCalisanFoto.Source = ImageSource.FromUri(new Uri(data.ProfilFoto));
                }
                else
                {
                    imgCalisanFoto.Source = data.ProfilFoto;
                }
            }
            else
            {
                imgCalisanFoto.Source = "default_user.png";
            }
        }
        catch (Exception ex)
        {
            lblCalisanAdSoyad.Text = $"{UserSession.Ad} {UserSession.Soyad}".Trim();
            lblBugunRandevu.Text = "0";
            lblSiradakiIsim.Text = "—";
            lblSiradakiSaat.Text = "—";
            imgCalisanFoto.Source = "default_user.png";

            await DisplayAlert("Hata", ex.Message, "Tamam");
        }
    }

    private async void BugunRandevular_Tapped(object sender, TappedEventArgs e)
        => await Navigation.PushAsync(new CalisanRandevularSayfasi(_api));

    private async void CalismaSaatleri_Tapped(object sender, TappedEventArgs e)
        => await Navigation.PushAsync(new BerberCalismaSaatleriSayfasi(_api));
}