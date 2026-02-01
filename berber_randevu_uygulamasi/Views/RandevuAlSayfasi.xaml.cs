using Microsoft.Data.SqlClient;
using System.Collections.ObjectModel;
using berber_randevu_uygulamasi.Models;

namespace berber_randevu_uygulamasi.Views
{
    public partial class RandevuAlSayfasi : ContentPage
    {
        // Berber listesi artýk STRING deðil -> MODEL
        private List<Berber> tumBerberler = new List<Berber>();

        private readonly string connectionString =
            "Data Source=localhost\\SQLEXPRESS;Initial Catalog=BerberDB;Integrated Security=True;TrustServerCertificate=True;";

        private string _ad = "";
        private string _soyad = "";

        public RandevuAlSayfasi(string ad, string soyad)
        {
            InitializeComponent();

            _ad = ad;
            _soyad = soyad;

            BerberleriYukle();
        }

        public RandevuAlSayfasi()
        {
            InitializeComponent();
            BerberleriYukle();
        }

        // ---------------------------
        //  VERÝTABANINDAN BERBER ÇEKME
        // ---------------------------

        private async void BerberleriYukle()
        {
            try
            {
                tumBerberler.Clear();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();

                    string query =
                        "SELECT BerberID, BerberAdi, Adres, Telefon, ResimYolu, Puan, AcilisSaati, KapanisSaati FROM Berberler";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            string? dbResim = reader.IsDBNull(4) ? null : reader.GetString(4);
                            tumBerberler.Add(new Berber
                            {
                                BerberID = reader.GetInt32(0),
                                BerberAdi = reader.GetString(1),
                                Adres = reader.GetString(2),
                                Telefon = reader.GetString(3),
                                ResimYolu = string.IsNullOrWhiteSpace(dbResim)
                                        ? "default_berber.png"
                                        : dbResim,
                                Puan = reader.GetDecimal(5),
                                AcilisSaati = reader.GetTimeSpan(6),
                                KapanisSaati = reader.GetTimeSpan(7)
                            });
                        }
                    }
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
            string arama = e.NewTextValue?.ToLower() ?? "";

            var filtreliListe = tumBerberler
                .Where(b => b.BerberAdi.ToLower().Contains(arama))
                .ToList();

            BerberCollection.ItemsSource = filtreliListe;
        }

        private async void BerberCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Berber secilenBerber)
            {
                await Navigation.PushAsync(new CalisanSecimSayfasi(secilenBerber));
            }
        }


        // ---------------------------
        //  ALT BAR EVENTLERÝ
        // ---------------------------

        private async void AnaSayfaClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AnaSayfa(_ad, _soyad));
        }

        private async void RandevuAlClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RandevuAlSayfasi(_ad, _soyad));
        }

        private async void ProfilClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ProfilDuzenleSayfasi(_ad, _soyad));
        }
    }
}
