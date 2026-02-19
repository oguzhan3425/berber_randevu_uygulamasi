using System;
using System.IO;
using System.Threading.Tasks;
using Npgsql;
using berber_randevu_uygulamasi.Services;

namespace berber_randevu_uygulamasi.Views.AltBarlar;

public partial class BerberKisiselBilgilerSayfasi : ContentPage
{
    private int _berberId;

    // Opsiyonel: alt bar gösterilsin mi?
    private readonly bool _showBottomBar;

    // Geri Al için
    private string _ilkAd = "";
    private string _ilkSoyad = "";
    private string _ilkTelefon = "";
    private string _ilkEposta = "";
    private string _ilkKullaniciAdi = "";
    private string _ilkProfilPath = "";

    private string _ilkBerberAdi = "";
    private string _ilkAdres = "";
    private string _ilkAcilis = "";
    private string _ilkKapanis = "";

    public BerberKisiselBilgilerSayfasi(bool showBottomBar = false)
    {
        InitializeComponent();
        _showBottomBar = showBottomBar;

        btnKaydet.Clicked += Kaydet_Clicked;
        btnGeriAl.Clicked += GeriAl_Clicked;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await SayfayiRoleGoreHazirlaAsync();
        await YukleAsync();
    }

    private async Task SayfayiRoleGoreHazirlaAsync()
    {
        // Kullanýcý tipini al (session boþsa db’den çek)
        var tip = (UserSession.KullaniciTipi ?? "").Trim();
        if (string.IsNullOrWhiteSpace(tip))
        {
            tip = await GetKullaniciTipiAsync(UserSession.KullaniciId);
            UserSession.KullaniciTipi = tip;
        }

        // Rol yazýsý
        if (tip.Equals("Calisan", StringComparison.OrdinalIgnoreCase))
            lblRol.Text = "Rol: Çalýþan";
        else if (tip.Equals("Berber", StringComparison.OrdinalIgnoreCase) || tip.Equals("Sahip", StringComparison.OrdinalIgnoreCase))
            lblRol.Text = "Rol: Sahip";
        else
            lblRol.Text = "Rol: Müþteri";

        // Dükkan kartý sadece berber/sahip için görünsün
        bool isBerber = tip.Equals("Berber", StringComparison.OrdinalIgnoreCase) ||
                        tip.Equals("Sahip", StringComparison.OrdinalIgnoreCase);

        // XAML’de shopCard yoksa bu satýr compile hatasý verir.
        // O yüzden XAML'e x:Name="shopCard" ekle.
        shopCard.IsVisible = isBerber;

        // Alt bar opsiyonel (istersen hiç göstermeyiz)
        if (!_showBottomBar)
        {
            // XAML’de BottomBarHost yoksa bu kýsmý komple silebilirsin.
            BottomBarHost.IsVisible = false;
            return;
        }

        // Müþteride alt bar istemiyorsan:
        bool isMusteri = tip.Equals("Musteri", StringComparison.OrdinalIgnoreCase);
        BottomBarHost.IsVisible = !isMusteri;

        if (!isMusteri)
        {
            barCalisan.IsVisible = tip.Equals("Calisan", StringComparison.OrdinalIgnoreCase);
            barBerber.IsVisible = !barCalisan.IsVisible;
        }
    }

