using berber_randevu_uygulamasi.Services;
using Microsoft.Maui.Controls;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace berber_randevu_uygulamasi.Views.AltBarlar;

public partial class BerberCalismaSaatleriSayfasi : ContentPage
{
    private int _calisanId;

    public BerberCalismaSaatleriSayfasi()
    {
        InitializeComponent();

        // Switch deðiþince inputlarý aç/kapa
        swPzt.Toggled += (s, e) => SetDayEnabled(1, e.Value);
        swSal.Toggled += (s, e) => SetDayEnabled(2, e.Value);
        swCar.Toggled += (s, e) => SetDayEnabled(3, e.Value);
        swPer.Toggled += (s, e) => SetDayEnabled(4, e.Value);
        swCum.Toggled += (s, e) => SetDayEnabled(5, e.Value);
        swPaz.Toggled += (s, e) => SetDayEnabled(7, e.Value);

        btnKaydet.Clicked += BtnKaydet_Clicked;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            _calisanId = await EnsureCalisanIdForOwnerAsync(UserSession.KullaniciId);

            // kayýtlar yoksa oluþtur (1-7)
            await EnsureWeekRowsAsync(_calisanId);

            // DB’den çek ve ekrana bas
            await YukleSaatlerAsync(_calisanId);

            // Switch durumlarýna göre input enable/disable uygula
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

    // Sahip de çalýþan: calisanlar tablosunda kayýt yoksa oluþturur, CalisanID döner
    private async Task<int> EnsureCalisanIdForOwnerAsync(int kullaniciId)
    {
        await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
        await conn.OpenAsync();

        // 1) Önce varsa CalisanID çek
        // Not: calisanlar tablonuzda kolon adlarý farklýysa burayý düzeltiriz.
        string sqlFind = @"
            SELECT ""CalisanID""
            FROM calisanlar
            WHERE ""KullaniciID"" = @kid
            LIMIT 1;";

        await using (var cmd = new NpgsqlCommand(sqlFind, conn))
        {
            cmd.Parameters.AddWithValue("@kid", kullaniciId);

            var obj = await cmd.ExecuteScalarAsync();
            if (obj != null && obj != DBNull.Value)
                return Convert.ToInt32(obj);
        }

        // 2) Yoksa sahibin BerberID'sini bul ve calisanlar'a ekle
        int berberId = await GetBerberIdByKullaniciIdAsync(kullaniciId);

        string sqlInsert = @"
            INSERT INTO calisanlar (""BerberID"", ""KullaniciID"")
            VALUES (@bid, @kid)
            RETURNING ""CalisanID"";";

        await using (var cmd2 = new NpgsqlCommand(sqlInsert, conn))
        {
            cmd2.Parameters.AddWithValue("@bid", berberId);
            cmd2.Parameters.AddWithValue("@kid", kullaniciId);

            var newId = await cmd2.ExecuteScalarAsync();
            return Convert.ToInt32(newId);
        }
    }

    private async Task EnsureWeekRowsAsync(int calisanId)
    {
        await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
        await conn.OpenAsync();

        // 1-7 gün satýrlarý yoksa ekle
        // Varsayýlan: 09:00-21:00, Pazar kapalý
        string sql = @"
            INSERT INTO calisan_calisma_saatleri (""CalisanID"", ""Gun"", ""AcikMi"", ""Acilis"", ""Kapanis"")
            SELECT @cid, d.gun,
                   CASE WHEN d.gun = 7 THEN FALSE ELSE TRUE END,
                   CASE WHEN d.gun = 7 THEN NULL ELSE '09:00'::time END,
                   CASE WHEN d.gun = 7 THEN NULL ELSE '21:00'::time END
            FROM (VALUES (1),(2),(3),(4),(5),(6),(7)) AS d(gun)
            ON CONFLICT (""CalisanID"",""Gun"") DO NOTHING;";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@cid", calisanId);
        await cmd.ExecuteNonQueryAsync();
    }

    private async Task YukleSaatlerAsync(int calisanId)
    {
        await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
        await conn.OpenAsync();

        string sql = @"
            SELECT ""Gun"", ""AcikMi"", ""Acilis"", ""Kapanis""
            FROM calisan_calisma_saatleri
            WHERE ""CalisanID"" = @cid
            ORDER BY ""Gun"";";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@cid", calisanId);

