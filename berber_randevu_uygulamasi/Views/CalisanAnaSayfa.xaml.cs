using System.Collections.ObjectModel;
using berber_randevu_uygulamasi.Models;

namespace berber_randevu_uygulamasi.Views;

public partial class CalisanAnaSayfa : ContentPage
{
    // ÖNEMLÝ: Giriþte belirlediðin çalýþan id’yi buraya set et.
    // Sen daha önce yönlendirmeyi yaptým dediðin için:
    // App.Current.Properties / Preferences / static Session gibi nerede tutuyorsan oradan okuyabilirsin.
    private readonly int _calisanId;

    public ObservableCollection<RandevuKart> BugunRandevular { get; } = new();

    public int BugunRandevuSayisi { get; set; }
    public decimal BugunKazanc { get; set; }
    

    public RandevuKart? SiradakiRandevu { get; set; }

    // Üst þerit alanlarý (CalisanKart’tan da besleyebilirsin)
    public string? Foto { get; set; }
    public string? AdSoyad { get; set; }
    public string? Uzmanlik { get; set; }
    public string DurumText { get; set; } = "Aktif";

    public CalisanAnaSayfa(int calisanId = 0)
    {
        InitializeComponent();

        _calisanId = calisanId;

        // Binding kaynak: sayfanýn kendisi
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDashboardAsync();
    }

    private async Task LoadDashboardAsync()
    {
        try
        {
            // 1) Profil / çalýþan bilgisi (Foto, AdSoyad, Uzmanlik, DurumText)
            await LoadCalisanInfoAsync();

            // 2) Bugünkü randevular (RandevuKart listesi)
            await LoadBugunRandevularAsync();

            // 3) Özet hesapla
            BugunRandevuSayisi = BugunRandevular.Count;
            BugunKazanc = BugunRandevular.Sum(x => x.ToplamUcret);
            

            // 4) Sýradaki randevu
            SiradakiRandevu = BugunRandevular
                
                .FirstOrDefault();

            // UI güncelle
            OnPropertyChanged(nameof(BugunRandevuSayisi));
            OnPropertyChanged(nameof(BugunKazanc));
            OnPropertyChanged(nameof(SiradakiRandevu));
            OnPropertyChanged(nameof(Foto));
            OnPropertyChanged(nameof(AdSoyad));
            OnPropertyChanged(nameof(Uzmanlik));
            OnPropertyChanged(nameof(DurumText));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", ex.Message, "Tamam");
        }
    }

    private Task LoadCalisanInfoAsync()
    {
        // TODO: Buraya kendi DB sorgunu koy:
        // CalisanKart tablosundan _calisanId’ye göre Ad, Soyad, Uzmanlik, Foto, Durum/Aktif vs.

        // Geçici örnek:
        Foto = "user.png";
        AdSoyad = "Çalýþan";
        Uzmanlik = "Berber";
        DurumText = "Aktif";

        return Task.CompletedTask;
    }

    private Task LoadBugunRandevularAsync()
    {
        // TODO: Buraya kendi DB sorgunu koy:
        // Bugünün randevularý: Randevu + Hizmet + Kullanici join
        // Sonuçlarý RandevuKart’a map et.

        BugunRandevular.Clear();

        // Geçici örnek:
        BugunRandevular.Add(new RandevuKart
        {
            RandevuID = 1,
            SaatText = "12:30",
            TarihText = DateTime.Today.ToString("dd.MM.yyyy"),
            MusteriAdSoyad = "Müþteri 1",
            HizmetAdi = "Saç Kesim",
            ToplamUcret = 300,
            DurumText = "Bekliyor"
        });

        BugunRandevular.Add(new RandevuKart
        {
            RandevuID = 2,
            SaatText = "13:15",
            TarihText = DateTime.Today.ToString("dd.MM.yyyy"),
            MusteriAdSoyad = "Müþteri 2",
            HizmetAdi = "Sakal",
            ToplamUcret = 200,
            DurumText = "Bekliyor"
        });

        OnPropertyChanged(nameof(BugunRandevular));
        return Task.CompletedTask;
    }

    private async void TumRandevular_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CalisanRandevularSayfasi());
    }
}