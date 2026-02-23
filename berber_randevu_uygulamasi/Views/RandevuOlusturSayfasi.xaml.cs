using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Npgsql;
using berber_randevu_uygulamasi.Services;
using Microsoft.Maui.Controls;

namespace berber_randevu_uygulamasi.Views
{
    public partial class RandevuOlusturSayfasi : ContentPage
    {
        public RandevuOlusturVM VM { get; }

        private readonly int _calisanId;
        private readonly int _berberId; // INSERT için dursun (raporlama/þube vs.)

        private DateOnly _seciliTarih = DateOnly.FromDateTime(DateTime.Now);

        private static readonly TimeSpan _gunBas = new(9, 0, 0);
        private static readonly TimeSpan _gunBit = new(18, 0, 0);

        private const int _dakikaAdim = 5;

        // DB’den gelen dolu aralýklar (randevunun kendi bloðu)
        private List<(TimeSpan bas, TimeSpan bit)> _doluAraliklar = new();

        private int? _oncekiSaatIndex = null;
        private int? _oncekiDakikaIndex = null;

        private bool _isRefreshing;

        public RandevuOlusturSayfasi(Models.CalisanKart calisan)
        {
            InitializeComponent();
            // ? Tarih picker: bugün - 7 gün
            dateTarih.MinimumDate = DateTime.Today;
            dateTarih.MaximumDate = DateTime.Today.AddDays(7);
            dateTarih.Date = DateTime.Today;

            // sayfadaki DateOnly tarih de uyumlu olsun
            _seciliTarih = DateOnly.FromDateTime(dateTarih.Date);

            _calisanId = calisan.CalisanID;
            _berberId = calisan.BerberID;

            VM = new RandevuOlusturVM();
            BindingContext = VM;

            VM.CalisanAdSoyad = $"{calisan.Ad} {calisan.Soyad}".Trim();
            VM.CalisanFoto = string.IsNullOrWhiteSpace(calisan.Foto) ? "default_berber.png" : calisan.Foto;

            VM.HizmetToggleCommand = new Command<HizmetItem>(async (h) =>
            {
                VM.HizmetToggle(h);
                await SaatDakikaListeleriniYenileAsync(keepSelection: true);
            });

            pickerSaat.SelectedIndexChanged += async (_, __) => await SaatSecimiDegistiAsync();
            pickerDakika.SelectedIndexChanged += async (_, __) => await DakikaSecimiDegistiAsync();
        }

        // ? Sayfaya her girildiðinde / geri dönüldüðünde DB’den yenile
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await RefreshFromDbAsync(keepSelection: true);
        }

        private async void dateTarih_DateSelected(object sender, DateChangedEventArgs e)
        {
            _seciliTarih = DateOnly.FromDateTime(e.NewDate);

            // ? Tarih deðiþince DB'den dolu aralýklarý çekip saat/dakikayý yenile
            await RefreshFromDbAsync(keepSelection: false);
        }
        private async Task RefreshFromDbAsync(bool keepSelection)
        {
            if (_isRefreshing) return;
            _isRefreshing = true;

            try
            {
                await YukleAsync();
                await SaatDakikaListeleriniYenileAsync(keepSelection: keepSelection);
            }
            finally
            {
                _isRefreshing = false;
            }
        }

        private async Task YukleAsync()
        {
            // 1) Hizmetleri çalýþan bazlý çek
            var hizmetler = await HizmetleriGetirAsync(_calisanId);
            VM.Hizmetler = new ObservableCollection<HizmetItem>(hizmetler);

            // 2) Dolu aralýklarý çalýþan bazlý çek
            _doluAraliklar = await DoluAraliklariGetirAsync(_calisanId, _seciliTarih);
        }

        private int SeciliToplamSureDakika()
        {
            int toplam = VM.ToplamSeciliSureDakika;
            return toplam > 0 ? toplam : 30;
        }

        // ? DB'deki randevunun kendi zaman bloðunda mý? (UI boyamasý için)
        private bool BaslangicAnindaDoluMu(TimeSpan t)
        {
            return _doluAraliklar.Any(d => t >= d.bas && t < d.bit);
        }