    private async Task YukleAsync()
    {
        try
        {
            await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
            await conn.OpenAsync();

            // 1) Kullanýcý bilgileri (herkes)
            string sqlUser = @"
                SELECT ""Ad"", ""Soyad"", ""Telefon"",
                       COALESCE(""Eposta"", ''), 
                       COALESCE(""KullaniciAdi"", ''),
                       COALESCE(""ProfilFoto"", '')
                FROM kullanici
                WHERE ""ID"" = @id
                LIMIT 1;";

            await using (var cmd = new NpgsqlCommand(sqlUser, conn))
            {
                cmd.Parameters.AddWithValue("@id", UserSession.KullaniciId);

                await using var dr = await cmd.ExecuteReaderAsync();
                if (await dr.ReadAsync())
                {
                    var ad = dr.IsDBNull(0) ? "" : dr.GetString(0);
                    var soyad = dr.IsDBNull(1) ? "" : dr.GetString(1);
                    var tel = dr.IsDBNull(2) ? "" : dr.GetString(2);
                    var eposta = dr.IsDBNull(3) ? "" : dr.GetString(3);
                    var kullaniciAdi = dr.IsDBNull(4) ? "" : dr.GetString(4);
                    var profilPath = dr.IsDBNull(5) ? "" : dr.GetString(5);

                    lblAdSoyad.Text = $"{ad} {soyad}".Trim();
                    lblTelefonUst.Text = tel;

                    txtAd.Text = ad;
                    txtSoyad.Text = soyad;
                    txtTelefon.Text = tel;
                    txtEposta.Text = eposta;
                    txtKullaniciAdi.Text = kullaniciAdi;

                    _ilkProfilPath = profilPath;
                    SetImageFromPath(imgProfil, profilPath, "default_user.png");

                    _ilkAd = ad;
                    _ilkSoyad = soyad;
                    _ilkTelefon = tel;
                    _ilkEposta = eposta;
                    _ilkKullaniciAdi = kullaniciAdi;
                }
            }

            // 2) Dükkan bilgileri (sadece berber görünür; ama yine de güvenli çekelim)
            if (shopCard.IsVisible)
            {
                // berberId bul
                _berberId = await GetBerberIdByKullaniciIdAsync(UserSession.KullaniciId);

                string sqlShop = @"
                    SELECT COALESCE(""BerberAdi"", ''),
                           COALESCE(""Adres"", ''),
                           COALESCE(""AcilisSaati"", '00:00'::time),
                           COALESCE(""KapanisSaati"", '00:00'::time)
                    FROM ""Berberler""
                    WHERE ""BerberID"" = @bid
                    LIMIT 1;";

                await using var cmd2 = new NpgsqlCommand(sqlShop, conn);
                cmd2.Parameters.AddWithValue("@bid", _berberId);

                await using var dr2 = await cmd2.ExecuteReaderAsync();
                if (await dr2.ReadAsync())
                {
                    var berberAdi = dr2.IsDBNull(0) ? "" : dr2.GetString(0);
                    var adres = dr2.IsDBNull(1) ? "" : dr2.GetString(1);
                    var acilis = dr2.IsDBNull(2) ? "" : dr2.GetTimeSpan(2).ToString(@"hh\:mm");
                    var kapanis = dr2.IsDBNull(3) ? "" : dr2.GetTimeSpan(3).ToString(@"hh\:mm");

                    txtBerberAdi.Text = berberAdi;
                    txtAdres.Text = adres;
                    txtAcilis.Text = acilis;
                    txtKapanis.Text = kapanis;

                    _ilkBerberAdi = berberAdi;
                    _ilkAdres = adres;
                    _ilkAcilis = acilis;
                    _ilkKapanis = kapanis;
                }
            }
        }
        catch (PostgresException pgex)
        {
            await DisplayAlert("DB Hata", pgex.MessageText, "Tamam");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", ex.Message, "Tamam");
        }
    }

    private static void SetImageFromPath(Image img, string? path, string defaultImage)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                img.Source = ImageSource.FromFile(path);
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

            // Dükkan alanlarý sadece berberde güncellenecek
            var berberAdi = (txtBerberAdi.Text ?? "").Trim();
            var adres = (txtAdres.Text ?? "").Trim();
            var acilisText = (txtAcilis.Text ?? "").Trim();
            var kapanisText = (txtKapanis.Text ?? "").Trim();

