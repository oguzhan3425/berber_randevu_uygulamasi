using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using berber_randevu_uygulamasi.Services;
using berber_randevu_uygulamasi.Models.Dtos;

namespace berber_randevu_uygulamasi.Views.AltBarlar
{
    public enum FotoHedefi
    {
        SahipProfilFoto,
        DukkanFoto
    }

    public partial class ProfilFotoDegistirSayfasi : ContentPage
    {
        private readonly FotoHedefi _hedef;
        private readonly int _id;
        private readonly ApiClient _api;

        public ProfilFotoDegistirSayfasi(ApiClient api, FotoHedefi hedef, int id)
        {
            InitializeComponent();
            _api = api;
            _hedef = hedef;
            _id = id;

            Title = _hedef == FotoHedefi.DukkanFoto ? "Dükkan Fotoðrafý" : "Profil Fotoðrafý";
        }

        private async void KameraAc_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (!MediaPicker.Default.IsCaptureSupported)
                {
                    await DisplayAlert("Bilgi", "Bu cihaz kamerayý desteklemiyor.", "Tamam");
                    return;
                }

                var photo = await MediaPicker.Default.CapturePhotoAsync();
                if (photo == null) return;

                await PreviewAndUploadAsync(photo);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", ex.Message, "Tamam");
            }
        }

        private async void GaleridenSec_Clicked(object sender, EventArgs e)
        {
            try
            {
                var photo = await MediaPicker.Default.PickPhotoAsync();
                if (photo == null) return;

                await PreviewAndUploadAsync(photo);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", ex.Message, "Tamam");
            }
        }

        private async Task PreviewAndUploadAsync(FileResult photo)
        {
            // 1) Fotoðrafý byte[] olarak al (hem önizleme hem upload için en saðlam yöntem)
            byte[] bytes;
            await using (var s = await photo.OpenReadAsync())
            await using (var ms = new MemoryStream())
            {
                await s.CopyToAsync(ms);
                bytes = ms.ToArray();
            }

            
            // 3) Endpoint seç
            var endpoint = _hedef == FotoHedefi.DukkanFoto
                ? $"photos/dukkan/{_id}"
                : $"photos/profil/{_id}";

            // 4) Dosya adý güvenli olsun
            var fileName = string.IsNullOrWhiteSpace(photo.FileName)
                ? $"photo_{DateTime.Now:yyyyMMdd_HHmmss}.jpg"
                : photo.FileName;

            // 5) Upload (byte[] -> MemoryStream)
            await using var uploadStream = new MemoryStream(bytes);

            var (ok, status, body, data) = await _api.UploadPhotoDebugAsync<PhotoUploadResponse>(
    endpoint,
    uploadStream,
    fileName,
    contentType: "application/octet-stream"
);

            if (!ok)
            {
                // Burada artýk gerçek hata var
                await DisplayAlert("Upload Hata", $"Status: {status}\nBody: {body}", "Tamam");
                return;
            }

            if (data == null)
            {
                await DisplayAlert("Upload Hata", $"JSON okunamadý.\nStatus: {status}\nBody: {body}", "Tamam");
                return;
            }

            if (!data.Basarili)
            {
                await DisplayAlert("Hata", data.Mesaj ?? "Yükleme baþarýsýz.", "Tamam");
                return;
            }

            await DisplayAlert("Baþarýlý", data.Mesaj ?? "Fotoðraf güncellendi.", "Tamam");
            await Navigation.PopAsync();
        }
    }
}