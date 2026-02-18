using berber_randevu_uygulamasi.Services;
using Microsoft.Maui.Controls;
using Npgsql;
using System;

namespace berber_randevu_uygulamasi.Views
{
    public partial class AnaSayfa : ContentPage
    {
        public AnaSayfa()
        {
            InitializeComponent();
            KullaniciBilgileriniYukle();
        }
        private async void KullaniciBilgileriniYukle()
        {
            if (!UserSession.IsLoggedIn)
                return;

            try
            {
                await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
                await conn.OpenAsync();

                string sql = @"
                    SELECT ""Ad"", ""Soyad"", ""ProfilFoto""
                    FROM kullanici
                    WHERE ""ID"" = @id;";

                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", UserSession.KullaniciId);

                await using var dr = await cmd.ExecuteReaderAsync();

                if (await dr.ReadAsync())
                {
                    string ad = dr.IsDBNull(0) ? "" : dr.GetString(0);
                    string soyad = dr.IsDBNull(1) ? "" : dr.GetString(1);
                    string foto = dr.IsDBNull(2) ? "" : dr.GetString(2);

                    lblAdSoyad.Text = $"{ad} {soyad}".Trim();

                    if (!string.IsNullOrWhiteSpace(foto))
                        imgProfil.Source = foto;
                    else
                        imgProfil.Source = "pp.png"; // default
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", ex.Message, "Tamam");
            }
        }
        private void GecmisRandevular_Tapped(object sender, TappedEventArgs e)
        {
            // geçiþ kodunu koyacam
        }

        private async void AnaSayfaClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AnaSayfa());
        }

        private async void RandevuAlClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RandevuAlSayfasi());
        }

        private void HizmetlerClicked(object sender, EventArgs e)
        {
            // geçiþ kodunu koyacam
        }

        private async void ProfilClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ProfilDuzenleSayfasi());
        }
    }
}
