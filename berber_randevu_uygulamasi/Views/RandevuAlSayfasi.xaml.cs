using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using berber_randevu_uygulamasi.Models;
using berber_randevu_uygulamasi.Models.Dtos;
using berber_randevu_uygulamasi.Services;

namespace berber_randevu_uygulamasi.Views
{
    public partial class RandevuAlSayfasi : ContentPage
    {
        protected readonly ApiClient _api;
        private readonly List<Berber> tumBerberler = new();

        public RandevuAlSayfasi(ApiClient api)
        {
            InitializeComponent();
            _api = api;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await BerberleriYukleAsync();
        }

        // ---------------------------
        //  API'DEN BERBER ÇEKME
        // ---------------------------
        private async Task BerberleriYukleAsync()
        {
            try
            {
                tumBerberler.Clear();

                var liste = await _api.GetAsync<List<BerberListeDto>>("berberler");

                if (liste == null)
                {
                    await DisplayAlert("Hata", "Berber listesi alýnamadý.", "Tamam");
                    return;
                }

                foreach (var item in liste)
                {
                    tumBerberler.Add(new Berber
                    {
                        BerberID = item.BerberId,
                        BerberAdi = item.BerberAdi ?? "",
                        Adres = item.Adres ?? "",
                        Telefon = item.Telefon ?? "",
                        ResimYolu = string.IsNullOrWhiteSpace(item.DukkanFotoUrl)
                            ? "default_berber.png"
                            : item.DukkanFotoUrl,
                        Puan = item.Puan,
                        AcilisSaati = ParseTime(item.Acilis),
                        KapanisSaati = ParseTime(item.Kapanis)
                    });
                }

                BerberCollection.ItemsSource = tumBerberler;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", "Berberler yüklenemedi: " + ex.Message, "Tamam");
            }
        }

        private TimeSpan ParseTime(string? text)
        {
            return TimeSpan.TryParse(text, out var ts) ? ts : TimeSpan.Zero;
        }

        // ---------------------------
        //  ARAMA
        // ---------------------------
        private void BerberAraEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            string arama = e.NewTextValue?.Trim().ToLower() ?? "";

            var filtreliListe = tumBerberler
                .Where(b => (b.BerberAdi ?? "").ToLower().Contains(arama))
                .ToList();

            BerberCollection.ItemsSource = filtreliListe;
        }

        // ---------------------------
        //  BERBER SEÇÝMÝ
        // ---------------------------
        private async void BerberCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Berber secilenBerber)
            {
                BerberCollection.SelectedItem = null;
                await Navigation.PushAsync(new CalisanSecimSayfasi(secilenBerber, _api));
            }
        }

        // ---------------------------
        //  ALT BAR
        // ---------------------------
        private async void AnaSayfaClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AnaSayfa(_api));
        }

        private async void RandevuAlClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RandevuAlSayfasi(_api));
        }

        private async void ProfilClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ProfilDuzenleSayfasi(_api));
        }
    }
}