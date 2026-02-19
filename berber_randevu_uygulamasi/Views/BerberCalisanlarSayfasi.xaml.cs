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

    async void CalisaniSil_Clicked(object sender, EventArgs e)
    {
        if (sender is not Button b || b.CommandParameter is not CalisanKart c)
            return;

        // Ýstersen sahibi/aktif kullanýcýyý silmeyi engelle
        if (c.KullaniciID == UserSession.KullaniciId)
        {
            await DisplayAlert("Uyarý", "Kendi hesabýný çalýþanlýktan çýkaramazsýn.", "Tamam");
            return;
        }

        bool ok = await DisplayAlert(
            "Onay",
            $"{c.Ad} {c.Soyad} çalýþanlýktan çýkarýlsýn mý?\n(Kullanýcý tipi Müþteri yapýlacak)",
            "Evet",
            "Vazgeç");

        if (!ok) return;

        try
        {
            await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
            await conn.OpenAsync();
            await using var tx = await conn.BeginTransactionAsync();

            // 1) Çalýþan iliþkisini kaldýr (kullanýcý hesabý silinmez!)
            string sqlDel = @"
            DELETE FROM calisanlar
            WHERE ""BerberID"" = @bid
              AND ""KullaniciID"" = @kid;";

            await using (var cmdDel = new NpgsqlCommand(sqlDel, conn, tx))
            {
                cmdDel.Parameters.AddWithValue("@bid", c.BerberID);     // zaten kartta var
                cmdDel.Parameters.AddWithValue("@kid", c.KullaniciID);

                int affected = await cmdDel.ExecuteNonQueryAsync();
                if (affected == 0)
                {
                    await tx.RollbackAsync();
                    await DisplayAlert("Bilgi", "Kayýt bulunamadý (zaten çýkarýlmýþ olabilir).", "Tamam");
                    return;
                }
            }

            // 2) Kullanýcý tipini Müþteri yap
            string sqlUpd = @"
            UPDATE kullanici
            SET ""KullaniciTipi"" = @tip
            WHERE ""ID"" = @kid;";

            await using (var cmdUpd = new NpgsqlCommand(sqlUpd, conn, tx))
            {
                cmdUpd.Parameters.AddWithValue("@tip", "Musteri");
                cmdUpd.Parameters.AddWithValue("@kid", c.KullaniciID);
                await cmdUpd.ExecuteNonQueryAsync();
            }

            await tx.CommitAsync();

            await DisplayAlert("Baþarýlý", "Çalýþan kaldýrýldý ve kullanýcý tipi Müþteri yapýldý.", "Tamam");

            // 3) Liste yenile
            await CalisanlariYukleAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", ex.Message, "Tamam");
        }
    }
}