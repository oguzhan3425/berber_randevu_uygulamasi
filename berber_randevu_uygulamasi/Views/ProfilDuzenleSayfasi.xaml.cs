using System;
using Microsoft.Maui.Controls;
using Npgsql;
using berber_randevu_uygulamasi.Services;

namespace berber_randevu_uygulamasi.Views
{
    public partial class ProfilDuzenleSayfasi : ContentPage
    {
        public ProfilDuzenleSayfasi()
        {
            InitializeComponent();

            // Ýlk açýlýþta isim yaz
            if (this.FindByName<Label>("lblAdSoyad") is Label lbl)
                lbl.Text = $"{UserSession.Ad} {UserSession.Soyad}".Trim();

            // Ýlk açýlýþta foto çek
            KullaniciBilgileriniYukle();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Geri dönünce foto/telefon yenilensin
            KullaniciBilgileriniYukle();
        }

        private async void KullaniciBilgileriniYukle()
        {
            try
            {
                await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
                await conn.OpenAsync();

                string sql = @"SELECT ""ProfilFoto""
                               FROM kullanici
                               WHERE ""ID"" = @id
                               LIMIT 1;";

                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", UserSession.KullaniciId);

                object? fotoObj = await cmd.ExecuteScalarAsync();
                string? foto = fotoObj?.ToString();

                imgProfil.Source = string.IsNullOrWhiteSpace(foto)
                    ? "default_user.png"
                    : foto;
            }
            catch (Exception ex)
            {
                imgProfil.Source = "default_user.png";
                await DisplayAlert("Hata", ex.Message, "Tamam");
            }
        }

        // Alt bar - Butonlar
        private async void AnaSayfaClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AnaSayfa());
        }

        private async void RandevularClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RandevuAlSayfasi());
        }

        private async void ProfilClicked(object sender, EventArgs e)
        {
            await System.Threading.Tasks.Task.CompletedTask;
        }

        // Ayarlar
        private async void SifreDegistir_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AltBarlar.SifreDegistirSayfasi());
        }

        private async void ProfilFotoDegistir_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AltBarlar.ProfilFotoDegistirSayfasi());
        }

        private async void TelefonDegistir_Tapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AltBarlar.TelefonDegistirSayfasi());
        }
    }
}