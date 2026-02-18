using Npgsql;

namespace berber_randevu_uygulamasi.Views;

public partial class CalisanProfilSayfasi : ContentPage
{
    private readonly int _calisanId;

    public string? Foto { get; set; }
    public string? AdSoyad { get; set; }
    public string? Uzmanlik { get; set; }
    public string? RolText { get; set; }
    public string? Telefon { get; set; }

    public string MesaiBaslangicText { get; set; } = "09:00";
    public string MesaiBitisText { get; set; } = "21:00";

    public CalisanProfilSayfasi(int calisanId = 0)
    {
        InitializeComponent();
        _calisanId = calisanId;

        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadProfilAsync();
    }

    private Task LoadProfilAsync()
    {
        // TODO: DB’den CalisanKart + Kullanici bilgilerini çek (_calisanId ile)
        Foto = "user.png";
        AdSoyad = "Çalýþan";
        Uzmanlik = "Berber";
        RolText = "Rol: Çalýþan";
        Telefon = "05xx xxx xx xx";

        // Eðer DB’de mesai saatlerin varsa:
        // MesaiBaslangicText = ...
        // MesaiBitisText = ...

        OnPropertyChanged(nameof(Foto));
        OnPropertyChanged(nameof(AdSoyad));
        OnPropertyChanged(nameof(Uzmanlik));
        OnPropertyChanged(nameof(RolText));
        OnPropertyChanged(nameof(Telefon));
        OnPropertyChanged(nameof(MesaiBaslangicText));
        OnPropertyChanged(nameof(MesaiBitisText));

        return Task.CompletedTask;
    }

    private async void Cikis_Clicked(object sender, EventArgs e)
    {
        // TODO: oturum temizliði (Preferences / Session)
        // Sonra giriþ sayfasýna dön
        await Navigation.PopToRootAsync();
    }
}