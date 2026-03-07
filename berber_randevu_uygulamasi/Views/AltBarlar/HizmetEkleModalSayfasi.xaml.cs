using berber_randevu_uygulamasi.Models.Dtos;
using berber_randevu_uygulamasi.Services;

namespace berber_randevu_uygulamasi.Views.AltBarlar;

public partial class HizmetEkleModalSayfasi : ContentPage
{
    protected readonly ApiClient _api;
    private readonly int _berberId;
    private readonly int _calisanId;
    private readonly Func<Task>? _onSaved;

    public HizmetEkleModalSayfasi(ApiClient api, int berberId, int calisanId, Func<Task>? onSaved = null)
    {
        InitializeComponent();
        _api = api;
        _berberId = berberId;
        _calisanId = calisanId;
        _onSaved = onSaved;
    }

    private async void Kaydet_Clicked(object sender, EventArgs e)
    {
        string ad = txtAd.Text?.Trim() ?? "";

        if (!int.TryParse(txtSure.Text?.Trim(), out int sure) || sure <= 0)
        {
            await DisplayAlert("Uyarý", "Süre (dk) doðru gir.", "Tamam");
            return;
        }

        if (!decimal.TryParse(txtFiyat.Text?.Trim(), out decimal fiyat) || fiyat < 0)
        {
            await DisplayAlert("Uyarý", "Fiyat doðru gir.", "Tamam");
            return;
        }

        if (string.IsNullOrWhiteSpace(ad))
        {
            await DisplayAlert("Uyarý", "Hizmet adý boþ olamaz.", "Tamam");
            return;
        }

        try
        {
            var result = await _api.HizmetEkleAsync(new HizmetEkleRequest
            {
                HizmetAdi = ad,
                Fiyat = fiyat,
                SureDakika = sure,
                BerberID = _berberId,
                CalisanID = _calisanId
            });

            if (!result.Success)
            {
                await DisplayAlert("Hata", result.Message, "Tamam");
                return;
            }

            await DisplayAlert("Baþarýlý", result.Message, "Tamam");

            if (_onSaved != null)
                await _onSaved();

            await Navigation.PopModalAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", ex.Message, "Tamam");
        }
    }

    private async void Vazgec_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}