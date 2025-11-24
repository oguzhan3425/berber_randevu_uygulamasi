using Microsoft.Data.SqlClient;
using System.Collections.ObjectModel;

namespace berber_randevu_uygulamasi.Views
{
    public partial class RandevuAlSayfasi : ContentPage
    {
        private readonly string connectionString =
            "Data Source=localhost\\SQLEXPRESS;Initial Catalog=BerberDB;Integrated Security=True;TrustServerCertificate=True;";

        private string _ad = "";
        private string _soyad = "";

        // Listeyi tutmak için koleksiyon
        public ObservableCollection<string> BerberListesi { get; set; } = new();

        public RandevuAlSayfasi(string ad, string soyad)
        {
            InitializeComponent();

            _ad = ad;
            _soyad = soyad;

            BerberleriYukle();
        }

        // Parametresiz constructor (gerekirse)
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
                using SqlConnection conn = new(connectionString);
                await conn.OpenAsync();

                string query = "SELECT BerberAdi FROM Berberler";

                using SqlCommand cmd = new(query, conn);
                using SqlDataReader reader = await cmd.ExecuteReaderAsync();

                BerberListesi.Clear();

                while (await reader.ReadAsync())
                {
                    BerberListesi.Add(reader.GetString(0)); // BerberAdi
                }

                // XAML'deki CollectionView'e listeyi baðla
                BerberCollection.ItemsSource = BerberListesi;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", ex.Message, "Tamam");
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
