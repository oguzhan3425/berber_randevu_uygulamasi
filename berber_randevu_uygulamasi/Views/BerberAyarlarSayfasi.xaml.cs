using System;
using System.IO;
using System.Threading.Tasks;
using Npgsql;
using berber_randevu_uygulamasi.Services;
using berber_randevu_uygulamasi.Views.AltBarlar;

namespace berber_randevu_uygulamasi.Views;

public partial class BerberAyarlarSayfasi : ContentPage
{
    public BerberAyarlarSayfasi()
    {
        InitializeComponent();

        // defaultlar
        imgDukkan.Source = "default_shop.png";
        imgSahip.Source = "default_user.png";
    }

    public enum FotoMode
    {
        Dukkan,
        Sahip
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
            int berberId = await GetBerberIdByKullaniciIdAsync(UserSession.KullaniciId);

            await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
            await conn.OpenAsync();

            // 1) DÜKKAN FOTO (Berberler tablosu)
            // Kolon adý farklýysa: "DukkanFoto" kýsmýný deðiþtir
            string sqlShop = @"
                SELECT ""ResimYolu""
                FROM ""Berberler""
                WHERE ""BerberID"" = @bid
                LIMIT 1;";

            await using (var cmdShop = new NpgsqlCommand(sqlShop, conn))
            {
                cmdShop.Parameters.AddWithValue("@bid", berberId);
                var obj = await cmdShop.ExecuteScalarAsync();

                if (obj != null && obj != DBNull.Value)
                {
                    var resimYolu = obj as string;

                    if (!string.IsNullOrWhiteSpace(resimYolu))
                        imgDukkan.Source = resimYolu;
                    else
                        imgDukkan.Source = "default_shop.png";
                }
                else
                {
                    imgDukkan.Source = "default_shop.png";
                }
            }


            // 2) SAHÝP FOTO (kullanici tablosu)
            // Kolon adý farklýysa: "ProfilFoto" kýsmýný deðiþtir
            string sqlOwner = @"
                SELECT ""ProfilFoto""
                FROM kullanici
                WHERE ""ID"" = @kid
                LIMIT 1;";

            await using (var cmdOwner = new NpgsqlCommand(sqlOwner, conn))
            {
                cmdOwner.Parameters.AddWithValue("@kid", UserSession.KullaniciId);
                var obj = await cmdOwner.ExecuteScalarAsync();

                if (obj != null && obj != DBNull.Value)
                {
                    string? path = obj as string;

                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        // path tam yolsa:
                        if (File.Exists(path))
                            imgSahip.Source = ImageSource.FromFile(path);
                        else
                            imgSahip.Source = "default_user.png";
                    }
                    else
                    {
                        imgSahip.Source = "default_user.png";
                    }
                }
                else
                {
                    imgSahip.Source = "default_user.png";
                }
            }
        }
        catch (Exception ex)
        {
            imgDukkan.Source = "default_shop.png";
            imgSahip.Source = "default_user.png";
            await DisplayAlert("Hata", ex.Message, "Tamam");
        }
    }

    private async Task<int> GetBerberIdByKullaniciIdAsync(int kullaniciId)
    {
        await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
        await conn.OpenAsync();

        string sql = @"
            SELECT ""BerberID""
            FROM ""Berberler""
            WHERE ""KullaniciID"" = @kid
            LIMIT 1;";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@kid", kullaniciId);

        var obj = await cmd.ExecuteScalarAsync();
        if (obj == null) throw new Exception("Bu kullanýcýya ait berber kaydý bulunamadý.");

        return Convert.ToInt32(obj);
    }

    // ÞÝMDÝLÝK örnek eventler (senin sayfalarýna yönlendiririz)
    private async void KisiselBilgiler_Tapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new BerberKisiselBilgilerSayfasi());
    }

    private async void CalismaSaatleri_Tapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new BerberCalismaSaatleriSayfasi());
    }

    private async void SifreDegistir_Tapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new SifreDegistirSayfasi());
    }
    private async void CikisYap_Tapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new GirisSayfasi());
    }

    private async void DukkanFotoDegistir_Clicked(object sender, EventArgs e)
    {
        int berberId = await GetBerberIdByKullaniciIdAsync(UserSession.KullaniciId);
        await Navigation.PushAsync(new ProfilFotoDegistirSayfasi(FotoHedefi.DukkanFoto, berberId));
    }

    private async void SahipFotoDegistir_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ProfilFotoDegistirSayfasi(FotoHedefi.SahipProfilFoto, UserSession.KullaniciId));
    }
}
