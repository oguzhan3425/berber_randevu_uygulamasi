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
            new("14:30","Ahmet Yýlmaz","Saç + Sakal","01 Þub 2026"),
            new("16:00","Can Aydýn","Saç Kesimi","01 Þub 2026"),
        };

        var gecmis = new List<RandevuKart>
        {
            new("12:00","Emre Þahin","Sakal","Tamamlandý"),
            new("13:00","Mert Kaya","Saç Kesimi","Ýptal"),
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
        if (sender is Button btn && btn.CommandParameter is RandevuKart r)
            await DisplayAlert("Durum", $"{r.Musteri} geldi olarak iþaretlendi.", "Tamam");
    }

    async void Gelmedi_Clicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is RandevuKart r)
            await DisplayAlert("Durum", $"{r.Musteri} gelmedi olarak iþaretlendi.", "Tamam");
    }

    public record RandevuKart(string Saat, string Musteri, string Hizmet, string TarihOrDurum)
    {
        public string Saat { get; init; } = Saat;
        public string Musteri { get; init; } = Musteri;
        public string Hizmet { get; init; } = Hizmet;

        // Gelecek listesi "Tarih", geçmiþ listesi "Durum" gösteriyor
        public string Tarih { get; init; } = TarihOrDurum;
        public string Durum { get; init; } = TarihOrDurum;
    }
}
