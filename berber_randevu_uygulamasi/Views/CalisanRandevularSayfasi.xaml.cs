using System.Collections.ObjectModel;
using berber_randevu_uygulamasi.Models;

namespace berber_randevu_uygulamasi.Views;

public partial class CalisanRandevularSayfasi : ContentPage
{
    public ObservableCollection<RandevuKart> Randevular { get; } = new();

    public string SeciliTarihText { get; set; } = DateTime.Today.ToString("dd.MM.yyyy");

    public CalisanRandevularSayfasi()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadRandevularAsync();
    }

    // ? Bu metot yoksa hata veriyordu. Artýk var.
    private async Task LoadRandevularAsync()
    {
        try
        {
            Randevular.Clear();

            // TODO: Buraya kendi DB sorgunu koy.
            // Þimdilik örnek data:
            Randevular.Add(new RandevuKart
            {
                RandevuID = 1,
                SaatText = "12:30",
                TarihText = SeciliTarihText,
                MusteriAdSoyad = "Müþteri 1",
                HizmetAdi = "Saç Kesim",
                ToplamUcret = 300,
                DurumText = "Bekliyor"
            });

            Randevular.Add(new RandevuKart
            {
                RandevuID = 2,
                SaatText = "13:15",
                TarihText = SeciliTarihText,
                MusteriAdSoyad = "Müþteri 2",
                HizmetAdi = "Sakal",
                ToplamUcret = 200,
                DurumText = "Bekliyor"
            });

            OnPropertyChanged(nameof(Randevular));
            OnPropertyChanged(nameof(SeciliTarihText));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", ex.Message, "Tamam");
        }
    }

    // ? XAML Clicked="Yenile_Clicked" bunu arýyordu.
    private async void Yenile_Clicked(object sender, EventArgs e)
    {
        await LoadRandevularAsync();
    }

    // XAML’de Detay_Clicked varsa (ben XAML’de koymuþtum), bu da dursun:
    private async void Detay_Clicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is RandevuKart kart)
        {
            await DisplayAlert("Randevu Detay",
                $"{kart.SaatText} - {kart.MusteriAdSoyad}\n{kart.HizmetAdi}\n{kart.ToplamUcret:0} ?\n{kart.DurumText}",
                "Tamam");
        }
    }
}