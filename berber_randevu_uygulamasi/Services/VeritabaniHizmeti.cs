using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using berber_randevu_uygulamasi.Models;

namespace berber_randevu_uygulamasi.Services
{
    public class VeritabaniHizmeti
    {
        private readonly string _connectionString =
            "Data Source=Oğuzhan\\SQLEXPRESS;Initial Catalog=BerberDB;Integrated Security=True;TrustServerCertificate=True;";

        // -----------------------------
        //  KULLANICI İŞLEMLERİ
        // -----------------------------

        public async Task<bool> KullaniciEkleAsync(Kullanici yeniKullanici)
        {
            try
            {
                using SqlConnection baglanti = new SqlConnection(_connectionString);
                await baglanti.OpenAsync();

                string sorgu = @"INSERT INTO Kullanici (Ad, Soyad, KullaniciAdi, Eposta, Sifre)
                                 VALUES (@Ad, @Soyad, @KullaniciAdi, @Eposta, @Sifre)";

                using SqlCommand komut = new SqlCommand(sorgu, baglanti);
                komut.Parameters.AddWithValue("@Ad", yeniKullanici.Ad);
                komut.Parameters.AddWithValue("@Soyad", yeniKullanici.Soyad);
                komut.Parameters.AddWithValue("@KullaniciAdi", yeniKullanici.KullaniciAdi);
                komut.Parameters.AddWithValue("@Eposta", yeniKullanici.Eposta);
                komut.Parameters.AddWithValue("@Sifre", yeniKullanici.Sifre);

                await komut.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Veritabanı hatası (KullaniciEkleAsync): " + ex.Message);
                return false;
            }
        }

