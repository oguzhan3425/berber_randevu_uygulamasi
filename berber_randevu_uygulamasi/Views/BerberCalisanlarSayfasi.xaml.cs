using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using berber_randevu_uygulamasi.Models;
using berber_randevu_uygulamasi.Services;

namespace berber_randevu_uygulamasi.Views;

public partial class BerberCalisanlarSayfasi : ContentPage
{
    private List<CalisanKart> _veri = new();

    public BerberCalisanlarSayfasi()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CalisanlariYukleAsync();
    }

    private async Task CalisanlariYukleAsync()
    {
        try
        {
            int berberId = await GetBerberIdByKullaniciIdAsync(UserSession.KullaniciId);

            await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
            await conn.OpenAsync();

            // calisanlar: "CalisanID","KullaniciID","BerberID"
            // kullanici: "ID","Ad","Soyad","Telefon","ProfilFoto"
            string sql = @"
                SELECT
                    c.""CalisanID"",
                    c.""BerberID"",
                    c.""KullaniciID"",
                    k.""Ad"",
                    k.""Soyad"",
                    k.""Telefon"",
                    k.""ProfilFoto""
                FROM calisanlar c
                JOIN kullanici k ON k.""ID"" = c.""KullaniciID""
                WHERE c.""BerberID"" = @bid
                ORDER BY k.""Ad"", k.""Soyad"";";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@bid", berberId);

            await using var dr = await cmd.ExecuteReaderAsync();

            var liste = new List<CalisanKart>();

            while (await dr.ReadAsync())
            {
                int calisanId = dr.GetInt32(0);
                int bid = dr.GetInt32(1);
                int kid = dr.GetInt32(2);

                string ad = dr.IsDBNull(3) ? "" : dr.GetString(3);
                string soyad = dr.IsDBNull(4) ? "" : dr.GetString(4);
                string tel = dr.IsDBNull(5) ? "" : dr.GetString(5);
                string foto = dr.IsDBNull(6) ? "" : dr.GetString(6);

                liste.Add(new CalisanKart
                {
                    CalisanID = calisanId,
                    BerberID = bid,
                    KullaniciID = kid,

                    Ad = ad,
                    Soyad = soyad,
                    Telefon = tel,

                    // Rol/Aktif DB’de yoksa þimdilik sabit
                    Rol = "Çalýþan",
                    Aktif = true,

                    // Eðer modelinde Foto alaný varsa doldur (yoksa bu satýrý kaldýr)
                    Foto = string.IsNullOrWhiteSpace(foto) ? "default_user.png" : foto,

                    
                });
            }

            _veri = liste;
            listeCalisan.ItemsSource = _veri;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", "Çalýþanlar yüklenemedi:\n" + ex.Message, "Tamam");
        }
    }

    private async Task<int> GetBerberIdByKullaniciIdAsync(int kullaniciId)
    {
        await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
        await conn.OpenAsync();

        // Eðer tablon küçük harf ise: FROM berberler
        // Eðer týrnaklý/büyük harf ise: FROM "Berberler"
        string sql = @"
            SELECT ""BerberID""
            FROM ""Berberler""
            WHERE ""KullaniciID"" = @kid
            LIMIT 1;";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@kid", kullaniciId);

        var obj = await cmd.ExecuteScalarAsync();
        if (obj == null)
            throw new Exception("Bu kullanýcýya ait berber kaydý bulunamadý.");

        return Convert.ToInt32(obj);
    }

    async void Ekle_Clicked(object sender, EventArgs e)
    {
        // BerberID'yi bul
        int berberId = await GetBerberIdByKullaniciIdAsync(UserSession.KullaniciId);

        // Modal sayfayý aç
        await Navigation.PushModalAsync(new NavigationPage(
            new CalisanEkleModalSayfasi(berberId, async () =>
            {
                // Ekleme baþarýlý olunca listeyi yenile
                await CalisanlariYukleAsync();
            })
        ));
    }

    async void Duzenle_Clicked(object sender, EventArgs e)
    {
        if (sender is Button b && b.CommandParameter is CalisanKart c)
            await DisplayAlert("Düzenle", $"{c.Ad} {c.Soyad} düzenle.", "Tamam");
    }

    void AktifPasif_Clicked(object sender, EventArgs e)
    {
        // Þimdilik sadece UI toggle (DB’ye yazmýyoruz)
        if (sender is Button b && b.CommandParameter is CalisanKart c)
        {
            c.Aktif = !c.Aktif;

            // Basit yenileme
            listeCalisan.ItemsSource = null;
            listeCalisan.ItemsSource = _veri;
        }
    }
}