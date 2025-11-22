using Microsoft.Maui.Controls;
using System;

namespace berber_randevu_uygulamasi.Views
{
    public partial class AnaSayfa : ContentPage
    {
        private string kullaniciAdi;
        private string kullaniciTipi;
        private string _ad="";
        private string _soyad="";

        public AnaSayfa(string ad, string tipi)
        {
            InitializeComponent();
            kullaniciAdi = ad;
            kullaniciTipi = tipi;

            

            // Eðer berber deðilse Hizmetler butonunu gizle
            if (kullaniciTipi != "Berber")
            {
                
            }
        }
        private void GecmisRandevular_Tapped(object sender, TappedEventArgs e)
        {
            // geçiþ kodunu koyacam
        }
        private void AnaSayfaClicked(object sender, EventArgs e)
        {
            // Ana sayfadan baþlýyor ama butonuda eksik olabilir bakacam
        }
         
        private void RandevuAlClicked(object sender, EventArgs e)
        {
            // geçiþ kodunu koyacam
        }

        private void HizmetlerClicked(object sender, EventArgs e)
        {
            // geçiþ kodunu koyacam
        }

        private async void ProfilClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ProfilDuzenleSayfasi(_ad, _soyad));
        }
    }
}
