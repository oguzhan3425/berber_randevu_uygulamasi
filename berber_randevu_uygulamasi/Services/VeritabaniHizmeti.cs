using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using berber_randevu_uygulamasi.Models;

namespace berber_randevu_uygulamasi.Services
{
    public class VeritabaniHizmeti
    {
        private readonly string _connectionString =
            "Data Source=Oğuzhan\\SQLEXPRESS;Initial Catalog=BerberDB;Integrated Security=True;TrustServerCertificate=True;";

        public async Task<bool> KullaniciEkleAsync(Kullanici yeniKullanici)
        {
            try
            {
                using (SqlConnection baglanti = new SqlConnection(_connectionString))
                {
                    await baglanti.OpenAsync();

                    string sorgu = "INSERT INTO Kullanici (Ad, Soyad, KullaniciAdi, Eposta, Sifre) VALUES (@Ad, @Soyad, @KullaniciAdi, @Eposta, @Sifre)";
                    using (SqlCommand komut = new SqlCommand(sorgu, baglanti))
                    {
                        komut.Parameters.AddWithValue("@Ad", yeniKullanici.Ad);
                        komut.Parameters.AddWithValue("@Soyad", yeniKullanici.Soyad);
                        komut.Parameters.AddWithValue("@KullaniciAdi", yeniKullanici.KullaniciAdi);
                        komut.Parameters.AddWithValue("@Eposta", yeniKullanici.Eposta);
                        komut.Parameters.AddWithValue("@Sifre", yeniKullanici.Sifre);

                        await komut.ExecuteNonQueryAsync();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Veritabanı hatası: " + ex.Message);
                return false;
            }
        }
    }
}
