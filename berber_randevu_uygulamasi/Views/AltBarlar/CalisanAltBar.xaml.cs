using berber_randevu_uygulamasi.Services;

namespace berber_randevu_uygulamasi.Views.AltBarlar;

public partial class CalisanAltBar : ContentView
{
    protected readonly ApiClient _api;

    public CalisanAltBar()
    {
        InitializeComponent();

        _api = Application.Current!
            .Handler!
            .MauiContext!
            .Services
            .GetService<ApiClient>()!;
    }

    public CalisanAltBar(ApiClient api)
    {
        InitializeComponent();
        _api = api;
    }

    async void Panel_Tapped(object sender, TappedEventArgs e)
        => await GitVeSayfayiDegistir(new CalisanAnaSayfa(_api));

    async void Randevu_Tapped(object sender, TappedEventArgs e)
        => await GitVeSayfayiDegistir(new CalisanRandevularSayfasi(_api));

    async void Hizmet_Tapped(object sender, TappedEventArgs e)
        => await GitVeSayfayiDegistir(new CalisanHizmetlerSayfasi(_api));

    async void Ayar_Tapped(object sender, TappedEventArgs e)
        => await GitVeSayfayiDegistir(new CalisanAyarlarSayfasi(_api));

    private static Task GitVeSayfayiDegistir(Page hedefSayfa)
    {
        var mevcut = Application.Current?.Windows[0].Page?.Navigation?.NavigationStack?.LastOrDefault();
        if (mevcut?.GetType() == hedefSayfa.GetType())
            return Task.CompletedTask;

        Application.Current!.Windows[0].Page = new NavigationPage(hedefSayfa);
        return Task.CompletedTask;
    }
}