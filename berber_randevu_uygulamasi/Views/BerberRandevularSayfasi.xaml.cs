using berber_randevu_uygulamasi.Models;

namespace berber_randevu_uygulamasi.Views;

public partial class BerberRandevularSayfasi : ContentPage
{
    public BerberRandevularSayfasi()
    {
        InitializeComponent();
        SegmentAyarla(gelecek: true);
        MockListeDoldur();
    }

    void MockListeDoldur()
    {
        var gelecek = new List<RandevuKart>
{
    new() { RandevuID=1, SaatText="14:30", TarihText="01.02.2026", MusteriAdSoyad="Ahmet Yýlmaz", HizmetAdi="Saç + Sakal", ToplamUcret=220, MusteriFoto="default_user.png" },
    new() { RandevuID=2, SaatText="16:00", TarihText="01.02.2026", MusteriAdSoyad="Can Aydýn", HizmetAdi="Saç Kesimi", ToplamUcret=150, MusteriFoto="default_user.png" },
};

        var gecmis = new List<RandevuKart>
{
    new() { RandevuID=3, SaatText="12:00", TarihText="30.01.2026", MusteriAdSoyad="Emre Þahin", HizmetAdi="Sakal", DurumText="Tamamlandý", ToplamUcret=100, MusteriFoto="default_user.png" },
    new() { RandevuID=4, SaatText="13:00", TarihText="29.01.2026", MusteriAdSoyad="Mert Kaya", HizmetAdi="Saç Kesimi", DurumText="Ýptal", ToplamUcret=150, MusteriFoto="default_user.png" },
};

        listeGelecek.ItemsSource = gelecek;
        listeGecmis.ItemsSource = gecmis;
    }

    void SegmentAyarla(bool gelecek)
    {
        listeGelecek.IsVisible = gelecek;
        listeGecmis.IsVisible = !gelecek;

        btnGelecek.Opacity = gelecek ? 1 : 0.6;
        btnGecmis.Opacity = !gelecek ? 1 : 0.6;
    }

    void Gelecek_Clicked(object sender, EventArgs e) => SegmentAyarla(true);
    void Gecmis_Clicked(object sender, EventArgs e) => SegmentAyarla(false);

    void Yenile_Clicked(object sender, EventArgs e) => MockListeDoldur();

    async void Geldi_Clicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Randevu r)
            await DisplayAlert("Durum", $"{r.KullaniciID} geldi olarak iþaretlendi.", "Tamam");
    }

    async void Gelmedi_Clicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Randevu r)
            await DisplayAlert("Durum", $"{r.KullaniciID} gelmedi olarak iþaretlendi.", "Tamam");
    }
}
