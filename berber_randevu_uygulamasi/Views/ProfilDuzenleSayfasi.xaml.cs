namespace berber_randevu_uygulamasi.Views;

public partial class ProfilDuzenleSayfasi : ContentPage
{
	string _ad;
	string _soyad;
    public ProfilDuzenleSayfasi(string ad, string soyad)
	{
		InitializeComponent();
        _ad = ad;
        _soyad = soyad;

        AdEntry.Text = ad;
        SoyadEntry.Text = soyad;
    }
	private async void AnaSayfaClicked(object sender, EventArgs e)
	{
        await Navigation.PushAsync(new AnaSayfa(_ad, _soyad));
    }
    private async void RandevuAlClicked(object sender, EventArgs e)
    {
        // Randevu alma sayfasýna yönlendirme
        await Navigation.PushAsync(new RandevuAlSayfasi());
    }
}