namespace berber_randevu_uygulamasi.Views;

public partial class BerberHizmetlerSayfasi : ContentPage
{
    List<HizmetKart> _veri = new();

    public BerberHizmetlerSayfasi()
    {
        InitializeComponent();
        MockDoldur();
    }

    void MockDoldur()
    {
        _veri = new List<HizmetKart>
        {
            new("Saç Kesimi","20 dk","150 ?", true),
            new("Sakal","15 dk","100 ?", true),
            new("Saç + Sakal","30 dk","220 ?", true),
        };

        listeHizmet.ItemsSource = _veri;
    }

    async void Ekle_Clicked(object sender, EventArgs e)
        => await DisplayAlert("Ekle", "Hizmet ekleme ekranýný sonra baðlayacaðýz.", "Tamam");

    async void Duzenle_Clicked(object sender, EventArgs e)
    {
        if (sender is Button b && b.CommandParameter is HizmetKart h)
            await DisplayAlert("Düzenle", $"{h.Ad} düzenle.", "Tamam");
    }

    void AktifPasif_Clicked(object sender, EventArgs e)
    {
        if (sender is Button b && b.CommandParameter is HizmetKart h)
        {
            h.Aktif = !h.Aktif;
            listeHizmet.ItemsSource = null;
            listeHizmet.ItemsSource = _veri;
        }
    }

    public class HizmetKart
    {
        public HizmetKart(string ad, string sure, string fiyat, bool aktif)
        {
            Ad = ad; Sure = sure; Fiyat = fiyat; Aktif = aktif;
        }
        public string Ad { get; set; }
        public string Sure { get; set; }
        public string Fiyat { get; set; }
        public bool Aktif { get; set; }
        public string Durum => Aktif ? "Durum: Aktif" : "Durum: Pasif";
    }
}
