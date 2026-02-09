using berber_randevu_uygulamasi.Models;
using Microsoft.Data.SqlClient;

namespace berber_randevu_uygulamasi.Views
{
    public partial class CalisanSecimSayfasi : ContentPage
    {
        private readonly string connectionString =
            "Data Source=localhost\\SQLEXPRESS;Initial Catalog=BerberDB;Integrated Security=True;TrustServerCertificate=True;";

        private Berber _berber;

        public CalisanSecimSayfasi(Berber berber)
        {
            InitializeComponent();
            _berber = berber;
            CalisanlariYukle();
        }

        // --- ÇALIÞANLARI YÜKLE ---
        private async void CalisanlariYukle()
        {
            List<CalisanKart> liste = new();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();

                    string sql = @"
                    SELECT 
                        c.CalisanID,
                        c.KullaniciID,
                        c.BerberID,
                        c.Uzmanlik,
                        k.Ad,
                        k.Soyad,
                        'default_berber.png' AS Foto
                    FROM CalisanKart c
                    INNER JOIN Kullanici k ON c.KullaniciID = k.ID
                    WHERE c.BerberID = @bid";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@bid", _berber.BerberID);

                        using (SqlDataReader rd = await cmd.ExecuteReaderAsync())
                        {
                            while (await rd.ReadAsync())
                            {
                                liste.Add(new CalisanKart
                                {
                                    CalisanID = rd.GetInt32(0),
                                    KullaniciID = rd.GetInt32(1),
                                    BerberID = rd.GetInt32(2),
                                    Uzmanlik = rd.IsDBNull(3) ? "" : rd.GetString(3),
                                    Ad = rd.GetString(4),
                                    Soyad = rd.GetString(5),
                                    Foto = rd.GetString(6)
                                });
                            }
                        }
                    }
                }

                CalisanCollection.ItemsSource = liste;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", "Çalýþanlar yüklenemedi:\n" + ex.Message, "Tamam");
            }
        }

        // --- ÇALIÞAN SEÇÝMÝ ---
        private async void CalisanCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is CalisanKart secilen)
            {
                // Þimdilik çalýþan bilgisi gösterelim
                await DisplayAlert("Seçilen Çalýþan",
                    $"{secilen.Ad} {secilen.Soyad}\nUzmanlýk: {secilen.Uzmanlik}",
                    "Tamam");

                // Bir sonraki sayfaya geçeceðiz:
                // await Navigation.PushAsync(new HizmetSecimSayfasi(secilen));
            }
        }
    }
}
