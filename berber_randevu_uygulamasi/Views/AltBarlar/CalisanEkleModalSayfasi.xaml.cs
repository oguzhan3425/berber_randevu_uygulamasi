using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Npgsql;
using berber_randevu_uygulamasi.Services;

namespace berber_randevu_uygulamasi.Views
{
    public partial class CalisanEkleModalSayfasi : ContentPage
    {
        private readonly int _berberId;
        private readonly Func<Task>? _onAddedRefresh;

        private int _bulunanKullaniciId = 0;

        public CalisanEkleModalSayfasi(int berberId, Func<Task>? onAddedRefresh = null)
        {
            InitializeComponent();
            _berberId = berberId;
            _onAddedRefresh = onAddedRefresh;
        }

        private async void Ara_Clicked(object sender, EventArgs e)
        {
            lblBilgi.IsVisible = false;
            sonucKart.IsVisible = false;
            _bulunanKullaniciId = 0;

            if (!int.TryParse(txtKullaniciId.Text?.Trim(), out int kid))
            {
                await DisplayAlert("Uyarý", "Geçerli bir kullanýcý ID gir.", "Tamam");
                return;
            }

            try
            {
                await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
                await conn.OpenAsync();

                string sql = @"
                    SELECT ""ID"", ""Ad"", ""Soyad"", ""Telefon"", ""KullaniciTipi""
                    FROM kullanici
                    WHERE ""ID"" = @id
                    LIMIT 1;";

                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", kid);

                await using var dr = await cmd.ExecuteReaderAsync();

                if (await dr.ReadAsync())
                {
                    _bulunanKullaniciId = dr.GetInt32(0);
                    string ad = dr.IsDBNull(1) ? "" : dr.GetString(1);
                    string soyad = dr.IsDBNull(2) ? "" : dr.GetString(2);
                    string tel = dr.IsDBNull(3) ? "" : dr.GetString(3);
                    string tip = dr.IsDBNull(4) ? "" : dr.GetString(4);

                    lblAdSoyad.Text = $"{ad} {soyad}".Trim();
                    lblTelefon.Text = string.IsNullOrWhiteSpace(tel) ? "Telefon: —" : $"Telefon: {tel}";
                    lblTip.Text = string.IsNullOrWhiteSpace(tip) ? "Tip: —" : $"Tip: {tip}";

                    sonucKart.IsVisible = true;
                }
                else
                {
                    lblBilgi.Text = "Bu ID ile kullanýcý bulunamadý.";
                    lblBilgi.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", ex.Message, "Tamam");
            }
        }

        private async void Ekle_Clicked(object sender, EventArgs e)
        {
            if (_bulunanKullaniciId <= 0)
            {
                await DisplayAlert("Uyarý", "Önce kullanýcýyý ara.", "Tamam");
                return;
            }

            try
            {
                await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
                await conn.OpenAsync();

                // 1) Bu kullanýcý zaten bir berbere baðlý mý kontrol et
                string sqlCheck = @"
                    SELECT ""BerberID""
                    FROM calisanlar
                    WHERE ""KullaniciID"" = @kid
                    LIMIT 1;";

                await using (var cmdCheck = new NpgsqlCommand(sqlCheck, conn))
                {
                    cmdCheck.Parameters.AddWithValue("@kid", _bulunanKullaniciId);
                    var existing = await cmdCheck.ExecuteScalarAsync();

                    if (existing != null)
                    {
                        int existingBerberId = Convert.ToInt32(existing);
                        if (existingBerberId == _berberId)
                        {
                            await DisplayAlert("Bilgi", "Bu kullanýcý zaten senin çalýþanýnda kayýtlý.", "Tamam");
                            return;
                        }

                        await DisplayAlert("Uyarý", "Bu kullanýcý zaten hizmet vermektedir.", "Tamam");
                        return;
                    }
                }

                // 2) Ekle
                string sqlInsert = @"
                    INSERT INTO calisanlar (""KullaniciID"", ""BerberID"")
                    VALUES (@kid, @bid);";

                await using (var cmdIns = new NpgsqlCommand(sqlInsert, conn))
                {
                    cmdIns.Parameters.AddWithValue("@kid", _bulunanKullaniciId);
                    cmdIns.Parameters.AddWithValue("@bid", _berberId);
                    await cmdIns.ExecuteNonQueryAsync();
                }

                await DisplayAlert("Baþarýlý", "Çalýþan eklendi.", "Tamam");

                if (_onAddedRefresh != null)
                    await _onAddedRefresh();

                await Navigation.PopModalAsync();
            }
            catch (PostgresException pex) when (pex.SqlState == "23505")
            {
                // uq_calisanlar_kullanici UNIQUE hatasý
                await DisplayAlert("Uyarý", "Bu kullanýcý zaten hizmet vermektedir.", "Tamam");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", ex.Message, "Tamam");
            }
        }

        private async void Kapat_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}