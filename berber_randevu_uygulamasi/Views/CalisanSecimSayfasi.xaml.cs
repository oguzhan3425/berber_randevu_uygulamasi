using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using berber_randevu_uygulamasi.Models;
using berber_randevu_uygulamasi.Models.Dtos;
using berber_randevu_uygulamasi.Services;

namespace berber_randevu_uygulamasi.Views
{
    public partial class CalisanSecimSayfasi : ContentPage
    {
        protected readonly ApiClient _api;
        private readonly Berber _berber;

        public CalisanSecimSayfasi(Berber berber, ApiClient api)
        {
            InitializeComponent();
            _berber = berber;
            _api = api;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CalisanlariYukleAsync();
        }

        private async System.Threading.Tasks.Task CalisanlariYukleAsync()
        {
            try
            {
                var dtoListe = await _api.GetAsync<List<CalisanListeDto>>($"calisanlar/by-berber/{_berber.BerberID}");

                if (dtoListe == null)
                {
                    await DisplayAlert("Hata", "Çalýþan listesi alýnamadý.", "Tamam");
                    return;
                }

                var liste = new List<CalisanKart>();

                foreach (var item in dtoListe)
                {
                    liste.Add(new CalisanKart
                    {
                        CalisanID = item.CalisanID,
                        BerberID = item.BerberID,
                        Ad = item.Ad ?? "",
                        Soyad = item.Soyad ?? "",
                        Uzmanlik = item.Uzmanlik ?? "",
                        Foto = string.IsNullOrWhiteSpace(item.FotoUrl)
                            ? "default_berber.png"
                            : item.FotoUrl
                    });
                }

                CalisanCollection.ItemsSource = liste;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", "Çalýþanlar yüklenemedi:\n" + ex.Message, "Tamam");
            }
        }

        private async void CalisanCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is CalisanKart secilen)
            {
                CalisanCollection.SelectedItem = null;
                await Navigation.PushAsync(new RandevuOlusturSayfasi(secilen, _api));
            }
        }
    }
}