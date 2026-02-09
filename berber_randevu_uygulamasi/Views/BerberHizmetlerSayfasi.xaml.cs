namespace berber_randevu_uygulamasi.Views; 
using berber_randevu_uygulamasi.Models;
public partial class BerberHizmetlerSayfasi : ContentPage
{
    List<Hizmet> _veri = new();

    public BerberHizmetlerSayfasi()
    {
        InitializeComponent();
        MockDoldur();
    }

    void MockDoldur()
    {
        _veri = new List<Hizmet>
        {
            new() { HizmetAdi="Saç Kesimi", SureDakika=20, Fiyat=150 },
            new() { HizmetAdi="Sakal", SureDakika=15, Fiyat=100 },
            new() { HizmetAdi="Saç + Sakal", SureDakika=30, Fiyat=220 },
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
