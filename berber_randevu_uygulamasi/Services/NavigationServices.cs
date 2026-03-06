using Microsoft.Maui.Controls;

namespace berber_randevu_uygulamasi.Services
{
    public class NavigationServices
    {
        private static Page? RootPage
            => Application.Current?.Windows.Count > 0
                ? Application.Current.Windows[0].Page
                : null;

        private static INavigation? Nav
            => RootPage?.Navigation;

        public async Task PushAsync(Page page)
        {
            if (Nav == null) return;
            await Nav.PushAsync(page);
        }

        public async Task PopAsync()
        {
            if (Nav == null) return;
            await Nav.PopAsync();
        }

        public void ReplaceRoot(Page page)
        {
            if (Application.Current?.Windows.Count > 0)
                Application.Current.Windows[0].Page = new NavigationPage(page);
        }

        public Page? CurrentPage()
            => Nav?.NavigationStack?.LastOrDefault();
    }
}