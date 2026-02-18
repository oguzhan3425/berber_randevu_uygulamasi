using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace berber_randevu_uygulamasi.Views
{
    public partial class RandevuOlusturSayfasi : ContentPage
    {
        public RandevuOlusturVM VM { get; }

        // Bu sayfaya CalisanKart ile gelmeni öneririm:
        // await Navigation.PushAsync(new RandevuOlusturSayfasi(secilenCalisan));
        public RandevuOlusturSayfasi(Models.CalisanKart calisan)
        {
            InitializeComponent();

            VM = new RandevuOlusturVM
            {
                CalisanAdSoyad = $"{calisan.Ad} {calisan.Soyad}".Trim(),
                CalisanFoto = string.IsNullOrWhiteSpace(calisan.Foto) ? "default_berber.png" : calisan.Foto
            };

            BindingContext = VM;

            // Örnek boþ saatler (istersen DB'den doldururuz)
            VM.Saatler = new ObservableCollection<SaatItem>
            {
                new("09:00"), new("09:30"), new("10:00", dolu:true),
                new("10:30", dolu:true), new("11:00"), new("11:30")
            };

            // Örnek hizmetler (istersen Hizmetler tablosundan çekeriz)
            VM.Hizmetler = new ObservableCollection<HizmetItem>
            {
                new("Saç Kesimi", 250),
                new("Sakal", 100),
                new("Saç + Sakal", 320),
            };

            VM.SaatSecCommand = new Command<SaatItem>(VM.SaatSec);
            VM.HizmetToggleCommand = new Command<HizmetItem>(VM.HizmetToggle);
        }

        private async void RandevuOlustur_Clicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(VM.SeciliSaat))
            {
                await DisplayAlert("Uyarý", "Lütfen bir saat seçiniz.", "Tamam");
                return;
            }

            if (VM.SeciliHizmetSayisi == 0)
            {
                await DisplayAlert("Uyarý", "Lütfen en az 1 hizmet seçiniz.", "Tamam");
                return;
            }

            // Buraya DB insert gelecek (istersen bir sonraki adýmda ekleyelim)

            await DisplayAlert("Baþarýlý", "Randevunuz oluþturuldu.", "Tamam");

            // AnaSayfa'ya yönlendir (sen Navigation.PushAsync kullanýyorsun)
            await Navigation.PushAsync(new AnaSayfa());
        }

        private async void Geri_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }

    public class RandevuOlusturVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string CalisanAdSoyad { get => _calisanAdSoyad; set { _calisanAdSoyad = value; OnChanged(); } }
        private string _calisanAdSoyad = "";

        public string CalisanFoto { get => _calisanFoto; set { _calisanFoto = value; OnChanged(); } }
        private string _calisanFoto = "default_berber.png";

        public ObservableCollection<SaatItem> Saatler { get => _saatler; set { _saatler = value; OnChanged(); } }
        private ObservableCollection<SaatItem> _saatler = new();

        public ObservableCollection<HizmetItem> Hizmetler { get => _hizmetler; set { _hizmetler = value; OnChanged(); } }
        private ObservableCollection<HizmetItem> _hizmetler = new();

        public ICommand? SaatSecCommand { get; set; }
        public ICommand? HizmetToggleCommand { get; set; }

        public string SeciliSaat { get => _seciliSaat; set { _seciliSaat = value; OnChanged(); } }
        private string _seciliSaat = "";

        public int SeciliHizmetSayisi => Hizmetler.Count(h => h.Secili);

        public void SaatSec(SaatItem? item)
        {
            if (item == null) return;
            if (item.Dolu) return;

            foreach (var s in Saatler)
                s.Secili = false;

            item.Secili = true;
            SeciliSaat = item.Saat;
            OnChanged(nameof(SeciliHizmetSayisi)); // gerekmez ama dursun
        }

        public void HizmetToggle(HizmetItem? item)
        {
            if (item == null) return;
            item.Secili = !item.Secili;
            OnChanged(nameof(SeciliHizmetSayisi));
        }

        private void OnChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class SaatItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string Saat { get; }
        public bool Dolu { get; }

        public bool Secili
        {
            get => _secili;
            set { _secili = value; OnChanged(); OnChanged(nameof(KartRenk)); OnChanged(nameof(YaziRenk)); }
        }
        private bool _secili;

        public string KartRenk => Dolu ? "#2A2A2A" : (Secili ? "#1B1B1B" : "#151515");
        public string YaziRenk => Dolu ? "#6F6F6F" : "#F2F2F2";

        public SaatItem(string saat, bool dolu = false)
        {
            Saat = saat;
            Dolu = dolu;
        }

        private void OnChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class HizmetItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string Ad { get; }
        public decimal Fiyat { get; }

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

        public HizmetItem(string ad, decimal fiyat)
        {
            Ad = ad;
            Fiyat = fiyat;
        }

        private void OnChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}