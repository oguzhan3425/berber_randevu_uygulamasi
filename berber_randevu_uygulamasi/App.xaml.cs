using berber_randevu_uygulamasi.Views;

namespace berber_randevu_uygulamasi
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new NavigationPage(new GirisSayfasi()));
        }
        
    }
}