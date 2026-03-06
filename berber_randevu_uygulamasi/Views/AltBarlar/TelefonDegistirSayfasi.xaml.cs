using System;
using Microsoft.Maui.Controls;
using Npgsql;
using berber_randevu_uygulamasi.Services;

namespace berber_randevu_uygulamasi.Views.AltBarlar
{
    public partial class TelefonDegistirSayfasi : ContentPage
    {
        protected readonly ApiClient _api;

        public TelefonDegistirSayfasi(ApiClient api)
        {
            InitializeComponent();
            _api = api;
        }

        private async void TelefonGuncelle_Clicked(object sender, EventArgs e)
        {
            string tel = TelefonEntry.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(tel))
            {
                await DisplayAlert("Uyarý", "Telefon numarasýný girin.", "Tamam");
                return;
            }

            // Basit kontrol (istersen daha sýký regex yaparýz)
            if (tel.Length < 10)
            {
                await DisplayAlert("Uyarý", "Telefon numarasý çok kýsa.", "Tamam");
                return;
            }

            try
            {
                await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
                await conn.OpenAsync();

                string sql = @"UPDATE kullanici
                               SET ""Telefon"" = @tel
                               WHERE ""ID"" = @id;";

                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@tel", tel);
                cmd.Parameters.AddWithValue("@id", UserSession.KullaniciId);

                int affected = await cmd.ExecuteNonQueryAsync();
                if (affected > 0)
                {
                    await DisplayAlert("Baþarýlý", "Telefon güncellendi.", "Tamam");
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Hata", "Güncelleme baþarýsýz.", "Tamam");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", ex.Message, "Tamam");
            }
        }
    }
}