        // ? Randevu oluþtururken gerçek çakýþma kontrolü (seçilen hizmet süresine göre)
        private bool SlotCakisiyorMu(TimeSpan adayBas, TimeSpan adayBit)
        {
            // çalýþma saati dýþý
            if (adayBas < _gunBas) return true;
            if (adayBit > _gunBit) return true;

            // çakýþma
            return _doluAraliklar.Any(d => adayBas < d.bit && adayBit > d.bas);
        }

        // ? Bugünse geçmiþ saatler (DOLU deðil, GEÇTÝ)
        private bool SlotGectiMi(TimeSpan adayBas)
        {
            bool bugunMu = _seciliTarih == DateOnly.FromDateTime(DateTime.Now);
            if (!bugunMu) return false;

            var simdi = TimeOnly.FromDateTime(DateTime.Now).ToTimeSpan();
            return adayBas < simdi;
        }

        private async Task SaatDakikaListeleriniYenileAsync(bool keepSelection)
        {
            // Her yenilemede DB’den tekrar çek (iptal/ekleme olabilir)
            _doluAraliklar = await DoluAraliklariGetirAsync(_calisanId, _seciliTarih);

            // Saat opsiyonlarý
            var saatOps = new List<OptionItem>();

            // ? 18:00'ý gösterme (18:00 baþlangýçlý randevu olamaz)
            for (int h = _gunBas.Hours; h < _gunBit.Hours; h++)
            {
                bool saatTamDolu = true;   // UI: o saat içindeki tüm dk'lar randevu bloðunda mý?
                bool saatTamGecti = true;  // UI: o saat içindeki tüm dk'lar geçti mi?

                for (int dk = 0; dk < 60; dk += _dakikaAdim)
                {
                    var bas = new TimeSpan(h, dk, 0);

                    bool dolu = BaslangicAnindaDoluMu(bas); // ? sadece randevunun kendi bloðu
                    bool gecti = SlotGectiMi(bas);

                    if (!dolu) saatTamDolu = false;
                    if (!gecti) saatTamGecti = false;
                }

                string text = $"{h:00}";
                if (saatTamDolu) text += " (Dolu)";
                else if (saatTamGecti) text += " (Geçti)";

                bool secilemez = saatTamDolu || saatTamGecti;

                saatOps.Add(new OptionItem(h, text, isDisabled: secilemez));
            }

            var oncekiSaatValue =
                (keepSelection && pickerSaat.SelectedItem is OptionItem os) ? os.Value : (int?)null;

            pickerSaat.ItemsSource = saatOps;

            int secSaatIndex = -1;

            if (oncekiSaatValue.HasValue)
                secSaatIndex = saatOps.FindIndex(x => x.Value == oncekiSaatValue.Value && !x.IsDisabled);

            if (secSaatIndex < 0)
                secSaatIndex = saatOps.FindIndex(x => !x.IsDisabled);

            pickerSaat.SelectedIndex = secSaatIndex;

            await DakikalariDoldurAsync(keepSelection: keepSelection);
            OzetGuncelle();
        }

        private async Task SaatSecimiDegistiAsync()
        {
            if (pickerSaat.SelectedItem is not OptionItem saatItem)
                return;

            if (saatItem.IsDisabled)
            {
                await DisplayAlert("Uyarý", "Bu saat seçilemez (dolu/geçti). Lütfen baþka saat seçin.", "Tamam");

                if (_oncekiSaatIndex.HasValue)
                    pickerSaat.SelectedIndex = _oncekiSaatIndex.Value;
                else
                    pickerSaat.SelectedIndex = ((List<OptionItem>)pickerSaat.ItemsSource).FindIndex(x => !x.IsDisabled);

                return;
            }

            _oncekiSaatIndex = pickerSaat.SelectedIndex;

            await DakikalariDoldurAsync(keepSelection: false);
            OzetGuncelle();
        }

