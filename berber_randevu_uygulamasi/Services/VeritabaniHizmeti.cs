using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using berber_randevu_uygulamasi.Models;

namespace berber_randevu_uygulamasi.Services
{
    public class VeritabaniHizmeti
    {
        // SQL Server connection string'i artık kullanılmıyor.
        // Bağlantı DbConfig.ConnectionString üzerinden PostgreSQL'e yapılacak.

        // -----------------------------
        //  KULLANICI İŞLEMLERİ
        // -----------------------------

        public async Task<bool> KullaniciEkleAsync(Kullanici yeniKullanici)
        {
            try
            {
                await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
                await conn.OpenAsync();

                string sql = @"INSERT INTO Kullanici (Ad, Soyad, KullaniciAdi, Eposta, Sifre)
                               VALUES (@Ad, @Soyad, @KullaniciAdi, @Eposta, @Sifre);";

                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Ad", yeniKullanici.Ad ?? "");
                cmd.Parameters.AddWithValue("@Soyad", yeniKullanici.Soyad ?? "");
                cmd.Parameters.AddWithValue("@KullaniciAdi", yeniKullanici.KullaniciAdi ?? "");
                cmd.Parameters.AddWithValue("@Eposta", yeniKullanici.Eposta ?? "");
                cmd.Parameters.AddWithValue("@Sifre", yeniKullanici.Sifre ?? "");

                int etkilenen = await cmd.ExecuteNonQueryAsync();
                return etkilenen > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Veritabanı hatası (KullaniciEkleAsync): " + ex.Message);
                return false;
            }
        }

        public async Task<Kullanici?> KullaniciGetirAsync(int kullaniciId)
        {
            try
            {
                await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
                await conn.OpenAsync();

                string sql = @"SELECT ID, Ad, Soyad, KullaniciAdi, Eposta, Sifre, KullaniciTipi
                               FROM Kullanici
                               WHERE ID = @id;";

                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", kullaniciId);

                await using var dr = await cmd.ExecuteReaderAsync();

                if (await dr.ReadAsync())
                {
                    return new Kullanici
                    {
                        ID = dr.GetInt32(0),
                        Ad = dr.IsDBNull(1) ? "" : dr.GetString(1),
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
                await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
                await conn.OpenAsync();

                string sql = @"SELECT BerberID, BerberAdi, Adres, Telefon, ResimYolu, Puan, AcilisSaati, KapanisSaati
                               FROM Berberler;";

                await using var cmd = new NpgsqlCommand(sql, conn);
                await using var dr = await cmd.ExecuteReaderAsync();

                while (await dr.ReadAsync())
                {
                    Berber b = new Berber
                    {
                        BerberID = dr.GetInt32(0),
                        BerberAdi = dr.IsDBNull(1) ? "" : dr.GetString(1),
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

        public async Task<List<CalisanKart>> CalisanlariGetirAsync(int berberId)
        {
            List<CalisanKart> liste = new();

            try
            {
                await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
                await conn.OpenAsync();

                string sql = @"SELECT CalisanID, BerberID, AdSoyad, DeneyimYili, ResimYolu
                               FROM Calisanlar
                               WHERE BerberID = @bid;";

                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@bid", berberId);

                await using var dr = await cmd.ExecuteReaderAsync();

                while (await dr.ReadAsync())
                {
                    // Not: AdSoyad tek alan ise Ad/Soyad ayrıştırmak gerekiyor.
                    // Şimdilik Ad alanına yazdım, Soyad'ı boş bıraktım.
                    var adSoyad = dr.IsDBNull(2) ? "" : dr.GetString(2);

                    CalisanKart c = new CalisanKart
                    {
                        CalisanID = dr.GetInt32(0),
                        BerberID = dr.GetInt32(1),
                        Ad = adSoyad,
                        Soyad = "",
                        Uzmanlik = dr.IsDBNull(3) ? "" : dr.GetString(3),
                        Foto = dr.IsDBNull(4) ? "" : dr.GetString(4)
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
                await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
                await conn.OpenAsync();

                string sql = @"SELECT HizmetID, CalisanID, HizmetAdi, Fiyat, SureDakika
                               FROM Hizmetler
                               WHERE CalisanID = @cid;";

                await using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@cid", calisanId);

                await using var dr = await cmd.ExecuteReaderAsync();

                while (await dr.ReadAsync())
                {
                    Hizmet h = new Hizmet
                    {
                        HizmetID = dr.GetInt32(0),
                        CalisanID = dr.GetInt32(1),
                        HizmetAdi = dr.IsDBNull(2) ? "" : dr.GetString(2),
                        Fiyat = dr.IsDBNull(3) ? 0 : dr.GetDecimal(3),
                        SureDakika = dr.IsDBNull(4) ? 0 : dr.GetInt32(4)
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
                await using var conn = new NpgsqlConnection(DbConfig.ConnectionString);
                await conn.OpenAsync();

                string sql = @"INSERT INTO Randevular
                               (KullaniciID, BerberID, CalisanID, HizmetID,
                                RandevuTarihi, RandevuSaati, SureDakika, ToplamUcret)
                               VALUES
                               (@kullanici, @berber, @calisan, @hizmet,
                                @tarih, @saat, @sure, @ucret);";

                await using var cmd = new NpgsqlCommand(sql, conn);
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
