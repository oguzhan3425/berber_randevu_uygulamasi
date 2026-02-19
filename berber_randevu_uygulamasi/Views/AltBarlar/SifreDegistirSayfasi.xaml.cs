using System;
using System.Threading.Tasks;
using Npgsql;
using berber_randevu_uygulamasi.Services;

namespace berber_randevu_uygulamasi.Views.AltBarlar
{
    public partial class SifreDegistirSayfasi : ContentPage
    {
        // Varsayýlan: alt bar kapalý.
        // Ýstersen new SifreDegistirSayfasi(showBottomBar:true) diyerek açarsýn.
        private readonly bool _showBottomBar;

        public SifreDegistirSayfasi(bool showBottomBar = false)
        {
            InitializeComponent();
            _showBottomBar = showBottomBar;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_showBottomBar)
                await AltBarAyarlaAsync();
            else
                BottomBarHost.IsVisible = false;
        }

        private async Task AltBarAyarlaAsync()
        {
            BottomBarHost.IsVisible = true;

            // 1) Tip session’da varsa onu kullan
            var tip = (UserSession.KullaniciTipi ?? "").Trim();

            // 2) Yoksa DB’den çek
            if (string.IsNullOrWhiteSpace(tip))
            {
                await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
                await conn.OpenAsync();

                string sql = @"SELECT COALESCE(""KullaniciTipi"", '')
                               FROM kullanici
                               WHERE ""ID""=@id
                               LIMIT 1;";

                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", UserSession.KullaniciId);

                tip = (await cmd.ExecuteScalarAsync())?.ToString() ?? "";
                UserSession.KullaniciTipi = tip;
            }

            // 3) Çalýþan -> CalisanAltBar, Berber -> BerberAltBar
            bool isCalisan = tip.Equals("Calisan", StringComparison.OrdinalIgnoreCase);

            // müþteri sayfalarýnda alt bar istemiyorsun; ama showBottomBar=true verilirse:
            // müþteri için ikisini de kapatabiliriz.
            bool isMusteri = tip.Equals("Musteri", StringComparison.OrdinalIgnoreCase);

            barCalisan.IsVisible = isCalisan && !isMusteri;
            barBerber.IsVisible = !isCalisan && !isMusteri;
        }

        private async void GeriDon_Clicked(object sender, EventArgs e)
        {
            // Sayfa stack’te varsa geri dön
            if (Navigation.NavigationStack.Count > 1)
            {
                await Navigation.PopAsync();
                return;
            }

            // Modal açýldýysa
            if (Navigation.ModalStack.Count > 0)
            {
                await Navigation.PopModalAsync();
                return;
            }

            // Hiç stack yoksa: bir þey yapma (istersen main sayfaya atarýz)
            await DisplayAlert("Bilgi", "Geri dönülecek sayfa yok.", "Tamam");
        }

        private async void SifreGuncelle_Clicked(object sender, EventArgs e)
        {
            string guncel = GuncelSifreEntry.Text ?? "";
            string yeni = YeniSifreEntry.Text ?? "";
            string yeniTekrar = YeniSifreTekrarEntry.Text ?? "";

            if (string.IsNullOrWhiteSpace(guncel) ||
                string.IsNullOrWhiteSpace(yeni) ||
                string.IsNullOrWhiteSpace(yeniTekrar))
            {
                await DisplayAlert("Uyarý", "Lütfen tüm alanlarý doldurun.", "Tamam");
                return;
            }

            if (yeni != yeniTekrar)
            {
                await DisplayAlert("Uyarý", "Yeni þifreler eþleþmiyor.", "Tamam");
                return;
            }

            if (yeni.Length < 6)
            {
                await DisplayAlert("Uyarý", "Yeni þifre en az 6 karakter olmalý.", "Tamam");
                return;
            }

            try
            {
                await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
                await conn.OpenAsync();

                // 1) DB'den mevcut þifreyi al
                string selectSql = @"SELECT ""Sifre""
                                     FROM kullanici
                                     WHERE ""ID"" = @id
                                     LIMIT 1;";
                await using var selectCmd = new NpgsqlCommand(selectSql, conn);
                selectCmd.Parameters.AddWithValue("@id", UserSession.KullaniciId);

                var dbSifreObj = await selectCmd.ExecuteScalarAsync();
                if (dbSifreObj == null)
                {
                    await DisplayAlert("Hata", "Kullanýcý bulunamadý.", "Tamam");
                    return;
                }

                string dbSifre = dbSifreObj.ToString() ?? "";
                if (dbSifre != guncel)
                {
                    await DisplayAlert("Hata", "Güncel þifre yanlýþ.", "Tamam");
                    return;
                }

                // 2) Update
                string updateSql = @"UPDATE kullanici
                                     SET ""Sifre"" = @yeni
                                     WHERE ""ID"" = @id;";
                await using var updateCmd = new NpgsqlCommand(updateSql, conn);
                updateCmd.Parameters.AddWithValue("@yeni", yeni);
                updateCmd.Parameters.AddWithValue("@id", UserSession.KullaniciId);

                int affected = await updateCmd.ExecuteNonQueryAsync();
                if (affected > 0)
                {
                    await DisplayAlert("Baþarýlý", "Þifreniz güncellendi.", "Tamam");
                    await GeriDonGuvenliAsync();
                }
                else
                {
                    await DisplayAlert("Hata", "Þifre güncellenemedi.", "Tamam");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", ex.Message, "Tamam");
            }
        }

        private async Task GeriDonGuvenliAsync()
        {
            if (Navigation.NavigationStack.Count > 1)
                await Navigation.PopAsync();
            else if (Navigation.ModalStack.Count > 0)
                await Navigation.PopModalAsync();
        }
    }
}
