using Microsoft.Maui.Controls;
using Microsoft.Data.SqlClient; // SQL bağlantısı için gerekli
using System;
using System.Threading.Tasks;

namespace berber_randevu_uygulamasi.Views
{
    public partial class KayitOlSayfasi : ContentPage
    {
        // ✅ Windows Authentication için güvenli bağlantı (localhost kullan)
        private readonly string connectionString =
            "Data Source=localhost\\SQLEXPRESS;Initial Catalog=BerberDB;Integrated Security=True;TrustServerCertificate=True;";

        public KayitOlSayfasi()
        {
            InitializeComponent();
        }

        private async void KayitOl_Clicked(object sender, EventArgs e)
        {
            // 💡 Form alanlarını XAML'deki Entry adlarıyla eşleştir
            string ad = AdEntry.Text;
            string soyad = SoyadEntry.Text;
            string kullaniciAdi = KullaniciAdiEntry.Text;
            string eposta = EmailEntry.Text;
            string sifre = SifreEntry.Text;

            // 🧠 Boş alan kontrolü
            if (string.IsNullOrWhiteSpace(ad) || string.IsNullOrWhiteSpace(soyad) ||
                string.IsNullOrWhiteSpace(kullaniciAdi) ||
                string.IsNullOrWhiteSpace(eposta) || string.IsNullOrWhiteSpace(sifre))
            {
                await DisplayAlert("Uyarı", "Lütfen tüm alanları doldurunuz.", "Tamam");
                return;
            }

            try
            {
                using (SqlConnection baglanti = new SqlConnection(connectionString))
                {
                    await baglanti.OpenAsync();

                    

                    // 🧩 INSERT sorgusu
                    string sorgu = "INSERT INTO Kullanici (Ad, Soyad, KullaniciAdi, Eposta, Sifre) VALUES (@Ad, @Soyad, @KullaniciAdi, @Eposta, @Sifre)";

                    using (SqlCommand komut = new SqlCommand(sorgu, baglanti))
                    {
                        komut.Parameters.AddWithValue("@Ad", ad);
                        komut.Parameters.AddWithValue("@Soyad", soyad);
                        komut.Parameters.AddWithValue("@KullaniciAdi", kullaniciAdi);
                        komut.Parameters.AddWithValue("@Eposta", eposta);
                        komut.Parameters.AddWithValue("@Sifre", sifre);

                        int sonuc = await komut.ExecuteNonQueryAsync();

                        if (sonuc > 0)
                            await DisplayAlert("Başarılı", "Kayıt işlemi tamamlandı!", "Tamam");
                        else
                            await DisplayAlert("Hata", "Kayıt eklenemedi!", "Tamam");
                    }
                }

                // ✅ Başarılıysa giriş sayfasına yönlendir
                await Navigation.PushAsync(new GirisSayfasi());
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
