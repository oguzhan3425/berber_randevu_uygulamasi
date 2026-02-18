using System;
using Microsoft.Maui.Controls;
using Npgsql;
using berber_randevu_uygulamasi.Services;

namespace berber_randevu_uygulamasi.Views.AltBarlar
{
    public partial class SifreDegistirSayfasi : ContentPage
    {
        public SifreDegistirSayfasi()
        {
            InitializeComponent();
        }

        private async void SifreGuncelle_Clicked(object sender, EventArgs e)
        {
            string guncel = GuncelSifreEntry.Text ?? "";
            string yeni = YeniSifreEntry.Text ?? "";
            string yeniTekrar = YeniSifreTekrarEntry.Text ?? "";

            if (string.IsNullOrWhiteSpace(guncel) ||
                string.IsNullOrWhiteSpace(yeni) ||
                string.IsNullOrWhiteSpace(yeniTekrar))
            {
                await DisplayAlert("Uyarý", "Lütfen tüm alanlarý doldurun.", "Tamam");
                return;
            }

            if (yeni != yeniTekrar)
            {
                await DisplayAlert("Uyarý", "Yeni þifreler eþleþmiyor.", "Tamam");
                return;
            }

            if (yeni.Length < 6)
            {
                await DisplayAlert("Uyarý", "Yeni þifre en az 6 karakter olmalý.", "Tamam");
                return;
            }

            try
            {
                await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
                await conn.OpenAsync();

                // 1) DB'den mevcut þifreyi al
                string selectSql = @"SELECT ""Sifre""
                                     FROM kullanici
                                     WHERE ""ID"" = @id
                                     LIMIT 1;";
                await using var selectCmd = new NpgsqlCommand(selectSql, conn);
                selectCmd.Parameters.AddWithValue("@id", UserSession.KullaniciId);

                var dbSifreObj = await selectCmd.ExecuteScalarAsync();
                if (dbSifreObj == null)
                {
                    await DisplayAlert("Hata", "Kullanýcý bulunamadý.", "Tamam");
                    return;
                }

                string dbSifre = dbSifreObj.ToString() ?? "";
                if (dbSifre != guncel)
                {
                    await DisplayAlert("Hata", "Güncel þifre yanlýþ.", "Tamam");
                    return;
                }

                // 2) Update
                string updateSql = @"UPDATE kullanici
                                     SET ""Sifre"" = @yeni
                                     WHERE ""ID"" = @id;";
                await using var updateCmd = new NpgsqlCommand(updateSql, conn);
                updateCmd.Parameters.AddWithValue("@yeni", yeni);
                updateCmd.Parameters.AddWithValue("@id", UserSession.KullaniciId);

                int affected = await updateCmd.ExecuteNonQueryAsync();
                if (affected > 0)
                {
                    await DisplayAlert("Baþarýlý", "Þifreniz güncellendi.", "Tamam");
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Hata", "Þifre güncellenemedi.", "Tamam");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", ex.Message, "Tamam");
            }
        }
    }
}