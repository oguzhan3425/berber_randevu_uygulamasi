using berber_randevu_uygulamasi.Services;

namespace berber_randevu_uygulamasi.Views.AltBarlar;

public partial class BerberAltBar : ContentView
{
    protected readonly ApiClient _api;

    public BerberAltBar()
    {
        InitializeComponent();

        _api = Application.Current!
            .Handler!
            .MauiContext!
            .Services
            .GetService<ApiClient>()!;
    }
    public BerberAltBar(ApiClient api)
    {
        InitializeComponent();
        _api = api;
    }

    async void Panel_Tapped(object sender, TappedEventArgs e)
        => await GitVeSayfayiDegistir(new BerberAnaPanelSayfasi(_api));

    async void Randevu_Tapped(object sender, TappedEventArgs e)
        => await GitVeSayfayiDegistir(new BerberRandevularSayfasi(_api));

    async void Calisan_Tapped(object sender, TappedEventArgs e)
        => await GitVeSayfayiDegistir(new BerberCalisanlarSayfasi(_api));

    async void Hizmet_Tapped(object sender, TappedEventArgs e)
        => await GitVeSayfayiDegistir(new BerberHizmetlerSayfasi(_api));

    async void Ayar_Tapped(object sender, TappedEventArgs e)
        => await GitVeSayfayiDegistir(new BerberAyarlarSayfasi(_api));

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
