using System.Collections.ObjectModel;
using berber_randevu_uygulamasi.Models;

namespace berber_randevu_uygulamasi.Views;

public partial class CalisanHizmetlerSayfasi : ContentPage
{
    private readonly int _calisanId;

    public ObservableCollection<Hizmet> Hizmetler { get; } = new();

    public CalisanHizmetlerSayfasi(int calisanId = 0)
    {
        InitializeComponent();
        _calisanId = calisanId;

        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadHizmetlerAsync();
    }

    private async Task LoadHizmetlerAsync()
    {
        try
        {
            // TODO: DB’den hizmetleri çek (berbere/þubeye göre filtrelemek istiyorsan burada filtrele)
            Hizmetler.Clear();

            // Geçici örnek:
            Hizmetler.Add(new Hizmet { HizmetAdi = "Saç Kesim", Fiyat = 300, SureDakika = 30 });
            Hizmetler.Add(new Hizmet { HizmetAdi = "Sakal", Fiyat = 200, SureDakika = 20 });

            OnPropertyChanged(nameof(Hizmetler));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", ex.Message, "Tamam");
        }
    }
}