        private async Task DakikalariDoldurAsync(bool keepSelection)
        {
            if (pickerSaat.SelectedItem is not OptionItem saatItem || saatItem.IsDisabled)
            {
                pickerDakika.ItemsSource = new List<OptionItem>();
                pickerDakika.SelectedIndex = -1;
                return;
            }

            var dkOps = new List<OptionItem>();
            for (int dk = 0; dk < 60; dk += _dakikaAdim)
            {
                var bas = new TimeSpan(saatItem.Value, dk, 0);

                // ? UI: sadece randevunun kendi bloðundaysa "Dolu"
                bool dolu = BaslangicAnindaDoluMu(bas);

                // ? UI: geçmiþ ise "Geçti"
                bool gecti = SlotGectiMi(bas);

                string text = $"{dk:00}";
                if (dolu) text += " (Dolu)";
                else if (gecti) text += " (Geçti)";

                bool secilemez = dolu || gecti;

                dkOps.Add(new OptionItem(dk, text, isDisabled: secilemez));
            }

            var oncekiDakikaValue =
                (keepSelection && pickerDakika.SelectedItem is OptionItem od) ? od.Value : (int?)null;

            pickerDakika.ItemsSource = dkOps;

            int secIdx = -1;
            if (oncekiDakikaValue.HasValue)
                secIdx = dkOps.FindIndex(x => x.Value == oncekiDakikaValue.Value && !x.IsDisabled);

            if (secIdx < 0)
                secIdx = dkOps.FindIndex(x => !x.IsDisabled);

            pickerDakika.SelectedIndex = secIdx;
            _oncekiDakikaIndex = pickerDakika.SelectedIndex;

            await Task.CompletedTask;
        }

        private async Task DakikaSecimiDegistiAsync()
        {
            if (pickerDakika.SelectedItem is not OptionItem dkItem)
                return;

            if (dkItem.IsDisabled)
            {
                await DisplayAlert("Uyarý", "Bu dakika seçilemez (dolu/geçti). Lütfen baþka dakika seçin.", "Tamam");

                if (_oncekiDakikaIndex.HasValue)
                    pickerDakika.SelectedIndex = _oncekiDakikaIndex.Value;
                else
                    pickerDakika.SelectedIndex = ((List<OptionItem>)pickerDakika.ItemsSource).FindIndex(x => !x.IsDisabled);

                return;
            }

            _oncekiDakikaIndex = pickerDakika.SelectedIndex;
            OzetGuncelle();
        }

        private void OzetGuncelle()
        {
            if (pickerSaat.SelectedItem is not OptionItem s || pickerDakika.SelectedItem is not OptionItem d)
            {
                lblSecimOzeti.Text = "";
                return;
            }

            int toplamSure = SeciliToplamSureDakika();
            var bas = new TimeSpan(s.Value, d.Value, 0);
            var bit = bas.Add(TimeSpan.FromMinutes(toplamSure));

            lblSecimOzeti.Text =
     $"Tarih: {_seciliTarih:dd.MM.yyyy}  •  Baþlangýç: {bas:hh\\:mm}  •  Bitiþ: {bit:hh\\:mm}  •  Süre: {toplamSure} dk";
        }

        // ? HÝZMETLER: SADECE ÇALIÞANA GÖRE
        private async Task<List<HizmetItem>> HizmetleriGetirAsync(int calisanId)
        {
            var liste = new List<HizmetItem>();

            await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
            await conn.OpenAsync();

            string sql = @"
                SELECT ""HizmetID"", ""HizmetAdi"", ""Fiyat"", ""SureDakika""
                FROM hizmetler
                WHERE ""CalisanID"" = @cid
                  AND COALESCE(""Aktif"", TRUE) = TRUE
                ORDER BY ""HizmetAdi"";";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@cid", calisanId);

            await using var dr = await cmd.ExecuteReaderAsync();
            while (await dr.ReadAsync())
            {
                var hizmetId = dr.GetInt32(0);
                var ad = dr.IsDBNull(1) ? "" : dr.GetString(1);
                var fiyat = dr.IsDBNull(2) ? 0 : dr.GetDecimal(2);
                var sure = dr.IsDBNull(3) ? 30 : dr.GetInt32(3);

                if (sure <= 0) sure = 30;

                liste.Add(new HizmetItem(hizmetId, ad, fiyat, sure));
            }

