using System;
using System.IO;
using System.Threading.Tasks;
using Npgsql;
using berber_randevu_uygulamasi.Services;
using berber_randevu_uygulamasi.Views.AltBarlar;

namespace berber_randevu_uygulamasi.Views;

public partial class BerberAnaPanelSayfasi : ContentPage
{
    public BerberAnaPanelSayfasi()
    {
        InitializeComponent();

        // Ýlk açýlýþta default görünsün
        imgSahipFoto.Source = "default_user.png";
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Sað üst: sahip ad-soyad
        var sahip = $"{UserSession.Ad} {UserSession.Soyad}".Trim();
        lblSahipAdSoyad.Text = string.IsNullOrWhiteSpace(sahip) ? "—" : sahip;

        // Sað üst: sahip foto
        await SahipFotosuYukleAsync();

        // Bugün özet
        await BugunOzetYukleAsync();
    }

    private async Task SahipFotosuYukleAsync()
    {
        try
        {
            await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
            await conn.OpenAsync();

            // FOTOÐRAF KOLONU: Eðer sende farklýysa sadece "ProfilFoto" kýsmýný deðiþtir.
            string sql = @"
                SELECT ""ProfilFoto""
                FROM kullanici
                WHERE ""ID"" = @kid
                LIMIT 1;";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@kid", UserSession.KullaniciId);

            var obj = await cmd.ExecuteScalarAsync();

            if (obj != null && obj != DBNull.Value)
            {
                var bytes = (byte[])obj;
                imgSahipFoto.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
            }
            else
            {
                imgSahipFoto.Source = "default_user.png";
            }
        }
        catch
        {
            imgSahipFoto.Source = "default_user.png";
        }
    }

    private async Task BugunOzetYukleAsync()
    {
        try
        {
            int berberId = await GetBerberIdByKullaniciIdAsync(UserSession.KullaniciId);

            await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
            await conn.OpenAsync();

            var bugun = DateOnly.FromDateTime(DateTime.Now);

            // Bugünkü randevu sayýsý
            string sqlCount = @"
                SELECT COUNT(*)
                FROM randevular r
                WHERE r.""BerberID"" = @bid
                  AND r.""RandevuTarihi"" = @tarih;";

            await using (var cmdCount = new NpgsqlCommand(sqlCount, conn))
            {
                cmdCount.Parameters.AddWithValue("@bid", berberId);
                cmdCount.Parameters.AddWithValue("@tarih", bugun);

                var count = Convert.ToInt32(await cmdCount.ExecuteScalarAsync());
                lblBugunRandevu.Text = count.ToString();
            }

            // Sýradaki randevu
            string sqlNext = @"
                SELECT k.""Ad"", k.""Soyad"", r.""RandevuSaati""
                FROM randevular r
                JOIN kullanici k ON k.""ID"" = r.""KullaniciID""
                WHERE r.""BerberID"" = @bid
                  AND r.""RandevuTarihi"" = @tarih
                  AND r.""RandevuSaati"" >= @simdi
                ORDER BY r.""RandevuSaati""
                LIMIT 1;";

            await using (var cmdNext = new NpgsqlCommand(sqlNext, conn))
            {
                cmdNext.Parameters.AddWithValue("@bid", berberId);
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

    private async void BugunRandevular_Tapped(object sender, TappedEventArgs e)
        => await Navigation.PushAsync(new BerberRandevularSayfasi());

    private async void CalismaSaatleri_Tapped(object sender, TappedEventArgs e)
    {
        await Navigation.PushAsync(new BerberCalismaSaatleriSayfasi());
    }
}
