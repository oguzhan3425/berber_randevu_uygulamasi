namespace berber_randevu_uygulamasi.Views.AltBarlar;

public partial class CalisanAltBar : ContentView
{
    public CalisanAltBar()
    {
        InitializeComponent();
    }

    private async Task Navigate(Page page)
    {
        if (this.Window?.Page is NavigationPage nav)
        {
            await nav.PushAsync(page);
        }
        else if (this.Window?.Page != null)
        {
            await this.Window.Page.Navigation.PushAsync(page);
        }
    }

    private async void AnaSayfa_Clicked(object sender, EventArgs e)
    {
        await Navigate(new Views.CalisanAnaSayfa());
    }

    private async void Randevular_Clicked(object sender, EventArgs e)
    {
        await Navigate(new Views.CalisanRandevularSayfasi());
    }

    private async void Profil_Clicked(object sender, EventArgs e)
    {
        await Navigate(new Views.CalisanProfilSayfasi());
    }
}