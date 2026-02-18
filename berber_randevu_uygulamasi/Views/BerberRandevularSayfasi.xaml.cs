using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using berber_randevu_uygulamasi.Models;
using berber_randevu_uygulamasi.Services;

namespace berber_randevu_uygulamasi.Views;

public partial class BerberRandevularSayfasi : ContentPage
{
    bool _gelecekSecili = true;
    double _panTotalY = 0;

    public BerberRandevularSayfasi()
    {
        InitializeComponent();
        SegmentAyarla(gelecek: true);
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
            int berberId = await GetBerberIdByKullaniciIdAsync(UserSession.KullaniciId);

            var bugun = DateOnly.FromDateTime(DateTime.Now);
            var simdi = TimeOnly.FromDateTime(DateTime.Now);

            await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
            await conn.OpenAsync();

            // ? GELECEK: bugün + saat>=þimdi veya gelecek günler
            string sqlGelecek = @"
                SELECT
                    r.""RandevuID"",
                    r.""RandevuTarihi"",
                    r.""RandevuSaati"",
                    r.""ToplamUcret"",
                    k.""Ad"",
                    k.""Soyad"",
                    k.""ProfilFoto"",
                    h.""HizmetAdi""
                FROM randevular r
                JOIN kullanici k ON k.""ID"" = r.""KullaniciID""
                LEFT JOIN hizmetler h ON h.""HizmetID"" = r.""HizmetID""
                WHERE r.""BerberID"" = @bid
                  AND (
                        r.""RandevuTarihi"" > @bugun
                     OR (r.""RandevuTarihi"" = @bugun AND r.""RandevuSaati"" >= @simdi)
                  )
                ORDER BY r.""RandevuTarihi"", r.""RandevuSaati"";";

            var gelecekListe = new List<RandevuKart>();
            await using (var cmd = new NpgsqlCommand(sqlGelecek, conn))
            {
                cmd.Parameters.AddWithValue("@bid", berberId);
                cmd.Parameters.AddWithValue("@bugun", bugun);
                cmd.Parameters.AddWithValue("@simdi", simdi);

                await using var dr = await cmd.ExecuteReaderAsync();
                while (await dr.ReadAsync())
                {
                    var tarih = dr.GetFieldValue<DateOnly>(1);
                    var saatTs = dr.IsDBNull(2) ? (TimeSpan?)null : dr.GetTimeSpan(2);

                    string ad = dr.IsDBNull(4) ? "" : dr.GetString(4);
                    string soyad = dr.IsDBNull(5) ? "" : dr.GetString(5);
                    string foto = dr.IsDBNull(6) ? "" : dr.GetString(6);
                    string hizmetAdi = dr.IsDBNull(7) ? "Hizmet" : dr.GetString(7);

                    gelecekListe.Add(new RandevuKart
                    {
                        RandevuID = dr.GetInt32(0),
                        TarihText = $"{tarih:dd.MM.yyyy}",
                        SaatText = saatTs.HasValue ? saatTs.Value.ToString(@"hh\:mm") : "—",
                        MusteriAdSoyad = $"{ad} {soyad}".Trim(),
                        MusteriFoto = string.IsNullOrWhiteSpace(foto) ? "default_user.png" : foto,
                        HizmetAdi = hizmetAdi,
                        ToplamUcret = dr.IsDBNull(3) ? 0 : dr.GetDecimal(3)
                    });
                }
            }

            // ? GEÇMÝÞ: bugün + saat<þimdi veya önceki günler
            string sqlGecmis = @"
                SELECT
                    r.""RandevuID"",
                    r.""RandevuTarihi"",
                    r.""RandevuSaati"",
                    r.""ToplamUcret"",
                    k.""Ad"",
                    k.""Soyad"",
                    k.""ProfilFoto"",
                    h.""HizmetAdi""
                FROM randevular r
                JOIN kullanici k ON k.""ID"" = r.""KullaniciID""
                LEFT JOIN hizmetler h ON h.""HizmetID"" = r.""HizmetID""
                WHERE r.""BerberID"" = @bid
                  AND (
                        r.""RandevuTarihi"" < @bugun
                     OR (r.""RandevuTarihi"" = @bugun AND r.""RandevuSaati"" < @simdi)
                  )
                ORDER BY r.""RandevuTarihi"" DESC, r.""RandevuSaati"" DESC;";

            var gecmisListe = new List<RandevuKart>();
            await using (var cmd2 = new NpgsqlCommand(sqlGecmis, conn))
            {
                cmd2.Parameters.AddWithValue("@bid", berberId);
                cmd2.Parameters.AddWithValue("@bugun", bugun);
                cmd2.Parameters.AddWithValue("@simdi", simdi);

                await using var dr2 = await cmd2.ExecuteReaderAsync();
                while (await dr2.ReadAsync())
                {
                    var tarih = dr2.GetFieldValue<DateOnly>(1);
                    var saatTs = dr2.IsDBNull(2) ? (TimeSpan?)null : dr2.GetTimeSpan(2);

                    string ad = dr2.IsDBNull(4) ? "" : dr2.GetString(4);
                    string soyad = dr2.IsDBNull(5) ? "" : dr2.GetString(5);
                    string foto = dr2.IsDBNull(6) ? "" : dr2.GetString(6);
                    string hizmetAdi = dr2.IsDBNull(7) ? "Hizmet" : dr2.GetString(7);

                    gecmisListe.Add(new RandevuKart
                    {
                        RandevuID = dr2.GetInt32(0),
                        TarihText = $"{tarih:dd.MM.yyyy}",
                        SaatText = saatTs.HasValue ? saatTs.Value.ToString(@"hh\:mm") : "—",
                        MusteriAdSoyad = $"{ad} {soyad}".Trim(),
                        MusteriFoto = string.IsNullOrWhiteSpace(foto) ? "default_user.png" : foto,
                        HizmetAdi = hizmetAdi,

                        // Eðer Durum alanýn yoksa sabit yazý kalsýn
                        DurumText = "Tamamlandý",
                        ToplamUcret = dr2.IsDBNull(3) ? 0 : dr2.GetDecimal(3)
                    });
                }
            }

            listeGelecek.ItemsSource = gelecekListe;
            listeGecmis.ItemsSource = gecmisListe;
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

        // Eðer tabloda adýn küçükse: FROM berberler
        // Eðer týrnaklý/büyükse: FROM "Berberler"
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

    // --- Segment (senin iPhone tarzý barýn) ---
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
                if (_panTotalY > 25) SegmentAyarla(false);     // aþaðý -> geçmiþ
                else if (_panTotalY < -25) SegmentAyarla(true); // yukarý -> gelecek
                break;
        }
    }

    private async void Yenile_Clicked(object sender, EventArgs e)
    {
        await RandevulariYukleAsync();
    }
}