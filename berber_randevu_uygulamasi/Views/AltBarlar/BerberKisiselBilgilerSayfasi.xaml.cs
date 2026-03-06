using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using berber_randevu_uygulamasi.Services;
using berber_randevu_uygulamasi.Models.Dtos;

namespace berber_randevu_uygulamasi.Views.AltBarlar;

public partial class BerberKisiselBilgilerSayfasi : ContentPage
{
    protected readonly ApiClient _api;
    private int _berberId;

    private readonly bool _showBottomBar;

    // Geri Al için
    private string _ilkAd = "";
    private string _ilkSoyad = "";
    private string _ilkTelefon = "";
    private string _ilkEposta = "";
    private string _ilkKullaniciAdi = "";
    private string _ilkProfilUrl = "";

    private string _ilkBerberAdi = "";
    private string _ilkAdres = "";
    private string _ilkAcilis = "";
    private string _ilkKapanis = "";

    public BerberKisiselBilgilerSayfasi(ApiClient api, bool showBottomBar = false)
    {
        InitializeComponent();
        _showBottomBar = showBottomBar;

        btnKaydet.Clicked += Kaydet_Clicked;
        btnGeriAl.Clicked += GeriAl_Clicked;

        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await YukleAsync();
    }

    private async Task YukleAsync()
    {
        try
        {
            // 1) Kullanýcý profil
            var user = await _api.GetAsync<UserProfileDto>($"users/{UserSession.KullaniciId}/profile");
            if (user == null)
            {
                await DisplayAlert("Hata", "Kullanýcý bilgileri alýnamadý.", "Tamam");
                return;
            }

            // Session tip (boþsa doldur)
            UserSession.KullaniciTipi = (user.KullaniciTipi ?? "").Trim();

            await SayfayiRoleGoreHazirlaAsync(UserSession.KullaniciTipi);

            var ad = (user.Ad ?? "").Trim();
            var soyad = (user.Soyad ?? "").Trim();
            var tel = (user.Telefon ?? "").Trim();
            var eposta = (user.Eposta ?? "").Trim();
            var kullaniciAdi = (user.KullaniciAdi ?? "").Trim();
            var profilUrl = (user.ProfilFotoUrl ?? "").Trim();

            lblAdSoyad.Text = $"{ad} {soyad}".Trim();
            lblTelefonUst.Text = tel;

            txtAd.Text = ad;
            txtSoyad.Text = soyad;
            txtTelefon.Text = tel;
            txtEposta.Text = eposta;
            txtKullaniciAdi.Text = kullaniciAdi;

            _ilkProfilUrl = profilUrl;
            SetImageFromUrl(imgProfil, profilUrl, "default_user.png");

            _ilkAd = ad;
            _ilkSoyad = soyad;
            _ilkTelefon = tel;
            _ilkEposta = eposta;
            _ilkKullaniciAdi = kullaniciAdi;

            // 2) Dükkan bilgileri (berber/sahip ise)
            if (shopCard.IsVisible)
            {
                var berber = await _api.GetAsync<BerberDto>($"berberler/by-kullanici/{UserSession.KullaniciId}");
                if (berber == null)
                {
                    // Berber kartý görünür ama API kayýt bulamazsa, kullanýcýya net söyle
                    await DisplayAlert("Uyarý", "Bu kullanýcýya ait berber kaydý bulunamadý.", "Tamam");
                    return;
                }

                _berberId = berber.BerberId;

                txtBerberAdi.Text = berber.BerberAdi ?? "";
                txtAdres.Text = berber.Adres ?? "";
                txtAcilis.Text = berber.Acilis ?? "";
                txtKapanis.Text = berber.Kapanis ?? "";

                _ilkBerberAdi = txtBerberAdi.Text;
                _ilkAdres = txtAdres.Text;
                _ilkAcilis = txtAcilis.Text;
                _ilkKapanis = txtKapanis.Text;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", ex.Message, "Tamam");
        }
    }

    private async Task SayfayiRoleGoreHazirlaAsync(string tip)
    {
        tip = (tip ?? "").Trim();

        if (tip.Equals("Calisan", StringComparison.OrdinalIgnoreCase))
            lblRol.Text = "Rol: Çalýþan";
        else if (tip.Equals("Berber", StringComparison.OrdinalIgnoreCase) || tip.Equals("Sahip", StringComparison.OrdinalIgnoreCase))
            lblRol.Text = "Rol: Sahip";
        else
            lblRol.Text = "Rol: Müþteri";

        bool isBerber = tip.Equals("Berber", StringComparison.OrdinalIgnoreCase) ||
                        tip.Equals("Sahip", StringComparison.OrdinalIgnoreCase);

        shopCard.IsVisible = isBerber;

        if (!_showBottomBar)
        {
            BottomBarHost.IsVisible = false;
            return;
        }

        bool isMusteri = tip.Equals("Musteri", StringComparison.OrdinalIgnoreCase);
        BottomBarHost.IsVisible = !isMusteri;

        if (!isMusteri)
        {
            barCalisan.IsVisible = tip.Equals("Calisan", StringComparison.OrdinalIgnoreCase);
            barBerber.IsVisible = !barCalisan.IsVisible;
        }

        await Task.CompletedTask;
    }

    private static void SetImageFromUrl(Image img, string? url, string defaultImage)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(url) && Uri.TryCreate(url, UriKind.Absolute, out var uri))
                img.Source = ImageSource.FromUri(uri);
            else
                img.Source = defaultImage;
        }
        catch
        {
            img.Source = defaultImage;
        }
    }

    private async void Kaydet_Clicked(object? sender, EventArgs e)
    {
        try
        {
            var ad = (txtAd.Text ?? "").Trim();
            var soyad = (txtSoyad.Text ?? "").Trim();
            var tel = (txtTelefon.Text ?? "").Trim();
            var eposta = (txtEposta.Text ?? "").Trim();

            if (string.IsNullOrWhiteSpace(ad) || string.IsNullOrWhiteSpace(soyad))
            {
                await DisplayAlert("Uyarý", "Ad ve Soyad boþ olamaz.", "Tamam");
                return;
            }

            // 1) Kullanýcý update
            var okUser = await _api.PutAsync($"users/{UserSession.KullaniciId}", new UpdateUserRequest
            {
                Ad = ad,
                Soyad = soyad,
                Telefon = tel,
                Eposta = eposta
            });

            if (!okUser)
            {
                await DisplayAlert("Hata", "Kullanýcý bilgileri güncellenemedi.", "Tamam");
                return;
            }

            // 2) Berber update (sadece berber)
            if (shopCard.IsVisible)
            {
                var berberAdi = (txtBerberAdi.Text ?? "").Trim();
                var adres = (txtAdres.Text ?? "").Trim();
                var acilisText = (txtAcilis.Text ?? "").Trim();
                var kapanisText = (txtKapanis.Text ?? "").Trim();

                if (!TryParseTime(acilisText))
                {
                    await DisplayAlert("Uyarý", "Açýlýþ saati formatý hatalý. Örn: 09:00", "Tamam");
                    return;
                }
                if (!TryParseTime(kapanisText))
                {
                    await DisplayAlert("Uyarý", "Kapanýþ saati formatý hatalý. Örn: 21:00", "Tamam");
                    return;
                }

                if (_berberId <= 0)
                {
                    // güvenli: yeniden çek
                    var berber = await _api.GetAsync<BerberDto>($"berberler/by-kullanici/{UserSession.KullaniciId}");
                    if (berber == null)
                    {
                        await DisplayAlert("Hata", "Berber bilgisi alýnamadý.", "Tamam");
                        return;
                    }
                    _berberId = berber.BerberId;
                }

                var okShop = await _api.PutAsync($"berberler/{_berberId}", new UpdateBerberRequest
                {
                    BerberAdi = berberAdi,
                    Adres = adres,
                    Acilis = acilisText,
                    Kapanis = kapanisText
                });

                if (!okShop)
                {
                    await DisplayAlert("Hata", "Dükkan bilgileri güncellenemedi.", "Tamam");
                    return;
                }

                _ilkBerberAdi = berberAdi;
                _ilkAdres = adres;
                _ilkAcilis = acilisText;
                _ilkKapanis = kapanisText;
            }

            // UI + session
            lblAdSoyad.Text = $"{ad} {soyad}".Trim();
            lblTelefonUst.Text = tel;
            UserSession.Ad = ad;
            UserSession.Soyad = soyad;

            _ilkAd = ad;
            _ilkSoyad = soyad;
            _ilkTelefon = tel;
            _ilkEposta = eposta;

            await DisplayAlert("Baþarýlý", "Bilgiler güncellendi.", "Tamam");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", ex.Message, "Tamam");
        }
    }

    private async void GeriAl_Clicked(object? sender, EventArgs e)
    {
        txtAd.Text = _ilkAd;
        txtSoyad.Text = _ilkSoyad;
        txtTelefon.Text = _ilkTelefon;
        txtEposta.Text = _ilkEposta;
        txtKullaniciAdi.Text = _ilkKullaniciAdi;

        lblAdSoyad.Text = $"{_ilkAd} {_ilkSoyad}".Trim();
        lblTelefonUst.Text = _ilkTelefon;

        SetImageFromUrl(imgProfil, _ilkProfilUrl, "default_user.png");

        if (shopCard.IsVisible)
        {
            txtBerberAdi.Text = _ilkBerberAdi;
            txtAdres.Text = _ilkAdres;
            txtAcilis.Text = _ilkAcilis;
            txtKapanis.Text = _ilkKapanis;
        }

        await DisplayAlert("Bilgi", "Deðiþiklikler geri alýndý.", "Tamam");
    }

    private static bool TryParseTime(string text)
        => TimeSpan.TryParse(text, out _);
}