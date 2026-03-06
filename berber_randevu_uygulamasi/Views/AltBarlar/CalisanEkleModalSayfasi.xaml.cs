using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Npgsql;
using berber_randevu_uygulamasi.Services;

namespace berber_randevu_uygulamasi.Views
{
    public partial class CalisanEkleModalSayfasi : ContentPage
    {
        protected readonly ApiClient _api;
        private readonly int _berberId;
        private readonly Func<Task>? _onAddedRefresh;

        private int _bulunanKullaniciId = 0;

        public CalisanEkleModalSayfasi(ApiClient api,int berberId, Func<Task>? onAddedRefresh = null)
        {
            InitializeComponent();
            _berberId = berberId;
            _onAddedRefresh = onAddedRefresh;
            _api = api;
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
                await using var tx = await conn.BeginTransactionAsync();

                // 1) Bu kullanýcý zaten bir berbere baðlý mý kontrol et
                string sqlCheck = @"
                    SELECT ""BerberID""
                    FROM calisanlar
                    WHERE ""KullaniciID"" = @kid
                    LIMIT 1;";

                await using (var cmdCheck = new NpgsqlCommand(sqlCheck, conn, tx))
                {
                    cmdCheck.Parameters.AddWithValue("@kid", _bulunanKullaniciId);
                    var existing = await cmdCheck.ExecuteScalarAsync();

                    if (existing != null && existing != DBNull.Value)
                    {
                        int existingBerberId = Convert.ToInt32(existing);

                        if (existingBerberId == _berberId)
                        {
                            await tx.RollbackAsync();
                            await DisplayAlert("Bilgi", "Bu kullanýcý zaten senin çalýþanýnda kayýtlý.", "Tamam");
                            return;
                        }

                        await tx.RollbackAsync();
                        await DisplayAlert("Uyarý", "Bu kullanýcý zaten baþka bir berberde çalýþýyor.", "Tamam");
                        return;
                    }
                }

                // 2) calisanlar'a ekle
                string sqlInsert = @"
                    INSERT INTO calisanlar (""KullaniciID"", ""BerberID"")
                    VALUES (@kid, @bid);";

                await using (var cmdIns = new NpgsqlCommand(sqlInsert, conn, tx))
                {
                    cmdIns.Parameters.AddWithValue("@kid", _bulunanKullaniciId);
                    cmdIns.Parameters.AddWithValue("@bid", _berberId);
                    await cmdIns.ExecuteNonQueryAsync();
                }

                // 3) kullanici tipini çalýþan yap
                // (Bu kolonu senin DB'de gördük: "KullaniciTipi")
                string sqlUpdateTip = @"
                    UPDATE kullanici
                    SET ""KullaniciTipi"" = @tip
                    WHERE ""ID"" = @kid;";

                await using (var cmdUpd = new NpgsqlCommand(sqlUpdateTip, conn, tx))
                {
                    cmdUpd.Parameters.AddWithValue("@tip", "Calisan");
                    cmdUpd.Parameters.AddWithValue("@kid", _bulunanKullaniciId);
                    await cmdUpd.ExecuteNonQueryAsync();
                }

                await tx.CommitAsync();

                await DisplayAlert("Baþarýlý", "Çalýþan eklendi.", "Tamam");

                if (_onAddedRefresh != null)
                    await _onAddedRefresh();

                await Navigation.PopModalAsync();
            }
            catch (PostgresException pex) when (pex.SqlState == "23505")
            {
                // UNIQUE hatasý (ayný kullanýcý tekrar eklenirse)
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
