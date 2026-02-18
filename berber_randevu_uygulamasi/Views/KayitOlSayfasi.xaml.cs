using System;
using Microsoft.Maui.Controls;
using Npgsql;
using berber_randevu_uygulamasi.Services; // DbConfig burada değilse namespace'i düzelt

namespace berber_randevu_uygulamasi.Views
{
    public partial class KayitOlSayfasi : ContentPage
    {
        public KayitOlSayfasi()
        {
            InitializeComponent();
        }

        private async void KayitOl_Clicked(object sender, EventArgs e)
        {
            // Form alanlarını oku
            string Ad = AdEntry.Text?.Trim() ?? "";
            string Soyad = SoyadEntry.Text?.Trim() ?? "";
            string KullaniciAdi = KullaniciAdiEntry.Text?.Trim() ?? "";
            string Eposta = EmailEntry.Text?.Trim() ?? "";
            string Sifre = SifreEntry.Text ?? "";
            string Telefon = TelefonEntry.Text?.Trim() ?? "";

            Telefon = new string(Telefon.Where(char.IsDigit).ToArray());

            // Boş alan kontrolü
            if (string.IsNullOrWhiteSpace(Ad) ||
                string.IsNullOrWhiteSpace(Soyad) ||
                string.IsNullOrWhiteSpace(KullaniciAdi) ||
                string.IsNullOrWhiteSpace(Eposta) ||
                string.IsNullOrWhiteSpace(Sifre) ||
                string.IsNullOrWhiteSpace(Telefon))

            {
                await DisplayAlert("Uyarı", "Lütfen tüm alanları doldurunuz.", "Tamam");
                return;
            }

            try
            {
                await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
                await conn.OpenAsync();

                // Not: Eğer KullaniciTipi kolonu NOT NULL ise burada da göndermen gerekir.
                string sql = @"
                    INSERT INTO kullanici (""Ad"", ""Soyad"", ""KullaniciAdi"", ""Eposta"", ""Sifre"", ""KullaniciTipi"",""Telefon"")
                    VALUES (@Ad, @Soyad, @KullaniciAdi, @Eposta, @Sifre, @Tip, @Telefon);";

                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Ad", Ad);
                cmd.Parameters.AddWithValue("@Soyad", Soyad);
                cmd.Parameters.AddWithValue("@KullaniciAdi", KullaniciAdi);
                cmd.Parameters.AddWithValue("@Eposta", Eposta);
                cmd.Parameters.AddWithValue("@Sifre", Sifre);
                cmd.Parameters.AddWithValue("@Telefon", Telefon);
                cmd.Parameters.AddWithValue("@Tip", "Musteri"); // kayıt olan varsayılan müşteri olsun

                int sonuc = await cmd.ExecuteNonQueryAsync();

                if (sonuc > 0)
                {
                    await DisplayAlert("Başarılı", "Kayıt işlemi tamamlandı!", "Tamam");
                    await Navigation.PushAsync(new GirisSayfasi());
                }
                else
                {
                    await DisplayAlert("Hata", "Kayıt eklenemedi!", "Tamam");
                }
            }
            catch (PostgresException pex) when (pex.SqlState == "23505")
            {
                // Unique ihlali (ör: KullaniciAdi unique ise)
                await DisplayAlert("Hata", "Bu kullanıcı adı veya e-posta zaten kayıtlı.", "Tamam");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", "Veritabanı hatası: " + ex.Message, "Tamam");
            }
        }

        private async void GirisYap_Tapped(object sender, TappedEventArgs e)
        {
            await Navigation.PushAsync(new GirisSayfasi());
        }
    }
}
