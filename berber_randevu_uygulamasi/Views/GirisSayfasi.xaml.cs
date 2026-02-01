using Microsoft.Maui.Controls;
using Microsoft.Data.SqlClient;

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
            string kadi = kullaniciAdiEntry.Text;
            string sifre = SifreEntry.Text;

            if (string.IsNullOrWhiteSpace(kadi) || string.IsNullOrWhiteSpace(sifre))
            {
                await DisplayAlert("Uyarı", "Lütfen kullanıcı adı ve şifreyi giriniz.", "Tamam");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();

                    string sql = @"
                        SELECT ID, Ad, Soyad, KullaniciTipi
                        FROM Kullanici
                        WHERE KullaniciAdi = @kadi AND Sifre = @sifre";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@kadi", kadi);
                        cmd.Parameters.AddWithValue("@sifre", sifre);

                        using (SqlDataReader rd = await cmd.ExecuteReaderAsync())
                        {
                            if (await rd.ReadAsync())
                            {
                                int id = rd.GetInt32(0);
                                string ad = rd.GetString(1);
                                string soyad = rd.GetString(2);
                                string tip = rd.GetString(3);

                                string adSoyad = ad + " " + soyad;

                                // 🎯 Rol bazlı yönlendirme
                                if (tip == "Berber")
                                {
                                    await Navigation.PushAsync(new BerberAnaPanelSayfasi());
                                    return;
                                }

                               // if (tip == "Calisan")
                               // {
                               //     await Navigation.PushAsync(new CalisanAnaSayfa(id, adSoyad));
                               //     return;
                             //   }

                                if (tip == "Musteri")
                                {
                                    await Navigation.PushAsync(new AnaSayfa(tip, adSoyad));
                                    return;
                                }

                                await DisplayAlert("Hata", "Kullanıcı tipi tanımlanmamış.", "Tamam");
                            }
                            else
                            {
                                await DisplayAlert("Hata", "Kullanıcı adı veya şifre hatalı!", "Tekrar Dene");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", ex.Message, "Tamam");
            }
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
