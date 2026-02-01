namespace berber_randevu_uygulamasi.Views;

public partial class BerberAyarlarSayfasi : ContentPage
{
    public BerberAyarlarSayfasi()
    {
        InitializeComponent();
    }

    async void Kaydet_Clicked(object sender, EventArgs e)
    {
        // Þimdilik sadece gösteriyoruz. DB baðlayýnca burayý kayda çevirirsin.
        var acikMi = swPzt.IsToggled ? "Açýk" : "Kapalý";
        var ac = tpAc.Time.ToString(@"hh\:mm");
        var kapa = tpKapa.Time.ToString(@"hh\:mm");
        var kapasite = entryKapasite.Text;

        await DisplayAlert("Kaydedildi",
            $"Pazartesi: {acikMi}\nSaat: {ac} - {kapa}\nKapasite: {kapasite}",
            "Tamam");
    }
}
