using Microsoft.Maui.Controls;
using berber_randevu_uygulamasi.Views;

namespace berber_randevu_uygulamasi
{
    public partial class App : Application
    {
        private readonly IServiceProvider _sp;

        public App(IServiceProvider sp)
        {
            InitializeComponent();
            _sp = sp;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Başlangıç sayfası DI ile resolve edilir
            var firstPage = _sp.GetRequiredService<GirisSayfasi>();

            // Temiz navigation temeli: tek NavigationPage
            return new Window(new NavigationPage(firstPage));
        }
    }
}