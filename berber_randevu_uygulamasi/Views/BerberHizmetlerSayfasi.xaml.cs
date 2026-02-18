using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Npgsql;
using berber_randevu_uygulamasi.Models;
using berber_randevu_uygulamasi.Services;            
using berber_randevu_uygulamasi.Views.AltBarlar;

namespace berber_randevu_uygulamasi.Views;

public partial class BerberHizmetlerSayfasi : ContentPage
{
    private List<Hizmet> _veri = new();

    public BerberHizmetlerSayfasi()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await HizmetleriYukleAsync();
    }

    private async Task HizmetleriYukleAsync()
    {
        try
        {
            int berberId = await GetBerberIdByKullaniciIdAsync(UserSession.KullaniciId);
            int calisanId = await GetCalisanIdByKullaniciIdAsync(UserSession.KullaniciId); // berberin kendi çalýþan kaydý

            await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
            await conn.OpenAsync();

            string sql = @"
                SELECT ""HizmetID"", ""CalisanID"", ""HizmetAdi"", ""Fiyat"", ""SureDakika"", ""BerberID"", ""Aktif""
                FROM hizmetler
                WHERE ""BerberID"" = @bid
                  AND ""CalisanID"" = @cid
                ORDER BY ""HizmetAdi"";";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@bid", berberId);
            cmd.Parameters.AddWithValue("@cid", calisanId);

            await using var dr = await cmd.ExecuteReaderAsync();

            var liste = new List<Hizmet>();
            while (await dr.ReadAsync())
            {
                liste.Add(new Hizmet
                {
                    HizmetID = dr.GetInt32(0),
                    CalisanID = dr.IsDBNull(1) ? 0 : dr.GetInt32(1),
                    HizmetAdi = dr.IsDBNull(2) ? "" : dr.GetString(2),
                    Fiyat = dr.IsDBNull(3) ? 0 : dr.GetDecimal(3),
                    SureDakika = dr.IsDBNull(4) ? 0 : dr.GetInt32(4),
                    BerberID = dr.IsDBNull(5) ? berberId : dr.GetInt32(5),
                    Aktif = !dr.IsDBNull(6) && dr.GetBoolean(6)
                });
            }

            _veri = liste;
            listeHizmet.ItemsSource = _veri;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", "Hizmetler yüklenemedi:\n" + ex.Message, "Tamam");
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
        if (obj == null) throw new Exception("Bu kullanýcý için calisanlar kaydý yok. (Berber giriþinde otomatik ekleme çalýþmamýþ olabilir.)");
        return Convert.ToInt32(obj);
    }

    async void Ekle_Clicked(object sender, EventArgs e)
    {
        int berberId = await GetBerberIdByKullaniciIdAsync(UserSession.KullaniciId);
        int calisanId = await GetCalisanIdByKullaniciIdAsync(UserSession.KullaniciId);

        await Navigation.PushModalAsync(new NavigationPage(
            new HizmetEkleModalSayfasi(berberId, calisanId, async () =>
            {
                await HizmetleriYukleAsync();
            })
        ));
    }

    async void Duzenle_Clicked(object sender, EventArgs e)
    {
        var h = (sender as TapGestureRecognizer)?.CommandParameter as Hizmet;
        if (h == null) return;

        await DisplayAlert("Düzenle", $"{h.HizmetAdi} düzenle (sonra baðlarýz).", "Tamam");
    }

    async void AktifPasif_Clicked(object sender, EventArgs e)
    {
        var h = (sender as TapGestureRecognizer)?.CommandParameter as Hizmet;
        if (h == null) return;

        try
        {
            await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
            await conn.OpenAsync();

            string sql = @"UPDATE hizmetler SET ""Aktif"" = NOT ""Aktif"" WHERE ""HizmetID"" = @id;";
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", h.HizmetID);
            await cmd.ExecuteNonQueryAsync();

            h.Aktif = !h.Aktif;

            listeHizmet.ItemsSource = null;
            listeHizmet.ItemsSource = _veri;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", ex.Message, "Tamam");
        }
    }
}