        // Örnek: ID ile kullanıcı çekmek istersen
        public async Task<Kullanici?> KullaniciGetirAsync(int kullaniciId)
        {
            try
            {
                using SqlConnection baglanti = new SqlConnection(_connectionString);
                await baglanti.OpenAsync();

                string sorgu = @"SELECT ID, Ad, Soyad, KullaniciAdi, Eposta, Sifre, KullaniciTipi
                                 FROM Kullanici WHERE ID = @id";

                using SqlCommand komut = new SqlCommand(sorgu, baglanti);
                komut.Parameters.AddWithValue("@id", kullaniciId);

                using SqlDataReader dr = await komut.ExecuteReaderAsync();
                if (await dr.ReadAsync())
                {
                    return new Kullanici
                    {
                        ID = dr.GetInt32(0),
                        Ad = dr.GetString(1),
                        Soyad = dr.IsDBNull(2) ? "" : dr.GetString(2),
                        KullaniciAdi = dr.IsDBNull(3) ? "" : dr.GetString(3),
                        Eposta = dr.IsDBNull(4) ? "" : dr.GetString(4),
                        Sifre = dr.IsDBNull(5) ? "" : dr.GetString(5),
                        KullaniciTipi = dr.IsDBNull(6) ? "" : dr.GetString(6)
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Veritabanı hatası (KullaniciGetirAsync): " + ex.Message);
                return null;
            }
        }

        // -----------------------------
        //  BERBER İŞLEMLERİ
        // -----------------------------

        public async Task<List<Berber>> BerberleriGetirAsync()
        {
            List<Berber> liste = new();

            try
            {
                using SqlConnection conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                string query = @"SELECT BerberID, BerberAdi, Adres, Telefon, ResimYolu, Puan, AcilisSaati, KapanisSaati
                                 FROM Berberler";

                using SqlCommand cmd = new SqlCommand(query, conn);
                using SqlDataReader dr = await cmd.ExecuteReaderAsync();

                while (await dr.ReadAsync())
                {
                    Berber b = new Berber
                    {
                        BerberID = dr.GetInt32(0),
                        BerberAdi = dr.GetString(1),
                        Adres = dr.IsDBNull(2) ? "" : dr.GetString(2),
                        Telefon = dr.IsDBNull(3) ? "" : dr.GetString(3),
                        ResimYolu = dr.IsDBNull(4) ? "" : dr.GetString(4),
                        Puan = dr.IsDBNull(5) ? 0 : dr.GetDecimal(5),
                        AcilisSaati = dr.IsDBNull(6) ? TimeSpan.Zero : dr.GetTimeSpan(6),
                        KapanisSaati = dr.IsDBNull(7) ? TimeSpan.Zero : dr.GetTimeSpan(7)
                    };

                    liste.Add(b);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Veritabanı hatası (BerberleriGetirAsync): " + ex.Message);
            }

            return liste;
        }

        // -----------------------------
        //  ÇALIŞAN İŞLEMLERİ
        // -----------------------------

        public async Task<List<Calisan>> CalisanlariGetirAsync(int berberId)
        {
            List<Calisan> liste = new();

            try
            {
                using SqlConnection conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                string query = @"SELECT CalisanID, BerberID, AdSoyad, DeneyimYili, ResimYolu
                                 FROM Calisanlar
                                 WHERE BerberID = @bid";

                using SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@bid", berberId);

                using SqlDataReader dr = await cmd.ExecuteReaderAsync();

                while (await dr.ReadAsync())
                {
                    Calisan c = new Calisan
                    {
                        CalisanID = dr.GetInt32(0),
                        BerberID = dr.GetInt32(1),
                        AdSoyad = dr.GetString(2),
                        DeneyimYili = dr.IsDBNull(3) ? 0 : dr.GetInt32(3),
                        ResimYolu = dr.IsDBNull(4) ? "" : dr.GetString(4)
                    };

                    liste.Add(c);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Veritabanı hatası (CalisanlariGetirAsync): " + ex.Message);
            }

            return liste;
        }

        // -----------------------------
        //  HİZMET İŞLEMLERİ
        // -----------------------------

        public async Task<List<Hizmet>> HizmetleriGetirAsync(int calisanId)
        {
            List<Hizmet> liste = new();

            try
            {
                using SqlConnection conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                string query = @"SELECT HizmetID, CalisanID, HizmetAdi, Fiyat, SureDakika
                                 FROM Hizmetler
                                 WHERE CalisanID = @cid";

                using SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@cid", calisanId);

                using SqlDataReader dr = await cmd.ExecuteReaderAsync();

                while (await dr.ReadAsync())
                {
                    Hizmet h = new Hizmet
                    {
                        HizmetID = dr.GetInt32(0),
                        CalisanID = dr.GetInt32(1),
                        HizmetAdi = dr.GetString(2),
                        Fiyat = dr.GetDecimal(3),
                        SureDakika = dr.GetInt32(4)
                    };

                    liste.Add(h);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Veritabanı hatası (HizmetleriGetirAsync): " + ex.Message);
            }

            return liste;
        }

        // -----------------------------
        //  RANDEVU İŞLEMLERİ
        // -----------------------------

        public async Task<bool> RandevuEkleAsync(Randevu r)
        {
            try
            {
                using SqlConnection conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                string query = @"INSERT INTO Randevular
                                 (KullaniciID, BerberID, CalisanID, HizmetID,
                                  RandevuTarihi, RandevuSaati, SureDakika, ToplamUcret)
                                 VALUES
                                 (@kullanici, @berber, @calisan, @hizmet,
                                  @tarih, @saat, @sure, @ucret)";

                using SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@kullanici", r.KullaniciID);
                cmd.Parameters.AddWithValue("@berber", r.BerberID);
                cmd.Parameters.AddWithValue("@calisan", r.CalisanID);
                cmd.Parameters.AddWithValue("@hizmet", r.HizmetID);
                cmd.Parameters.AddWithValue("@tarih", r.RandevuTarihi);
                cmd.Parameters.AddWithValue("@saat", r.RandevuSaati);
                cmd.Parameters.AddWithValue("@sure", r.SureDakika);
                cmd.Parameters.AddWithValue("@ucret", r.ToplamUcret);

                int etkilenen = await cmd.ExecuteNonQueryAsync();
                return etkilenen > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Veritabanı hatası (RandevuEkleAsync): " + ex.Message);
                return false;
            }
        }
    }
}
