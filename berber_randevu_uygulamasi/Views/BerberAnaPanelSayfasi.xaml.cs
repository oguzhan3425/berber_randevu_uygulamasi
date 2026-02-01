using berber_randevu_uygulamasi.Views;

namespace berber_randevu_uygulamasi.Views;

public partial class BerberAnaPanelSayfasi : ContentPage
{
    public BerberAnaPanelSayfasi()
    {
        InitializeComponent();
        MockVeriDoldur();
    }

    void MockVeriDoldur()
    {
        // Þimdilik sabit; DB baðlayýnca burayý dinamik yaparsýn
        lblBugunRandevu.Text = "5";
        lblSiradakiIsim.Text = "Ahmet Yýlmaz";
        lblSiradakiSaat.Text = "14:30";
    }

    void BugunRandevular_Clicked(object sender, EventArgs e)
        => Application.Current!.Windows[0].Page = new NavigationPage(new BerberRandevularSayfasi());

    void CalismaSaatleri_Clicked(object sender, EventArgs e)
        => Application.Current!.Windows[0].Page = new NavigationPage(new BerberRandevularSayfasi());
}
