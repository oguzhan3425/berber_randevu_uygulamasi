using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Npgsql;
using berber_randevu_uygulamasi.Models;
using berber_randevu_uygulamasi.Services;

namespace berber_randevu_uygulamasi.Views
{
    public partial class CalisanSecimSayfasi : ContentPage
    {
        private readonly Berber _berber;

        public CalisanSecimSayfasi(Berber berber)
        {
            InitializeComponent();
            _berber = berber;
            _ = CalisanlariYukleAsync();
        }

        private async System.Threading.Tasks.Task CalisanlariYukleAsync()
        {
            List<CalisanKart> liste = new();

            try
            {
                await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
                await conn.OpenAsync();

                // ? Calisanlar tablosu: CalisanID, KullaniciID, BerberID
                // ? Ad/Soyad kullanýcý tablosundan alýnýr
                string sql = @"
                    SELECT
                        c.""CalisanID"",
                        c.""BerberID"",
                        k.""Ad"",
                        k.""Soyad"",
                        k.""ProfilFoto""
                    FROM ""calisanlar"" c
                    JOIN kullanici k ON k.""ID"" = c.""KullaniciID""
                    WHERE c.""BerberID"" = @bid
                    ORDER BY k.""Ad"", k.""Soyad"";";

                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@bid", _berber.BerberID);

                await using var dr = await cmd.ExecuteReaderAsync();

                while (await dr.ReadAsync())
                {
                    int calisanId = dr.GetInt32(0);
                    int berberId = dr.GetInt32(1);
                    string ad = dr.IsDBNull(2) ? "" : dr.GetString(2);
                    string soyad = dr.IsDBNull(3) ? "" : dr.GetString(3);
                    string foto = dr.IsDBNull(4) ? "" : dr.GetString(4);

                    liste.Add(new CalisanKart
                    {
                        CalisanID = calisanId,
                        BerberID = berberId,
                        Ad = ad,
                        Soyad = soyad,

                        // Senin DB’de "DeneyimYili / Uzmanlik" yok.
                        // Þimdilik boþ býrakýyoruz. Ýstersen tabloya ekleriz.
                        Uzmanlik = "",

                        Foto = string.IsNullOrWhiteSpace(foto) ? "default_berber.png" : foto
                    });
                }

                CalisanCollection.ItemsSource = liste;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", "Çalýþanlar yüklenemedi:\n" + ex.Message, "Tamam");
            }
        }

        private async void CalisanCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is CalisanKart secilen)
            {
                CalisanCollection.SelectedItem = null;

                

                await Navigation.PushAsync(new RandevuOlusturSayfasi(secilen));
            }
        }
    }
}