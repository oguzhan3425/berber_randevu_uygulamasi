using System.Linq;
using System.Threading.Tasks;

namespace berber_randevu_uygulamasi.Views.AltBarlar;

public partial class CalisanAltBar : ContentView
{
    public CalisanAltBar()
    {
        InitializeComponent();
    }

    async void Panel_Tapped(object sender, TappedEventArgs e)
        => await GitVeSayfayiDegistir(new CalisanAnaSayfa());

    async void Randevu_Tapped(object sender, TappedEventArgs e)
        => await GitVeSayfayiDegistir(new CalisanRandevularSayfasi());

    async void Hizmet_Tapped(object sender, TappedEventArgs e)
        => await GitVeSayfayiDegistir(new CalisanHizmetlerSayfasi());

    async void Ayar_Tapped(object sender, TappedEventArgs e)
        => await GitVeSayfayiDegistir(new CalisanAyarlarSayfasi());

    static async Task GitVeSayfayiDegistir(Page hedefSayfa)
    {
        var mevcut = Application.Current?.Windows[0].Page?.Navigation?.NavigationStack?.LastOrDefault();
        if (mevcut?.GetType() == hedefSayfa.GetType())
            return;

        Application.Current!.Windows[0].Page = new NavigationPage(hedefSayfa);
        await Task.CompletedTask;
    }
}
