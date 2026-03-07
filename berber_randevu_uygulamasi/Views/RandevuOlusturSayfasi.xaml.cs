using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using berber_randevu_uygulamasi.Models;
using berber_randevu_uygulamasi.Models.Dtos;
using berber_randevu_uygulamasi.Services;
using Microsoft.Maui.Controls;

namespace berber_randevu_uygulamasi.Views
{
    public partial class RandevuOlusturSayfasi : ContentPage
    {
        protected readonly ApiClient _api;
        public RandevuOlusturVM VM { get; }

        private readonly int _calisanId;
        private readonly int _berberId;

        private DateOnly _seciliTarih = DateOnly.FromDateTime(DateTime.Now);

        private static readonly TimeSpan _gunBas = new(9, 0, 0);
        private static readonly TimeSpan _gunBit = new(18, 0, 0);
        private const int _dakikaAdim = 5;

        private List<(TimeSpan bas, TimeSpan bit)> _doluAraliklar = new();

        private int? _oncekiSaatIndex = null;
        private int? _oncekiDakikaIndex = null;

        private bool _isRefreshing;

        public RandevuOlusturSayfasi(CalisanKart calisan, ApiClient api)
        {
            InitializeComponent();
            _api = api;

            dateTarih.MinimumDate = DateTime.Today;
            dateTarih.MaximumDate = DateTime.Today.AddDays(7);
            dateTarih.Date = DateTime.Today;

            _seciliTarih = DateOnly.FromDateTime(dateTarih.Date);

            _calisanId = calisan.CalisanID;
            _berberId = calisan.BerberID;

            VM = new RandevuOlusturVM();
            BindingContext = VM;

            VM.CalisanAdSoyad = $"{calisan.Ad} {calisan.Soyad}".Trim();
            VM.CalisanFoto = string.IsNullOrWhiteSpace(calisan.Foto) ? "default_berber.png" : calisan.Foto;

            VM.HizmetToggleCommand = new Command<HizmetItem>(async h =>
            {
                VM.HizmetToggle(h);
                await SaatDakikaListeleriniYenileAsync(true);
            });

            pickerSaat.SelectedIndexChanged += async (_, __) => await SaatSecimiDegistiAsync();
            pickerDakika.SelectedIndexChanged += async (_, __) => await DakikaSecimiDegistiAsync();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await RefreshFromApiAsync(true);
        }

        private async void dateTarih_DateSelected(object sender, DateChangedEventArgs e)
        {
            _seciliTarih = DateOnly.FromDateTime(e.NewDate);
            await RefreshFromApiAsync(false);
        }

        private async Task RefreshFromApiAsync(bool keepSelection)
        {
            if (_isRefreshing) return;
            _isRefreshing = true;

            try
            {
                await YukleAsync();
                await SaatDakikaListeleriniYenileAsync(keepSelection);
            }
            finally
            {
                _isRefreshing = false;
            }
        }

        private async Task YukleAsync()
        {
            var hizmetler = await HizmetleriGetirAsync(_calisanId);
            VM.Hizmetler = new ObservableCollection<HizmetItem>(hizmetler);

            _doluAraliklar = await DoluAraliklariGetirAsync(_calisanId, _seciliTarih);
        }

        private int SeciliToplamSureDakika()
        {
            int toplam = VM.ToplamSeciliSureDakika;
            return toplam > 0 ? toplam : 30;
        }

        private bool BaslangicAnindaDoluMu(TimeSpan t)
        {
            return _doluAraliklar.Any(d => t >= d.bas && t < d.bit);
        }

        private bool SlotCakisiyorMu(TimeSpan adayBas, TimeSpan adayBit)
        {
            if (adayBas < _gunBas) return true;
            if (adayBit > _gunBit) return true;

            return _doluAraliklar.Any(d => adayBas < d.bit && adayBit > d.bas);
        }

        private bool SlotGectiMi(TimeSpan adayBas)
        {
            bool bugunMu = _seciliTarih == DateOnly.FromDateTime(DateTime.Now);
            if (!bugunMu) return false;

            var simdi = TimeOnly.FromDateTime(DateTime.Now).ToTimeSpan();
            return adayBas < simdi;
        }

