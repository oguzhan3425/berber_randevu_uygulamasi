using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Npgsql;
using berber_randevu_uygulamasi.Models;
using berber_randevu_uygulamasi.Services;

namespace berber_randevu_uygulamasi.Views
{
    public partial class RandevuAlSayfasi : ContentPage
    {
        private readonly List<Berber> tumBerberler = new();

        public RandevuAlSayfasi()
        {
            InitializeComponent();
            _ = BerberleriYukleAsync();
        }

        // ---------------------------
        //  VERÝTABANINDAN BERBER ÇEKME
        // ---------------------------
        private async Task BerberleriYukleAsync()
        {
            try
            {
                tumBerberler.Clear();

                await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
                await conn.OpenAsync();

                // ? PostgreSQL + Büyük harfli tablo/kolon isimleri (týrnaklý)
                // Eðer þema kullanýyorsan: FROM public."Berberler" veya "berber"."Berberler"
                string sql = @"
                    SELECT 
                        ""BerberID"", ""BerberAdi"", ""Adres"", ""Telefon"", 
                        ""ResimYolu"", ""Puan"", ""AcilisSaati"", ""KapanisSaati""
                    FROM ""Berberler""
                    ORDER BY ""BerberAdi"";";

                await using var cmd = new NpgsqlCommand(sql, conn);
                await using var dr = await cmd.ExecuteReaderAsync();

                while (await dr.ReadAsync())
                {
                    string? dbResim = dr.IsDBNull(4) ? null : dr.GetString(4);

                    tumBerberler.Add(new Berber
                    {
                        BerberID = dr.GetInt32(0),
                        BerberAdi = dr.IsDBNull(1) ? "" : dr.GetString(1),
                        Adres = dr.IsDBNull(2) ? "" : dr.GetString(2),
                        Telefon = dr.IsDBNull(3) ? "" : dr.GetString(3),
                        ResimYolu = string.IsNullOrWhiteSpace(dbResim) ? "default_berber.png" : dbResim,
                        Puan = dr.IsDBNull(5) ? 0 : dr.GetDecimal(5),
                        AcilisSaati = dr.IsDBNull(6) ? TimeSpan.Zero : dr.GetTimeSpan(6),
                        KapanisSaati = dr.IsDBNull(7) ? TimeSpan.Zero : dr.GetTimeSpan(7)
                    });
                }

                BerberCollection.ItemsSource = tumBerberler;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", "Berberler yüklenemedi: " + ex.Message, "Tamam");
            }
        }

        // ---------------------------
        //  ARAMA
        // ---------------------------
        private void BerberAraEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            string arama = e.NewTextValue?.Trim().ToLower() ?? "";

            var filtreliListe = tumBerberler
                .Where(b => (b.BerberAdi ?? "").ToLower().Contains(arama))
                .ToList();

            BerberCollection.ItemsSource = filtreliListe;
        }

        // ---------------------------
        //  BERBER SEÇÝMÝ
        // ---------------------------
        private async void BerberCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Berber secilenBerber)
            {
                BerberCollection.SelectedItem = null;
                await Navigation.PushAsync(new CalisanSecimSayfasi(secilenBerber));
            }
        }

        // ---------------------------
        //  ALT BAR
        // ---------------------------
        private async void AnaSayfaClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AnaSayfa());
        }

        private async void RandevuAlClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RandevuAlSayfasi());
        }

        private async void ProfilClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ProfilDuzenleSayfasi());
        }
    }
}