        await using var dr = await cmd.ExecuteReaderAsync();
        while (await dr.ReadAsync())
        {
            short gun = dr.GetInt16(0);
            bool acikMi = dr.GetBoolean(1);
            TimeSpan? acilis = dr.IsDBNull(2) ? (TimeSpan?)null : dr.GetTimeSpan(2);
            TimeSpan? kapanis = dr.IsDBNull(3) ? (TimeSpan?)null : dr.GetTimeSpan(3);

            ApplyToUi(gun, acikMi, acilis, kapanis);
        }
    }

    private void ApplyToUi(short gun, bool acikMi, TimeSpan? acilis, TimeSpan? kapanis)
    {
        string ac = acilis.HasValue ? acilis.Value.ToString(@"hh\:mm") : "";
        string ka = kapanis.HasValue ? kapanis.Value.ToString(@"hh\:mm") : "";

        switch (gun)
        {
            case 1:
                swPzt.IsToggled = acikMi;
                txtPztAcilis.Text = ac;
                txtPztKapanis.Text = ka;
                break;

            case 2:
                swSal.IsToggled = acikMi;
                txtSalAcilis.Text = ac;
                txtSalKapanis.Text = ka;
                break;

            case 3:
                swCar.IsToggled = acikMi;
                txtCarAcilis.Text = ac;
                txtCarKapanis.Text = ka;
                break;

            case 4:
                swPer.IsToggled = acikMi;
                txtPerAcilis.Text = ac;
                txtPerKapanis.Text = ka;
                break;

            case 5:
                swCum.IsToggled = acikMi;
                txtCumAcilis.Text = ac;
                txtCumKapanis.Text = ka;
                break;

            // 6 Cumartesi: XAML’de yoksa þimdilik pas
            case 7:
                swPaz.IsToggled = acikMi;
                txtPazAcilis.Text = ac;
                txtPazKapanis.Text = ka;
                break;
        }
    }

    private void SetDayEnabled(short gun, bool enabled)
    {
        // kapalýysa giriþler disable, biraz soluk gözüksün
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
            var list = new List<(short gun, bool acik, TimeSpan? acilis, TimeSpan? kapanis)>();

            list.Add(ReadFromUi(1, swPzt.IsToggled, txtPztAcilis.Text, txtPztKapanis.Text));
            list.Add(ReadFromUi(2, swSal.IsToggled, txtSalAcilis.Text, txtSalKapanis.Text));
            list.Add(ReadFromUi(3, swCar.IsToggled, txtCarAcilis.Text, txtCarKapanis.Text));
            list.Add(ReadFromUi(4, swPer.IsToggled, txtPerAcilis.Text, txtPerKapanis.Text));
            list.Add(ReadFromUi(5, swCum.IsToggled, txtCumAcilis.Text, txtCumKapanis.Text));
            list.Add(ReadFromUi(7, swPaz.IsToggled, txtPazAcilis.Text, txtPazKapanis.Text));

            await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
            await conn.OpenAsync();

            string sqlUpsert = @"
                INSERT INTO calisan_calisma_saatleri
                    (""CalisanID"", ""Gun"", ""AcikMi"", ""Acilis"", ""Kapanis"")
                VALUES
                    (@cid, @gun, @acik, @acilis, @kapanis)
                ON CONFLICT (""CalisanID"", ""Gun"")
                DO UPDATE SET
                    ""AcikMi"" = EXCLUDED.""AcikMi"",
                    ""Acilis"" = EXCLUDED.""Acilis"",
                    ""Kapanis"" = EXCLUDED.""Kapanis"";";

            foreach (var item in list)
            {
                await using var cmd = new NpgsqlCommand(sqlUpsert, conn);
                cmd.Parameters.AddWithValue("@cid", _calisanId);
                cmd.Parameters.AddWithValue("@gun", item.gun);
                cmd.Parameters.AddWithValue("@acik", item.acik);
                cmd.Parameters.AddWithValue("@acilis", (object?)item.acilis ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@kapanis", (object?)item.kapanis ?? DBNull.Value);

                await cmd.ExecuteNonQueryAsync();
            }

            await DisplayAlert("Baþarýlý", "Çalýþma saatleri kaydedildi.", "Tamam");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", ex.Message, "Tamam");
        }
    }

    private (short gun, bool acik, TimeSpan? acilis, TimeSpan? kapanis) ReadFromUi(short gun, bool acikMi, string? acilisText, string? kapanisText)
    {
        if (!acikMi)
            return (gun, false, null, null);

        if (!TryParseHHmm(acilisText, out var acilis))
            throw new Exception($"{GunAdi(gun)} açýlýþ saati hatalý (örn: 09:00).");

        if (!TryParseHHmm(kapanisText, out var kapanis))
            throw new Exception($"{GunAdi(gun)} kapanýþ saati hatalý (örn: 21:00).");

        if (acilis >= kapanis)
            throw new Exception($"{GunAdi(gun)} için açýlýþ kapanýþtan küçük olmalý.");

        return (gun, true, acilis, kapanis);
    }

    private static bool TryParseHHmm(string? text, out TimeSpan time)
    {
        time = default;
        if (string.IsNullOrWhiteSpace(text)) return false;

        // 09:00 formatý
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
