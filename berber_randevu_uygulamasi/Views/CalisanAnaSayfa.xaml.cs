using System;
using System.IO;
using System.Threading.Tasks;
using Npgsql;
using berber_randevu_uygulamasi.Services;
using berber_randevu_uygulamasi.Views.AltBarlar;

namespace berber_randevu_uygulamasi.Views;

public partial class CalisanAnaSayfa : ContentPage
{
    public CalisanAnaSayfa()
    {
        InitializeComponent();

        // Ýlk açýlýþta default görünsün
        imgCalisanFoto.Source = "default_user.png";
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Üst: çalýþan ad-soyad
        var adSoyad = $"{UserSession.Ad} {UserSession.Soyad}".Trim();
        lblCalisanAdSoyad.Text = string.IsNullOrWhiteSpace(adSoyad) ? "—" : adSoyad;

        // Üst: çalýþan foto
        await CalisanFotosuYukleAsync();

        // Bugün özet (çalýþanýn randevularý)
        await BugunOzetYukleAsync();
    }

    private async Task CalisanFotosuYukleAsync()
    {
        try
        {
            await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
            await conn.OpenAsync();

            string sql = @"
            SELECT ""ProfilFoto""
            FROM kullanici
            WHERE ""ID"" = @kid
            LIMIT 1;";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@kid", UserSession.KullaniciId);

            var obj = await cmd.ExecuteScalarAsync();
            var s = obj?.ToString()?.Trim();

            if (string.IsNullOrWhiteSpace(s))
            {
                imgCalisanFoto.Source = "default_user.png";
                return;
            }

            // URL ise
            if (Uri.TryCreate(s, UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                imgCalisanFoto.Source = new UriImageSource
                {
                    Uri = uri,
                    CachingEnabled = true
                };
                return;
            }

            // Deðilse uygulama içi dosya adý / local path gibi davran
            imgCalisanFoto.Source = ImageSource.FromFile(s);
        }
        catch
        {
            imgCalisanFoto.Source = "default_user.png";
        }
    }

    private async Task BugunOzetYukleAsync()
    {
        try
        {
            int calisanId = await GetCalisanIdByKullaniciIdAsync(UserSession.KullaniciId);
            int berberId = await GetBerberIdByCalisanKullaniciIdAsync(UserSession.KullaniciId);

            await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
            await conn.OpenAsync();

            var bugun = DateOnly.FromDateTime(DateTime.Now);

            // Bugünkü randevu sayýsý (çalýþana göre)
            string sqlCount = @"
                SELECT COUNT(*)
                FROM randevular r
                WHERE r.""BerberID"" = @bid
                  AND r.""CalisanID"" = @cid
                  AND r.""RandevuTarihi"" = @tarih;";

            await using (var cmdCount = new NpgsqlCommand(sqlCount, conn))
            {
                cmdCount.Parameters.AddWithValue("@bid", berberId);
                cmdCount.Parameters.AddWithValue("@cid", calisanId);
                cmdCount.Parameters.AddWithValue("@tarih", bugun);

                var count = Convert.ToInt32(await cmdCount.ExecuteScalarAsync());
                lblBugunRandevu.Text = count.ToString();
            }

            // Sýradaki randevu (çalýþana göre)
            string sqlNext = @"
                SELECT k.""Ad"", k.""Soyad"", r.""RandevuSaati""
                FROM randevular r
                JOIN kullanici k ON k.""ID"" = r.""KullaniciID""
                WHERE r.""BerberID"" = @bid
                  AND r.""CalisanID"" = @cid
                  AND r.""RandevuTarihi"" = @tarih
                  AND r.""RandevuSaati"" >= @simdi
                ORDER BY r.""RandevuSaati""
                LIMIT 1;";

            await using (var cmdNext = new NpgsqlCommand(sqlNext, conn))
            {
                cmdNext.Parameters.AddWithValue("@bid", berberId);
                cmdNext.Parameters.AddWithValue("@cid", calisanId);
                cmdNext.Parameters.AddWithValue("@tarih", bugun);
                cmdNext.Parameters.AddWithValue("@simdi", TimeOnly.FromDateTime(DateTime.Now));

                await using var dr = await cmdNext.ExecuteReaderAsync();
                if (await dr.ReadAsync())
                {
                    string ad = dr.IsDBNull(0) ? "" : dr.GetString(0);
                    string soyad = dr.IsDBNull(1) ? "" : dr.GetString(1);
                    var saat = dr.IsDBNull(2) ? (TimeSpan?)null : dr.GetTimeSpan(2);

                    lblSiradakiIsim.Text = $"{ad} {soyad}".Trim();
                    lblSiradakiSaat.Text = saat.HasValue ? saat.Value.ToString(@"hh\:mm") : "—";
                }
                else
                {
                    lblSiradakiIsim.Text = "—";
                    lblSiradakiSaat.Text = "—";
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", ex.Message, "Tamam");
        }
    }

    private async Task<int> GetCalisanIdByKullaniciIdAsync(int kullaniciId)
    {
        await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
        await conn.OpenAsync();

        string sql = @"
            SELECT ""CalisanID""
            FROM calisanlar
            WHERE ""KullaniciID"" = @kid
            LIMIT 1;";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@kid", kullaniciId);

        var obj = await cmd.ExecuteScalarAsync();
        if (obj == null)
            throw new Exception("Bu kullanýcý için calisanlar kaydý yok. (Çalýþan kaydý oluþturulmamýþ olabilir.)");

        return Convert.ToInt32(obj);
    }

    private async Task<int> GetBerberIdByCalisanKullaniciIdAsync(int kullaniciId)
    {
        await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
        await conn.OpenAsync();

        // Varsayým: calisanlar tablosunda "BerberID" var
        string sql = @"
            SELECT ""BerberID""
            FROM calisanlar
            WHERE ""KullaniciID"" = @kid
            LIMIT 1;";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@kid", kullaniciId);

        var obj = await cmd.ExecuteScalarAsync();
        if (obj == null)
            throw new Exception("Bu kullanýcý için BerberID bulunamadý (calisanlar.BerberID boþ olabilir).");

        return Convert.ToInt32(obj);
    }

    // Kart týklamalarý
    private async void BugunRandevular_Tapped(object sender, TappedEventArgs e)
        => await Navigation.PushAsync(new CalisanRandevularSayfasi()); // sende isim farklýysa deðiþtir

    private async void CalismaSaatleri_Tapped(object sender, TappedEventArgs e)
    { 
         await Navigation.PushAsync(new BerberCalismaSaatleriSayfasi()); // sende isim farklýysa deðiþtir
    }
}