        private async Task SaatDakikaListeleriniYenileAsync(bool keepSelection)
        {
            _doluAraliklar = await DoluAraliklariGetirAsync(_calisanId, _seciliTarih);

            var saatOps = new List<OptionItem>();

            for (int h = _gunBas.Hours; h < _gunBit.Hours; h++)
            {
                bool saatTamDolu = true;
                bool saatTamGecti = true;

                for (int dk = 0; dk < 60; dk += _dakikaAdim)
                {
                    var bas = new TimeSpan(h, dk, 0);

                    bool dolu = BaslangicAnindaDoluMu(bas);
                    bool gecti = SlotGectiMi(bas);

                    if (!dolu) saatTamDolu = false;
                    if (!gecti) saatTamGecti = false;
                }

                string text = $"{h:00}";
                if (saatTamDolu) text += " (Dolu)";
                else if (saatTamGecti) text += " (Geçti)";

                bool secilemez = saatTamDolu || saatTamGecti;
                saatOps.Add(new OptionItem(h, text, secilemez));
            }

            int? oncekiSaatValue =
                (keepSelection && pickerSaat.SelectedItem is OptionItem os) ? os.Value : null;

            pickerSaat.ItemsSource = saatOps;

            int secSaatIndex = -1;

            if (oncekiSaatValue.HasValue)
                secSaatIndex = saatOps.FindIndex(x => x.Value == oncekiSaatValue.Value && !x.IsDisabled);

            if (secSaatIndex < 0)
                secSaatIndex = saatOps.FindIndex(x => !x.IsDisabled);

            pickerSaat.SelectedIndex = secSaatIndex;

            await DakikalariDoldurAsync(keepSelection);
            OzetGuncelle();
        }

        private async Task SaatSecimiDegistiAsync()
        {
            if (pickerSaat.SelectedItem is not OptionItem saatItem)
                return;

            if (saatItem.IsDisabled)
            {
                await DisplayAlert("Uyarı", "Bu saat seçilemez (dolu/geçti). Lütfen başka saat seçin.", "Tamam");

                if (_oncekiSaatIndex.HasValue)
                    pickerSaat.SelectedIndex = _oncekiSaatIndex.Value;
                else if (pickerSaat.ItemsSource is List<OptionItem> saatler)
                    pickerSaat.SelectedIndex = saatler.FindIndex(x => !x.IsDisabled);

                return;
            }

            _oncekiSaatIndex = pickerSaat.SelectedIndex;

            await DakikalariDoldurAsync(false);
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

                bool dolu = BaslangicAnindaDoluMu(bas);
                bool gecti = SlotGectiMi(bas);

                string text = $"{dk:00}";
                if (dolu) text += " (Dolu)";
                else if (gecti) text += " (Geçti)";

                bool secilemez = dolu || gecti;
                dkOps.Add(new OptionItem(dk, text, secilemez));
            }

            int? oncekiDakikaValue =
                (keepSelection && pickerDakika.SelectedItem is OptionItem od) ? od.Value : null;

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
                await DisplayAlert("Uyarı", "Bu dakika seçilemez (dolu/geçti). Lütfen başka dakika seçin.", "Tamam");

                if (_oncekiDakikaIndex.HasValue)
                    pickerDakika.SelectedIndex = _oncekiDakikaIndex.Value;
                else if (pickerDakika.ItemsSource is List<OptionItem> dakikalar)
                    pickerDakika.SelectedIndex = dakikalar.FindIndex(x => !x.IsDisabled);

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
                $"Tarih: {_seciliTarih:dd.MM.yyyy}  •  Başlangıç: {bas:hh\\:mm}  •  Bitiş: {bit:hh\\:mm}  •  Süre: {toplamSure} dk";
        }

        private async Task<List<HizmetItem>> HizmetleriGetirAsync(int calisanId)
        {
            var dtoList = await _api.CalisanaGoreHizmetleriGetirAsync(calisanId);
            var liste = new List<HizmetItem>();

            foreach (var x in dtoList)
            {
                int sure = x.SureDakika <= 0 ? 30 : x.SureDakika;

                liste.Add(new HizmetItem(
                    x.HizmetID,
                    x.HizmetAdi ?? "",
                    x.Fiyat,
                    sure));
            }

            return liste;
        }

        private async Task<List<(TimeSpan bas, TimeSpan bit)>> DoluAraliklariGetirAsync(int calisanId, DateOnly tarih)
        {
            var dtoList = await _api.DoluAraliklariGetirAsync(calisanId, tarih);

            var liste = new List<(TimeSpan bas, TimeSpan bit)>();

            foreach (var x in dtoList)
            {
                if (!TimeSpan.TryParse(x.Baslangic, out var bas)) continue;
                if (!TimeSpan.TryParse(x.Bitis, out var bit)) continue;

                liste.Add((bas, bit));
            }

            return liste;
        }

