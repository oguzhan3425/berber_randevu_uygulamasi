using Npgsql;
using berber_randevu_uygulamasi.Services;
namespace berber_randevu_uygulamasi.Views.AltBarlar;

public partial class HizmetEkleModalSayfasi : ContentPage
{
    private readonly int _berberId;
    private readonly int _calisanId;
    private readonly Func<Task>? _onSaved;
    public HizmetEkleModalSayfasi(int berberId, int calisanId, Func<Task>? onSaved = null)
	{
		InitializeComponent();
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
            await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
            await conn.OpenAsync();

            string sql = @"
                INSERT INTO hizmetler
                (""HizmetAdi"", ""Fiyat"", ""SureDakika"", ""BerberID"", ""CalisanID"", ""Aktif"")
                VALUES
                (@ad, @fiyat, @sure, @bid, @cid, true);";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ad", ad);
            cmd.Parameters.AddWithValue("@fiyat", fiyat);
            cmd.Parameters.AddWithValue("@sure", sure);
            cmd.Parameters.AddWithValue("@bid", _berberId);
            cmd.Parameters.AddWithValue("@cid", _calisanId);

            await cmd.ExecuteNonQueryAsync();

            await DisplayAlert("Baþarýlý", "Hizmet eklendi.", "Tamam");

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