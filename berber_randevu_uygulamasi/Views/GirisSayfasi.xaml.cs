using Microsoft.Maui.Controls;
using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;

namespace berber_randevu_uygulamasi.Views
{
    public partial class GirisSayfasi : ContentPage
    {
        private readonly string connectionString =
            "Data Source=localhost\\SQLEXPRESS;Initial Catalog=BerberDB;Integrated Security=True;TrustServerCertificate=True;";

        public GirisSayfasi()
        {
            InitializeComponent();
        }

        private async void GirisYap_Clicked(object sender, EventArgs e)
        {
            string kullaniciAdi = kullaniciAdiEntry.Text;
            string sifre = SifreEntry.Text;

            if (string.IsNullOrWhiteSpace(kullaniciAdi) || string.IsNullOrWhiteSpace(sifre))
            {
                await DisplayAlert("Uyarý", "Lütfen kullanýcý adý ve þifreyi giriniz.", "Tamam");
                return;
            }

            try
            {
                using (SqlConnection baglanti = new SqlConnection(connectionString))
                {
                    await baglanti.OpenAsync();

                    string sorgu = "SELECT KullaniciTipi FROM Kullanici WHERE KullaniciAdi = @KullaniciAdi AND Sifre = @Sifre";

                    using (SqlCommand komut = new SqlCommand(sorgu, baglanti))
                    {
                        komut.Parameters.AddWithValue("@KullaniciAdi", kullaniciAdi);
                        komut.Parameters.AddWithValue("@Sifre", sifre);

                        object? tipObj = await komut.ExecuteScalarAsync();

                        if (tipObj != null)
                        {
                            string kullaniciTipi = tipObj.ToString() ?? "Musteri";
                            await DisplayAlert("Test", $"Kullanýcý tipi: {kullaniciTipi}", "Tamam");
                            await DisplayAlert("Baþarýlý", "Giriþ baþarýlý!", "Tamam");
                            // AnaSayfa constructor'unu senin yapýna göre düzenle
                            await Navigation.PushAsync(new AnaSayfa(kullaniciAdi, kullaniciTipi));
                        }
                        else
                        {
                            await DisplayAlert("Hata", "Kullanýcý adý veya þifre hatalý!", "Tekrar Dene");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", ex.Message, "Tamam");
            }
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
