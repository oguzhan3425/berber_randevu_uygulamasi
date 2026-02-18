using System;
using System.IO;
using Microsoft.Maui.Controls;
using Npgsql;
using berber_randevu_uygulamasi.Services;

namespace berber_randevu_uygulamasi.Views.AltBarlar
{
    public enum FotoHedefi
    {
        SahipProfilFoto,   // kullanici."ProfilFoto"
        DukkanFoto         // "Berberler"."ResimYolu"
    }

    public partial class ProfilFotoDegistirSayfasi : ContentPage
    {
        private readonly FotoHedefi _hedef;
        private readonly int _id; // hedefe göre KullaniciId veya BerberID

        // ? Eski kullaným bozulmasýn diye parametresiz constructor da kalsýn:
        public ProfilFotoDegistirSayfasi()
            : this(FotoHedefi.SahipProfilFoto, UserSession.KullaniciId)
        {
        }

        // ? Yeni: hangi tablo/kolon güncellenecek onu söyleyerek açacaðýz
        public ProfilFotoDegistirSayfasi(FotoHedefi hedef, int id)
        {
            InitializeComponent();
            _hedef = hedef;
            _id = id;

            // Ýstersen baþlýðý hedefe göre deðiþtir
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

                string savedPath = await SavePhotoToAppFolder(photo);
                await UpdateFotoInDb(savedPath);

                await DisplayAlert("Baþarýlý", "Fotoðraf güncellendi.", "Tamam");
                await Navigation.PopAsync();
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

                string savedPath = await SavePhotoToAppFolder(photo);
                await UpdateFotoInDb(savedPath);

                await DisplayAlert("Baþarýlý", "Fotoðraf güncellendi.", "Tamam");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Hata", ex.Message, "Tamam");
            }
        }

        private static async System.Threading.Tasks.Task<string> SavePhotoToAppFolder(FileResult photo)
        {
            string folder = FileSystem.AppDataDirectory;
            string fileName = $"pp_{DateTime.Now:yyyyMMdd_HHmmss}{Path.GetExtension(photo.FileName)}";
            string fullPath = Path.Combine(folder, fileName);

            await using var src = await photo.OpenReadAsync();
            await using var dst = File.OpenWrite(fullPath);
            await src.CopyToAsync(dst);

            return fullPath; // DB'ye path yazýyoruz
        }

        private async System.Threading.Tasks.Task UpdateFotoInDb(string path)
        {
            await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
            await conn.OpenAsync();

            if (_hedef == FotoHedefi.SahipProfilFoto)
            {
                // ? Senin mevcut yapýn: kullanici."ProfilFoto" (string path)
                string sql = @"
                    UPDATE kullanici
                    SET ""ProfilFoto"" = @p
                    WHERE ""ID"" = @id;";

                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@p", path);
                cmd.Parameters.AddWithValue("@id", _id);
                await cmd.ExecuteNonQueryAsync();
            }
            else // FotoHedefi.DukkanFoto
            {
                // ? Senin DB yapýn: "Berberler"."ResimYolu" (string)
                string sql = @"
                    UPDATE ""Berberler""
                    SET ""ResimYolu"" = @p
                    WHERE ""BerberID"" = @id;";

                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@p", path);
                cmd.Parameters.AddWithValue("@id", _id);
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
