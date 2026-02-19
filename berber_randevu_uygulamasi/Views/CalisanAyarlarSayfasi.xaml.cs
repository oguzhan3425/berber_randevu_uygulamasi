using System;
using System.IO;
using System.Threading.Tasks;
using Npgsql;
using berber_randevu_uygulamasi.Services;
using berber_randevu_uygulamasi.Views.AltBarlar;

namespace berber_randevu_uygulamasi.Views;

public partial class CalisanAyarlarSayfasi : ContentPage
{
    public CalisanAyarlarSayfasi()
    {
        InitializeComponent();
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
            await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
            await conn.OpenAsync();

            string sql = @"
                SELECT COALESCE(""ProfilFoto"", '')
                FROM kullanici
                WHERE ""ID"" = @id
                LIMIT 1;";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", UserSession.KullaniciId);

            var obj = await cmd.ExecuteScalarAsync();
            var path = obj?.ToString() ?? "";

            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                imgProfil.Source = ImageSource.FromFile(path);
            else
                imgProfil.Source = "default_user.png";
        }
        catch
        {
            imgProfil.Source = "default_user.png";
        }
    }

    private async void ProfilFotoDegistir_Clicked(object sender, EventArgs e)
    {
        // Senin mevcut sayfan (ayný tasarým / ayný sayfa)
        await Navigation.PushAsync(new ProfilFotoDegistirSayfasi());
    }

    private async void KisiselBilgiler_Tapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new BerberKisiselBilgilerSayfasi());
    }

    private async void SifreDegistir_Tapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new SifreDegistirSayfasi());
    }

    private async void CalismaSaatleri_Tapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new BerberCalismaSaatleriSayfasi());
    }

    private async void CikisYap_Tapped(object sender, TappedEventArgs e)
    {
        bool ok = await DisplayAlert("Çýkýþ", "Çýkýþ yapmak istiyor musun?", "Evet", "Hayýr");
        if (!ok) return;

        // Session temizle (senin projende nasýl yapýyorsan ona göre)
        UserSession.KullaniciId = 0;
        UserSession.Ad = "";
        UserSession.Soyad = "";

        // Giriþ sayfasýna dön (sende Login sayfasý adý neyse onu yaz)
        Application.Current!.Windows[0].Page = new NavigationPage(new GirisSayfasi());
    }
}
