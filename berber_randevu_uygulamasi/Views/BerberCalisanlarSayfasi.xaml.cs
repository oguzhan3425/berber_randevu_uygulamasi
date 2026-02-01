namespace berber_randevu_uygulamasi.Views;

public partial class BerberCalisanlarSayfasi : ContentPage
{
    List<CalisanKart> _veri = new();

    public BerberCalisanlarSayfasi()
    {
        InitializeComponent();
        MockDoldur();
    }

    void MockDoldur()
    {
        _veri = new List<CalisanKart>
        {
            new("Mehmet Kaya","Kýdemli","05xx xxx xx xx", true),
            new("Ali Demir","Kalfa","05xx xxx xx xx", true),
            new("Kaan Yýldýz","Çýrak","05xx xxx xx xx", false),
        };

        listeCalisan.ItemsSource = _veri;
    }

    async void Ekle_Clicked(object sender, EventArgs e)
        => await DisplayAlert("Ekle", "Çalýþan ekleme ekranýný sonra baðlayacaðýz.", "Tamam");

    async void Duzenle_Clicked(object sender, EventArgs e)
    {
        if (sender is Button b && b.CommandParameter is CalisanKart c)
            await DisplayAlert("Düzenle", $"{c.AdSoyad} düzenle.", "Tamam");
    }

    void AktifPasif_Clicked(object sender, EventArgs e)
    {
        if (sender is Button b && b.CommandParameter is CalisanKart c)
        {
            c.Aktif = !c.Aktif;
            listeCalisan.ItemsSource = null;
            listeCalisan.ItemsSource = _veri;
        }
    }

    public class CalisanKart
    {
        public CalisanKart(string adSoyad, string rol, string telefon, bool aktif)
        {
            AdSoyad = adSoyad;
            Rol = rol;
            Telefon = telefon;
            Aktif = aktif;
        }

        public string AdSoyad { get; set; }
        public string Rol { get; set; }
        public string Telefon { get; set; }
        public bool Aktif { get; set; }
        public string Durum => Aktif ? "Durum: Aktif" : "Durum: Pasif";
    }
}
