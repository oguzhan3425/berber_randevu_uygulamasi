using System;
using Microsoft.Maui.Controls;
using Npgsql;
using berber_randevu_uygulamasi.Services; // DbConfig burada değilse namespace'i düzelt

namespace berber_randevu_uygulamasi.Views
{
    public partial class GirisSayfasi : ContentPage
    {
        public GirisSayfasi()
        {
            InitializeComponent();
        }

        private async void GirisYap_Clicked(object sender, EventArgs e)
        {
            string kadi = kullaniciAdiEntry.Text?.Trim() ?? "";
            string sifre = SifreEntry.Text ?? "";

            if (string.IsNullOrWhiteSpace(kadi) || string.IsNullOrWhiteSpace(sifre))
            {
                await DisplayAlert("Uyarı", "Lütfen kullanıcı adı ve şifreyi giriniz.", "Tamam");
                return;
            }

            try
            {
                await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
                await conn.OpenAsync();

                string sql = @"
                    SELECT ""ID"", ""Ad"", ""Soyad"",""Telefon"", ""KullaniciTipi""
                    FROM kullanici
                    WHERE ""KullaniciAdi"" = @kadi AND ""Sifre"" = @sifre
                    LIMIT 1;";

                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@kadi", kadi);
                cmd.Parameters.AddWithValue("@sifre", sifre);

                await using var dr = await cmd.ExecuteReaderAsync();

                if (await dr.ReadAsync())
                {
                    int id = dr.GetInt32(dr.GetOrdinal("ID"));
                    string ad = dr.IsDBNull(dr.GetOrdinal("Ad")) ? "" : dr.GetString(dr.GetOrdinal("Ad"));
                    string soyad = dr.IsDBNull(dr.GetOrdinal("Soyad")) ? "" : dr.GetString(dr.GetOrdinal("Soyad"));
                    string tip = dr.IsDBNull(dr.GetOrdinal("KullaniciTipi")) ? "" : dr.GetString(dr.GetOrdinal("KullaniciTipi"));
                    string telefon = dr.IsDBNull(dr.GetOrdinal("Telefon")) ? "" : dr.GetString(dr.GetOrdinal("Telefon"));

                    UserSession.KullaniciId = id;
                    UserSession.Ad = ad;
                    UserSession.Soyad = soyad;
                    UserSession.KullaniciTipi = tip;
                    UserSession.Telefon = telefon; // UserSession'da alan varsa

                    string adSoyad = (ad + " " + soyad).Trim();

                    // 🎯 Rol bazlı yönlendirme
                    if (tip == "Berber")
                    {
                        await EnsureBerberAsCalisanAsync(id);  // ✅ kendini çalışan yap
                        await Navigation.PushAsync(new BerberAnaPanelSayfasi());
                        return;
                    }

                    if (tip == "Calisan")
                    {
                        await Navigation.PushAsync(new CalisanAnaSayfa());
                        return;
                    }

                    if (tip == "Musteri")
                    {
                        await Navigation.PushAsync(new AnaSayfa());
                        return;
                    }

                    await DisplayAlert("Hata", "Kullanıcı tipi tanımlanmamış.", "Tamam");
                }
                else
                {
                    await DisplayAlert("Hata", "Kullanıcı adı veya şifre hatalı!", "Tekrar Dene");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", ex.Message, "Tamam");
            }
        }

        private static async Task EnsureBerberAsCalisanAsync(int kullaniciId)
        {
            await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
            await conn.OpenAsync();

            string sql = @"
        INSERT INTO calisanlar (""KullaniciID"", ""BerberID"")
        SELECT b.""KullaniciID"", b.""BerberID""
        FROM ""Berberler"" b
        WHERE b.""KullaniciID"" = @kid
          AND NOT EXISTS (
              SELECT 1 FROM calisanlar c WHERE c.""KullaniciID"" = b.""KullaniciID""
          );";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@kid", kullaniciId);
            await cmd.ExecuteNonQueryAsync();
        }

        private async void DirektBerber_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new BerberAnaPanelSayfasi());
        }

        private async void KayitOl_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new KayitOlSayfasi());
        }

        private void KullaniciAdiEntry_Completed(object sender, EventArgs e)
        {
            SifreEntry.Focus();
        }

        private void SifreEntry_Completed(object sender, EventArgs e)
        {
            GirisYap_Clicked(sender, e);
        }
    }
}
