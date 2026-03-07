using berber_randevu_uygulamasi.Models.Dtos;
using berber_randevu_uygulamasi.Services;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace berber_randevu_uygulamasi.Views.AltBarlar;

public partial class BerberCalismaSaatleriSayfasi : ContentPage
{
    protected readonly ApiClient _api;
    private int _calisanId;

    public BerberCalismaSaatleriSayfasi(ApiClient api)
    {
        InitializeComponent();

        swPzt.Toggled += (s, e) => SetDayEnabled(1, e.Value);
        swSal.Toggled += (s, e) => SetDayEnabled(2, e.Value);
        swCar.Toggled += (s, e) => SetDayEnabled(3, e.Value);
        swPer.Toggled += (s, e) => SetDayEnabled(4, e.Value);
        swCum.Toggled += (s, e) => SetDayEnabled(5, e.Value);
        swPaz.Toggled += (s, e) => SetDayEnabled(7, e.Value);

        btnKaydet.Clicked += BtnKaydet_Clicked;
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            var response = await _api.GetBerberCalismaSaatleriAsync(UserSession.KullaniciId);
            _calisanId = response.CalisanID;

            foreach (var gun in response.Gunler)
                ApplyToUi(gun.Gun, gun.AcikMi, gun.Acilis, gun.Kapanis);

            SetDayEnabled(1, swPzt.IsToggled);
            SetDayEnabled(2, swSal.IsToggled);
            SetDayEnabled(3, swCar.IsToggled);
            SetDayEnabled(4, swPer.IsToggled);
            SetDayEnabled(5, swCum.IsToggled);
            SetDayEnabled(7, swPaz.IsToggled);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", ex.Message, "Tamam");
        }
    }

    private void ApplyToUi(short gun, bool acikMi, string acilis, string kapanis)
    {
        switch (gun)
        {
            case 1:
                swPzt.IsToggled = acikMi;
                txtPztAcilis.Text = acilis;
                txtPztKapanis.Text = kapanis;
                break;

            case 2:
                swSal.IsToggled = acikMi;
                txtSalAcilis.Text = acilis;
                txtSalKapanis.Text = kapanis;
                break;

            case 3:
                swCar.IsToggled = acikMi;
                txtCarAcilis.Text = acilis;
                txtCarKapanis.Text = kapanis;
                break;

            case 4:
                swPer.IsToggled = acikMi;
                txtPerAcilis.Text = acilis;
                txtPerKapanis.Text = kapanis;
                break;

            case 5:
                swCum.IsToggled = acikMi;
                txtCumAcilis.Text = acilis;
                txtCumKapanis.Text = kapanis;
                break;

            case 7:
                swPaz.IsToggled = acikMi;
                txtPazAcilis.Text = acilis;
                txtPazKapanis.Text = kapanis;
                break;
        }
    }

    private void SetDayEnabled(short gun, bool enabled)
    {
        switch (gun)
        {
            case 1:
                txtPztAcilis.IsEnabled = enabled;
                txtPztKapanis.IsEnabled = enabled;
                break;
            case 2:
                txtSalAcilis.IsEnabled = enabled;
                txtSalKapanis.IsEnabled = enabled;
                break;
            case 3:
                txtCarAcilis.IsEnabled = enabled;
                txtCarKapanis.IsEnabled = enabled;
                break;
            case 4:
                txtPerAcilis.IsEnabled = enabled;
                txtPerKapanis.IsEnabled = enabled;
                break;
            case 5:
                txtCumAcilis.IsEnabled = enabled;
                txtCumKapanis.IsEnabled = enabled;
                break;
            case 7:
                txtPazAcilis.IsEnabled = enabled;
                txtPazKapanis.IsEnabled = enabled;
                break;
        }
    }

    private async void BtnKaydet_Clicked(object? sender, EventArgs e)
    {
        try
        {
            var gunler = new List<CalismaSaatiGunDto>
            {
                ReadFromUi(1, swPzt.IsToggled, txtPztAcilis.Text, txtPztKapanis.Text),
                ReadFromUi(2, swSal.IsToggled, txtSalAcilis.Text, txtSalKapanis.Text),
                ReadFromUi(3, swCar.IsToggled, txtCarAcilis.Text, txtCarKapanis.Text),
                ReadFromUi(4, swPer.IsToggled, txtPerAcilis.Text, txtPerKapanis.Text),
                ReadFromUi(5, swCum.IsToggled, txtCumAcilis.Text, txtCumKapanis.Text),
                ReadFromUi(7, swPaz.IsToggled, txtPazAcilis.Text, txtPazKapanis.Text)
            };

            var result = await _api.SaveBerberCalismaSaatleriAsync(new CalismaSaatleriKaydetRequest
            {
                KullaniciId = UserSession.KullaniciId,
                Gunler = gunler
            });

            if (!result.Success)
            {
                await DisplayAlert("Hata", result.Message, "Tamam");
                return;
            }

            await DisplayAlert("Baþarýlý", result.Message, "Tamam");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", ex.Message, "Tamam");
        }
    }

    private CalismaSaatiGunDto ReadFromUi(short gun, bool acikMi, string? acilisText, string? kapanisText)
    {
        if (!acikMi)
        {
            return new CalismaSaatiGunDto
            {
                Gun = gun,
                AcikMi = false,
                Acilis = "",
                Kapanis = ""
            };
        }

        if (!TryParseHHmm(acilisText, out var acilis))
            throw new Exception($"{GunAdi(gun)} açýlýþ saati hatalý (örn: 09:00).");

        if (!TryParseHHmm(kapanisText, out var kapanis))
            throw new Exception($"{GunAdi(gun)} kapanýþ saati hatalý (örn: 21:00).");

        if (acilis >= kapanis)
            throw new Exception($"{GunAdi(gun)} için açýlýþ kapanýþtan küçük olmalý.");

        return new CalismaSaatiGunDto
        {
            Gun = gun,
            AcikMi = true,
            Acilis = acilis.ToString(@"hh\:mm"),
            Kapanis = kapanis.ToString(@"hh\:mm")
        };
    }

    private static bool TryParseHHmm(string? text, out TimeSpan time)
    {
        time = default;
        if (string.IsNullOrWhiteSpace(text)) return false;
        return TimeSpan.TryParseExact(text.Trim(), @"hh\:mm", CultureInfo.InvariantCulture, out time);
    }

    private static string GunAdi(short gun) => gun switch
    {
        1 => "Pazartesi",
        2 => "Salý",
        3 => "Çarþamba",
        4 => "Perþembe",
        5 => "Cuma",
        6 => "Cumartesi",
        7 => "Pazar",
        _ => "Gün"
    };
}