            TimeSpan acilis = default, kapanis = default;
            if (shopCard.IsVisible)
            {
                if (!TryParseTime(acilisText, out acilis))
                {
                    await DisplayAlert("Uyarý", "Açýlýþ saati formatý hatalý. Örn: 09:00", "Tamam");
                    return;
                }

                if (!TryParseTime(kapanisText, out kapanis))
                {
                    await DisplayAlert("Uyarý", "Kapanýþ saati formatý hatalý. Örn: 21:00", "Tamam");
                    return;
                }
            }

            await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
            await conn.OpenAsync();

            // 1) Kullanýcý update (herkes)
            string sqlUserUpd = @"
                UPDATE kullanici
                SET ""Ad""=@ad, ""Soyad""=@soyad, ""Telefon""=@tel, ""Eposta""=@eposta
                WHERE ""ID""=@id;";

            await using (var cmd = new NpgsqlCommand(sqlUserUpd, conn))
            {
                cmd.Parameters.AddWithValue("@ad", ad);
                cmd.Parameters.AddWithValue("@soyad", soyad);
                cmd.Parameters.AddWithValue("@tel", tel);
                cmd.Parameters.AddWithValue("@eposta", eposta);
                cmd.Parameters.AddWithValue("@id", UserSession.KullaniciId);
                await cmd.ExecuteNonQueryAsync();
            }

            // 2) Berber update (sadece berber)
            if (shopCard.IsVisible)
            {
                if (_berberId <= 0)
                    _berberId = await GetBerberIdByKullaniciIdAsync(UserSession.KullaniciId);

                string sqlShopUpd = @"
                    UPDATE ""Berberler""
                    SET ""BerberAdi""=@badi,
                        ""Adres""=@adres,
                        ""AcilisSaati""=@acilis,
                        ""KapanisSaati""=@kapanis
                    WHERE ""BerberID""=@bid;";

                await using var cmd2 = new NpgsqlCommand(sqlShopUpd, conn);
                cmd2.Parameters.AddWithValue("@badi", berberAdi);
                cmd2.Parameters.AddWithValue("@adres", adres);
                cmd2.Parameters.AddWithValue("@acilis", acilis);
                cmd2.Parameters.AddWithValue("@kapanis", kapanis);
                cmd2.Parameters.AddWithValue("@bid", _berberId);
                await cmd2.ExecuteNonQueryAsync();

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
        catch (PostgresException pgex)
        {
            await DisplayAlert("DB Hata", pgex.MessageText, "Tamam");
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

        SetImageFromPath(imgProfil, _ilkProfilPath, "default_user.png");

        if (shopCard.IsVisible)
        {
            txtBerberAdi.Text = _ilkBerberAdi;
            txtAdres.Text = _ilkAdres;
            txtAcilis.Text = _ilkAcilis;
            txtKapanis.Text = _ilkKapanis;
        }

        await DisplayAlert("Bilgi", "Deðiþiklikler geri alýndý.", "Tamam");
    }

    private static bool TryParseTime(string text, out TimeSpan time)
        => TimeSpan.TryParse(text, out time);

    private async Task<string> GetKullaniciTipiAsync(int kullaniciId)
    {
        await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
        await conn.OpenAsync();

        string sql = @"SELECT COALESCE(""KullaniciTipi"", '')
                       FROM kullanici
                       WHERE ""ID""=@id
                       LIMIT 1;";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", kullaniciId);

        return (await cmd.ExecuteScalarAsync())?.ToString() ?? "";
    }

    private async Task<int> GetBerberIdByKullaniciIdAsync(int kullaniciId)
    {
        await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
        await conn.OpenAsync();

        string sql = @"
            SELECT ""BerberID""
            FROM ""Berberler""
            WHERE ""KullaniciID"" = @kid
            LIMIT 1;";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@kid", kullaniciId);

        var obj = await cmd.ExecuteScalarAsync();
        if (obj == null) throw new Exception("Bu kullanýcýya ait berber kaydý bulunamadý.");

        return Convert.ToInt32(obj);
    }
}