            return liste;
        }

        // ? DOLU ARALIKLAR: SADECE ÇALIÞANA GÖRE
        private async Task<List<(TimeSpan bas, TimeSpan bit)>> DoluAraliklariGetirAsync(int calisanId, DateOnly tarih)
        {
            var liste = new List<(TimeSpan bas, TimeSpan bit)>();

            await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
            await conn.OpenAsync();

            string sql = @"
                SELECT ""RandevuSaati"", COALESCE(NULLIF(""SureDakika"", 0), 30)
                FROM randevular
                WHERE ""CalisanID"" = @cid
                  AND ""RandevuTarihi"" = @tarih;";

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@cid", calisanId);
            cmd.Parameters.AddWithValue("@tarih", tarih);

            await using var dr = await cmd.ExecuteReaderAsync();
            while (await dr.ReadAsync())
            {
                if (dr.IsDBNull(0)) continue;

                var bas = dr.GetTimeSpan(0);
                var sure = dr.IsDBNull(1) ? 30 : dr.GetInt32(1);
                if (sure <= 0) sure = 30;

                var bit = bas.Add(TimeSpan.FromMinutes(sure));
                liste.Add((bas, bit));
            }

            return liste;
        }

        private async void Hizmet_Tapped(object sender, TappedEventArgs e)
        {
            if (sender is not TapGestureRecognizer tgr) return;

            if (tgr.CommandParameter is HizmetItem item)
            {
                VM.HizmetToggle(item);
                await SaatDakikaListeleriniYenileAsync(keepSelection: true);
            }
        }
        private async void RandevuOlustur_Clicked(object sender, EventArgs e)
        {
            try
            {
                var seciliHizmetler = VM.Hizmetler.Where(x => x.Secili).ToList();
                if (seciliHizmetler.Count == 0)
                {
                    await DisplayAlert("Uyarý", "Hizmet seçiniz.", "Tamam");
                    return;
                }

                if (pickerSaat.SelectedItem is not OptionItem s || s.IsDisabled)
                {
                    await DisplayAlert("Uyarý", "Uygun bir saat seçiniz.", "Tamam");
                    return;
                }

                if (pickerDakika.SelectedItem is not OptionItem d || d.IsDisabled)
                {
                    await DisplayAlert("Uyarý", "Uygun bir dakika seçiniz.", "Tamam");
                    return;
                }

                int toplamSure = seciliHizmetler.Sum(x => x.SureDakika);
                if (toplamSure <= 0) toplamSure = 30;

                decimal toplamUcret = seciliHizmetler.Sum(x => x.Fiyat);
                int hizmetId = seciliHizmetler.First().HizmetID;

                var adayBas = new TimeSpan(s.Value, d.Value, 0);
                var adayBit = adayBas.Add(TimeSpan.FromMinutes(toplamSure));

                // ? Gerçek çakýþma kontrolü (seçilen hizmet süresine göre)
                if (SlotCakisiyorMu(adayBas, adayBit))
                {
                    await DisplayAlert("Uyarý", "Bu saat aralýðý seçilen hizmet süresiyle çakýþýyor.", "Tamam");
                    await RefreshFromDbAsync(keepSelection: true);
                    return;
                }

                if (SlotGectiMi(adayBas))
                {
                    await DisplayAlert("Uyarý", "Geçmiþ bir saat seçilemez.", "Tamam");
                    return;
                }

                bool onay = await DisplayAlert(
                    "Randevu Onayý",
                    $"Tarih: {_seciliTarih:dd.MM.yyyy}\n" +
                    $"Baþlangýç: {adayBas:hh\\:mm}\n" +
                    $"Bitiþ: {adayBit:hh\\:mm}\n" +
                    $"Toplam Süre: {toplamSure} dk\n" +
                    $"Toplam Ücret: {toplamUcret:0} ?\n\n" +
                    $"Randevuyu oluþturmak istiyor musunuz?",
                    "Tamam",
                    "Ýptal");

                if (!onay) return;

                bool ok = await RandevuKaydetAsync(
                    UserSession.KullaniciId,
                    _berberId,
                    _calisanId,
                    hizmetId,
                    _seciliTarih,
                    adayBas,
                    toplamSure,
                    toplamUcret);

                if (!ok)
                {
                    await DisplayAlert("Uyarý", "Bu saat aralýðý dolu.", "Tamam");
                    await RefreshFromDbAsync(keepSelection: true);
                    return;
                }

                await DisplayAlert("Baþarýlý", "Randevu oluþturuldu.", "Tamam");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", ex.Message, "Tamam");
            }
        }

        private async Task<bool> RandevuKaydetAsync(
            int kullaniciId,
            int berberId,
            int calisanId,
            int hizmetId,
            DateOnly tarih,
            TimeSpan saat,
            int sureDakika,
            decimal toplamUcret)
        {
            await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
            await conn.OpenAsync();

            await using var tx = await conn.BeginTransactionAsync();

            var adayBas = saat;
            var adayBit = saat.Add(TimeSpan.FromMinutes(sureDakika));

            string sqlKontrol = @"
                SELECT COUNT(*)
                FROM randevular
                WHERE ""CalisanID"" = @cid
                  AND ""RandevuTarihi"" = @tarih
                  AND (
                        (@adayBas < (""RandevuSaati"" + (COALESCE(NULLIF(""SureDakika"",0),30) * INTERVAL '1 minute')))
                    AND (@adayBit > ""RandevuSaati"")
                  );";

            await using (var cmd = new NpgsqlCommand(sqlKontrol, conn, tx))
            {
                cmd.Parameters.AddWithValue("@cid", calisanId);
                cmd.Parameters.AddWithValue("@tarih", tarih);
                cmd.Parameters.AddWithValue("@adayBas", adayBas);
                cmd.Parameters.AddWithValue("@adayBit", adayBit);

                int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                if (count > 0)
                {
                    await tx.RollbackAsync();
                    return false;
                }
            }

            string sqlInsert = @"
                INSERT INTO randevular
                (""KullaniciID"", ""BerberID"", ""CalisanID"", ""HizmetID"",
                 ""RandevuTarihi"", ""RandevuSaati"", ""SureDakika"", ""ToplamUcret"")
                VALUES
                (@kid, @bid, @cid, @hid,
                 @tarih, @saat, @sure, @ucret);";

            await using (var cmd = new NpgsqlCommand(sqlInsert, conn, tx))
            {
                cmd.Parameters.AddWithValue("@kid", kullaniciId);
                cmd.Parameters.AddWithValue("@bid", berberId);
                cmd.Parameters.AddWithValue("@cid", calisanId);
                cmd.Parameters.AddWithValue("@hid", hizmetId);
                cmd.Parameters.AddWithValue("@tarih", tarih);
                cmd.Parameters.AddWithValue("@saat", adayBas);
                cmd.Parameters.AddWithValue("@sure", sureDakika);
                cmd.Parameters.AddWithValue("@ucret", toplamUcret);

                await cmd.ExecuteNonQueryAsync();
            }

            await tx.CommitAsync();
            return true;
        }

        private async void Geri_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }

    // Picker item modeli
    public class OptionItem
    {
        public int Value { get; }
        public string Text { get; }
        public bool IsDisabled { get; }

        public OptionItem(int value, string text, bool isDisabled)
        {
            Value = value;
            Text = text;
            IsDisabled = isDisabled;
        }

        public override string ToString() => Text; // ? Picker Text için garanti
    }

    public class RandevuOlusturVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string CalisanAdSoyad { get => _calisanAdSoyad; set { _calisanAdSoyad = value; Raise(); } }
        private string _calisanAdSoyad = "";

        public string CalisanFoto { get => _calisanFoto; set { _calisanFoto = value; Raise(); } }
        private string _calisanFoto = "default_berber.png";

        public ObservableCollection<HizmetItem> Hizmetler
        {
            get => _hizmetler;
            set { _hizmetler = value; Raise(); Raise(nameof(SeciliHizmetSayisi)); }
        }
        private ObservableCollection<HizmetItem> _hizmetler = new();

        public ICommand? HizmetToggleCommand { get; set; }

        public int SeciliHizmetSayisi => Hizmetler.Count(h => h.Secili);
        public int ToplamSeciliSureDakika => Hizmetler.Where(h => h.Secili).Sum(h => h.SureDakika);

        public void HizmetToggle(HizmetItem? item)
        {
            if (item == null) return;
            item.Secili = !item.Secili;
            Raise(nameof(SeciliHizmetSayisi));
        }

        public void Raise([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class HizmetItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public int HizmetID { get; }
        public string Ad { get; }
        public decimal Fiyat { get; }
        public int SureDakika { get; }

        public bool Secili
        {
            get => _secili;
            set
            {
                _secili = value;
                OnChanged();
                OnChanged(nameof(Check));
                OnChanged(nameof(KartRenk));
            }
        }
        private bool _secili;

        public string Check => Secili ? "?" : "";
        public string KartRenk => Secili ? "#1B1B1B" : "#151515";
        public string FiyatText => $"{Fiyat:0} ?";

        public HizmetItem(int hizmetId, string ad, decimal fiyat, int sureDakika)
        {
            HizmetID = hizmetId;
            Ad = ad;
            Fiyat = fiyat;
            SureDakika = sureDakika <= 0 ? 30 : sureDakika;
        }

        private void OnChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}