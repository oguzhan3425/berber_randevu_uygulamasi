namespace berber_randevu_uygulamasi.Views.AltBarlar;

public partial class BerberAltBar : ContentView
{
    public BerberAltBar()
    {
        InitializeComponent();
    }

    async void Panel_Tapped(object sender, TappedEventArgs e)
        => await GitVeSayfayiDegistir(new BerberAnaPanelSayfasi());

    async void Randevu_Tapped(object sender, TappedEventArgs e)
        => await GitVeSayfayiDegistir(new BerberRandevularSayfasi());

    async void Calisan_Tapped(object sender, TappedEventArgs e)
        => await GitVeSayfayiDegistir(new BerberCalisanlarSayfasi());

    async void Hizmet_Tapped(object sender, TappedEventArgs e)
        => await GitVeSayfayiDegistir(new BerberHizmetlerSayfasi());

    async void Ayar_Tapped(object sender, TappedEventArgs e)
        => await GitVeSayfayiDegistir(new BerberAyarlarSayfasi());

    static async Task GitVeSayfayiDegistir(Page hedefSayfa)
    {
        // Ayný sayfadaysan boþuna tekrar push yapma
        var mevcut = Application.Current?.Windows[0].Page?.Navigation?.NavigationStack?.LastOrDefault();
        if (mevcut?.GetType() == hedefSayfa.GetType())
            return;

        // Burada “push” yerine “root deðiþtir” daha temiz olur (alt bar sabit hissettirir)
        Application.Current!.Windows[0].Page = new NavigationPage(hedefSayfa);
        await Task.CompletedTask;
    }
}
