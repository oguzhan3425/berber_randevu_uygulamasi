using berber_randevu_uygulamasi.Models;

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
            new CalisanKart { Ad="Mehmet", Soyad="Kaya", Rol="Kýdemli", Telefon="05xx xxx xx xx", Aktif=true },
            new CalisanKart { Ad="Ali", Soyad="Demir", Rol="Kalfa", Telefon="05xx xxx xx xx", Aktif=true },
            new CalisanKart { Ad="Kaan", Soyad="Yýldýz", Rol="Çýrak", Telefon="05xx xxx xx xx", Aktif=false },
        };

        listeCalisan.ItemsSource = _veri;
    }

    async void Ekle_Clicked(object sender, EventArgs e)
        => await DisplayAlert("Ekle", "Çalýþan ekleme ekranýný sonra baðlayacaðýz.", "Tamam");

    async void Duzenle_Clicked(object sender, EventArgs e)
    {
        if (sender is Button b && b.CommandParameter is CalisanKart c)
            await DisplayAlert("Düzenle", $"{c.Ad} {c.Soyad} düzenle.", "Tamam");
    }

    void AktifPasif_Clicked(object sender, EventArgs e)
    {
        if (sender is Button b && b.CommandParameter is CalisanKart c)
        {
            c.Aktif = !c.Aktif;

            // Basit yenileme
            listeCalisan.ItemsSource = null;
            listeCalisan.ItemsSource = _veri;
        }
    }
}