        private async void Hizmet_Tapped(object sender, TappedEventArgs e)
        {
            if ((sender as BindableObject)?.BindingContext is HizmetItem item)
            {
                VM.HizmetToggle(item);
                await SaatDakikaListeleriniYenileAsync(true);
            }
        }

        private async void RandevuOlustur_Clicked(object sender, EventArgs e)
        {
            try
            {
                var seciliHizmetler = VM.Hizmetler.Where(x => x.Secili).ToList();
                if (seciliHizmetler.Count == 0)
                {
                    await DisplayAlert("Uyarı", "Hizmet seçiniz.", "Tamam");
                    return;
                }

                if (pickerSaat.SelectedItem is not OptionItem s || s.IsDisabled)
                {
                    await DisplayAlert("Uyarı", "Uygun bir saat seçiniz.", "Tamam");
                    return;
                }

                if (pickerDakika.SelectedItem is not OptionItem d || d.IsDisabled)
                {
                    await DisplayAlert("Uyarı", "Uygun bir dakika seçiniz.", "Tamam");
                    return;
                }

                int toplamSure = seciliHizmetler.Sum(x => x.SureDakika);
                if (toplamSure <= 0) toplamSure = 30;

                decimal toplamUcret = seciliHizmetler.Sum(x => x.Fiyat);
                int hizmetId = seciliHizmetler.First().HizmetID;

                var adayBas = new TimeSpan(s.Value, d.Value, 0);
                var adayBit = adayBas.Add(TimeSpan.FromMinutes(toplamSure));

                if (SlotCakisiyorMu(adayBas, adayBit))
                {
                    await DisplayAlert("Uyarı", "Bu saat aralığı seçilen hizmet süresiyle çakışıyor.", "Tamam");
                    await RefreshFromApiAsync(true);
                    return;
                }

                if (SlotGectiMi(adayBas))
                {
                    await DisplayAlert("Uyarı", "Geçmiş bir saat seçilemez.", "Tamam");
                    return;
                }

                bool onay = await DisplayAlert(
                    "Randevu Onayı",
                    $"Tarih: {_seciliTarih:dd.MM.yyyy}\n" +
                    $"Başlangıç: {adayBas:hh\\:mm}\n" +
                    $"Bitiş: {adayBit:hh\\:mm}\n" +
                    $"Toplam Süre: {toplamSure} dk\n" +
                    $"Toplam Ücret: {toplamUcret:0} ₺\n\n" +
                    $"Randevuyu oluşturmak istiyor musunuz?",
                    "Tamam",
                    "İptal");

                if (!onay) return;

                var req = new RandevuCreateRequest
                {
                    KullaniciId = UserSession.KullaniciId,
                    CalisanId = _calisanId,
                    HizmetId = hizmetId,
                    Tarih = _seciliTarih.ToString("yyyy-MM-dd"),
                    Saat = adayBas.ToString(@"hh\:mm")
                };

                var result = await _api.RandevuOlusturAsync(req);

                if (!result.Success)
                {
                    await DisplayAlert("Uyarı", result.Message, "Tamam");
                    await RefreshFromApiAsync(true);
                    return;
                }

                await DisplayAlert("Başarılı", result.Message, "Tamam");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", ex.Message, "Tamam");
            }
        }

        private async void Geri_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }

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

        public override string ToString() => Text;
    }

    public class RandevuOlusturVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string CalisanAdSoyad
        {
            get => _calisanAdSoyad;
            set { _calisanAdSoyad = value; Raise(); }
        }
        private string _calisanAdSoyad = "";

        public string CalisanFoto
        {
            get => _calisanFoto;
            set { _calisanFoto = value; Raise(); }
        }
        private string _calisanFoto = "default_berber.png";

        public ObservableCollection<HizmetItem> Hizmetler
        {
            get => _hizmetler;
            set
            {
                _hizmetler = value;
                Raise();
                Raise(nameof(SeciliHizmetSayisi));
                Raise(nameof(ToplamSeciliSureDakika));
            }
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
            Raise(nameof(ToplamSeciliSureDakika));
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

        public string Check => Secili ? "✓" : "";
        public string KartRenk => Secili ? "#1B1B1B" : "#151515";
        public string FiyatText => $"{Fiyat:0} ₺";

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