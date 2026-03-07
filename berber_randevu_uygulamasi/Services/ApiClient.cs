using berber_randevu_uygulamasi.Models;
using berber_randevu_uygulamasi.Models.Dtos;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace berber_randevu_uygulamasi.Services
{
    public class ApiClient
    {
        private readonly HttpClient _http;

        public ApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<CalisanRandevuDto>> GetCalisanGelecekRandevularAsync(int kullaniciId)
        {
            return await _http.GetFromJsonAsync<List<CalisanRandevuDto>>(
                       $"calisan-randevular/gelecek/{kullaniciId}")
                   ?? new List<CalisanRandevuDto>();
        }
        public async Task<CalisanHizmetListeResponse> GetCalisanHizmetleriAsync(int kullaniciId)
        {
            return await _http.GetFromJsonAsync<CalisanHizmetListeResponse>(
                       $"calisan-hizmetler/{kullaniciId}")
                   ?? new CalisanHizmetListeResponse();
        }

        public async Task<List<BerberRandevuDto>> GetBerberGelecekRandevularAsync(int kullaniciId)
        {
            return await _http.GetFromJsonAsync<List<BerberRandevuDto>>(
                       $"berber-randevular/gelecek/{kullaniciId}")
                   ?? new List<BerberRandevuDto>();
        }

        public async Task<List<BerberRandevuDto>> GetBerberGecmisRandevularAsync(int kullaniciId)
        {
            return await _http.GetFromJsonAsync<List<BerberRandevuDto>>(
                       $"berber-randevular/gecmis/{kullaniciId}")
                   ?? new List<BerberRandevuDto>();
        }

        public async Task<CalisanHizmetListeResponse> GetBerberHizmetleriAsync(int kullaniciId)
        {
            return await _http.GetFromJsonAsync<CalisanHizmetListeResponse>(
                       $"berber-hizmetler/{kullaniciId}")
                   ?? new CalisanHizmetListeResponse();
        }
        public async Task<BerberCalisanListeResponse> GetBerberCalisanlariAsync(int kullaniciId)
        {
            return await _http.GetFromJsonAsync<BerberCalisanListeResponse>(
                       $"berber-calisanlar/{kullaniciId}")
                   ?? new BerberCalisanListeResponse();
        }

        public async Task<CalisanAdayDto?> CalisanAdayAraAsync(int kullaniciId)
        {
            try
            {
                return await _http.GetFromJsonAsync<CalisanAdayDto>(
                    $"calisan-islemleri/aday/{kullaniciId}");
            }
            catch
            {
                return null;
            }
        }

        public async Task<HizmetEkleResponse> HizmetEkleAsync(HizmetEkleRequest request)
        {
            var resp = await _http.PostAsJsonAsync("hizmetler", request);

            if (resp.IsSuccessStatusCode)
            {
                return await resp.Content.ReadFromJsonAsync<HizmetEkleResponse>()
                       ?? new HizmetEkleResponse
                       {
                           Success = false,
                           Message = "Sunucu boş cevap döndü."
                       };
            }

            var hata = await resp.Content.ReadAsStringAsync();
            return new HizmetEkleResponse
            {
                Success = false,
                Message = string.IsNullOrWhiteSpace(hata) ? "İşlem başarısız." : hata
            };
        }

        public async Task<KullaniciMiniProfilDto> KullaniciMiniProfilGetirAsync(int kullaniciId)
        {
            return await _http.GetFromJsonAsync<KullaniciMiniProfilDto>(
                       $"kullanicilar/{kullaniciId}/mini-profil")
                   ?? new KullaniciMiniProfilDto();
        }

        public async Task<TelefonGuncelleResponse> TelefonGuncelleAsync(int kullaniciId, string telefon)
        {
            var resp = await _http.PostAsJsonAsync("kullanicilar/telefon-guncelle",
                new TelefonGuncelleRequest
                {
                    KullaniciId = kullaniciId,
                    Telefon = telefon
                });

            if (resp.IsSuccessStatusCode)
            {
                return await resp.Content.ReadFromJsonAsync<TelefonGuncelleResponse>()
                       ?? new TelefonGuncelleResponse
                       {
                           Success = false,
                           Message = "Sunucu boş cevap döndü."
                       };
            }

            var hata = await resp.Content.ReadAsStringAsync();
            return new TelefonGuncelleResponse
            {
                Success = false,
                Message = string.IsNullOrWhiteSpace(hata) ? "İşlem başarısız." : hata
            };
        }

        public async Task<BerberAnaPanelDto> GetBerberAnaPanelAsync(int kullaniciId)
        {
            return await _http.GetFromJsonAsync<BerberAnaPanelDto>(
                       $"berber-panel/{kullaniciId}")
                   ?? new BerberAnaPanelDto();
        }

        public async Task<CalisanEkleResponse> CalisanEkleAsync(int berberId, int kullaniciId)
        {
            var resp = await _http.PostAsJsonAsync("calisan-islemleri/ekle",
                new CalisanEkleRequest
                {
                    BerberID = berberId,
                    KullaniciID = kullaniciId
                });

            if (resp.IsSuccessStatusCode)
            {
                return await resp.Content.ReadFromJsonAsync<CalisanEkleResponse>()
                       ?? new CalisanEkleResponse
                       {
                           Success = false,
                           Message = "Sunucu boş cevap döndü."
                       };
            }

            var hata = await resp.Content.ReadAsStringAsync();
            return new CalisanEkleResponse
            {
                Success = false,
                Message = string.IsNullOrWhiteSpace(hata) ? "İşlem başarısız." : hata
            };
        }

        public async Task<CalisanSilResponse> BerberCalisaniSilAsync(int berberId, int kullaniciId)
        {
            var resp = await _http.PostAsJsonAsync("berber-calisanlar/sil",
                new CalisanSilRequest
                {
                    BerberID = berberId,
                    KullaniciID = kullaniciId
                });

            if (resp.IsSuccessStatusCode)
            {
                return await resp.Content.ReadFromJsonAsync<CalisanSilResponse>()
                       ?? new CalisanSilResponse
                       {
                           Success = false,
                           Message = "Sunucu boş cevap döndü."
                       };
            }

            var hata = await resp.Content.ReadAsStringAsync();
            return new CalisanSilResponse
            {
                Success = false,
                Message = string.IsNullOrWhiteSpace(hata) ? "İşlem başarısız." : hata
            };
        }

        public async Task<HizmetAktifPasifResponse> BerberHizmetAktifPasifAsync(int hizmetId)
        {
            var resp = await _http.PostAsJsonAsync("berber-hizmetler/toggle-aktif",
                new HizmetAktifPasifRequest
                {
                    HizmetID = hizmetId
                });

            if (resp.IsSuccessStatusCode)
            {
                return await resp.Content.ReadFromJsonAsync<HizmetAktifPasifResponse>()
                       ?? new HizmetAktifPasifResponse
                       {
                           Success = false,
                           Message = "Sunucu boş cevap döndü."
                       };
            }

            var hata = await resp.Content.ReadAsStringAsync();
            return new HizmetAktifPasifResponse
            {
                Success = false,
                Message = string.IsNullOrWhiteSpace(hata) ? "İşlem başarısız." : hata
            };
        }
        public async Task<CalisanAnaSayfaDto> GetCalisanAnaSayfaAsync(int kullaniciId)
        {
            return await _http.GetFromJsonAsync<CalisanAnaSayfaDto>(
                       $"calisan-panel/{kullaniciId}")
                   ?? new CalisanAnaSayfaDto();
        }

        public async Task<HizmetAktifPasifResponse> HizmetAktifPasifAsync(int hizmetId)
        {
            var resp = await _http.PostAsJsonAsync("calisan-hizmetler/toggle-aktif",
                new HizmetAktifPasifRequest
                {
                    HizmetID = hizmetId
                });

            if (resp.IsSuccessStatusCode)
            {
                return await resp.Content.ReadFromJsonAsync<HizmetAktifPasifResponse>()
                       ?? new HizmetAktifPasifResponse
                       {
                           Success = false,
                           Message = "Sunucu boş cevap döndü."
                       };
            }

            var hata = await resp.Content.ReadAsStringAsync();
            return new HizmetAktifPasifResponse
            {
                Success = false,
                Message = string.IsNullOrWhiteSpace(hata) ? "İşlem başarısız." : hata
            };
        }

        public async Task<List<CalisanRandevuDto>> GetCalisanGecmisRandevularAsync(int kullaniciId)
        {
            return await _http.GetFromJsonAsync<List<CalisanRandevuDto>>(
                       $"calisan-randevular/gecmis/{kullaniciId}")
                   ?? new List<CalisanRandevuDto>();

        }
        // Basit string GET (ping için)
        public Task<string> GetStringAsync(string endpoint)
            => _http.GetStringAsync(endpoint);

        // Generic GET
        public Task<T?> GetAsync<T>(string endpoint)
            => _http.GetFromJsonAsync<T>(endpoint);

        // Generic POST (JSON) -> sadece success
        public async Task<bool> PostAsync<T>(string endpoint, T data)
        {
            var res = await _http.PostAsJsonAsync(endpoint, data);
            return res.IsSuccessStatusCode;
        }

        public async Task<bool> PutAsync<T>(string endpoint, T data)
        {
            var res = await _http.PutAsJsonAsync(endpoint, data);
            return res.IsSuccessStatusCode;
        }

        // JSON POST -> response döndür
        public async Task<TResponse?> PostJsonAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            var res = await _http.PostAsJsonAsync(endpoint, data);
            if (!res.IsSuccessStatusCode) return default;
            return await res.Content.ReadFromJsonAsync<TResponse>();
        }



        // Debug amaçlı: status + body
        public async Task<(bool ok, int status, string body)> PostJsonDebugAsync<TRequest>(string endpoint, TRequest data)
        {
            try
            {
                var res = await _http.PostAsJsonAsync(endpoint, data);
                var body = await res.Content.ReadAsStringAsync();
                return (res.IsSuccessStatusCode, (int)res.StatusCode, body);
            }
            catch (Exception ex)
            {
                return (false, -1, ex.Message);
            }
        }

        // =========================================================
        //  FOTO UPLOAD (multipart/form-data)
        // =========================================================

        /// <summary>
        /// Upload eder ve JSON response'u parse etmeye çalışır.
        /// Not: Başarısız HTTP (404/500) durumunda da body JSON olabilir.
        /// JSON değilse default döner.
        /// </summary>
        public async Task<TResponse?> UploadPhotoAsync<TResponse>(
            string endpoint,
            Stream fileStream,
            string fileName,
            string contentType = "application/octet-stream")
        {
            try
            {
                using var content = new MultipartFormDataContent();

                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

                // API tarafında parametre adı: IFormFile file => "file"
                content.Add(fileContent, "file", fileName);

                var res = await _http.PostAsync(endpoint, content);
                var body = await res.Content.ReadAsStringAsync();

                // JSON parse dene
                try
                {
                    return JsonSerializer.Deserialize<TResponse>(
                        body,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch
                {
                    return default;
                }
            }
            catch
            {
                return default;
            }
        }
        public async Task<(bool ok, int status, string body, TResponse? data)> PostJsonWithBodyAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            try
            {
                var res = await _http.PostAsJsonAsync(endpoint, data);
                var body = await res.Content.ReadAsStringAsync();

                TResponse? parsed = default;
                try
                {
                    parsed = System.Text.Json.JsonSerializer.Deserialize<TResponse>(
                        body,
                        new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                }
                catch
                {
                    parsed = default;
                }

                return (res.IsSuccessStatusCode, (int)res.StatusCode, body, parsed);
            }
            catch (Exception ex)
            {
                return (false, -1, ex.Message, default);
            }
        }

        /// <summary>
        /// Upload eder, status + body + (varsa) parse edilmiş data döner.
        /// "response okunamadı" sorununu gerçek sebebiyle görmen için.
        /// </summary>
        public async Task<(bool ok, int status, string body, TResponse? data)> UploadPhotoDebugAsync<TResponse>(
            string endpoint,
            Stream fileStream,
            string fileName,
            string contentType = "application/octet-stream")
        {
            try
            {
                using var content = new MultipartFormDataContent();

                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

                content.Add(fileContent, "file", fileName);

                var res = await _http.PostAsync(endpoint, content);
                var body = await res.Content.ReadAsStringAsync();

                TResponse? data = default;
                try
                {
                    data = JsonSerializer.Deserialize<TResponse>(
                        body,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch
                {
                    data = default;
                }

                return (res.IsSuccessStatusCode, (int)res.StatusCode, body, data);
            }
            catch (Exception ex)
            {
                return (false, -1, ex.Message, default);
            }
        }

        public async Task<KullaniciTipDto> KullaniciTipGetirAsync(int kullaniciId)
        {
            return await _http.GetFromJsonAsync<KullaniciTipDto>(
                       $"kullanicilar/{kullaniciId}/tip")
                   ?? new KullaniciTipDto();
        }

        public async Task<CalismaSaatleriGetResponse> GetBerberCalismaSaatleriAsync(int kullaniciId)
        {
            return await _http.GetFromJsonAsync<CalismaSaatleriGetResponse>(
                       $"calisma-saatleri/berber/{kullaniciId}")
                   ?? new CalismaSaatleriGetResponse();
        }

        public async Task<List<HizmetListeDto>> CalisanaGoreHizmetleriGetirAsync(int calisanId)
        {
            return await _http.GetFromJsonAsync<List<HizmetListeDto>>(
                       $"hizmetler/by-calisan/{calisanId}")
                   ?? new List<HizmetListeDto>();
        }

        public async Task<List<DoluAralikDto>> DoluAraliklariGetirAsync(int calisanId, DateOnly tarih)
        {
            string tarihText = tarih.ToString("yyyy-MM-dd");

            return await _http.GetFromJsonAsync<List<DoluAralikDto>>(
                       $"randevular/dolu-araliklar?calisanId={calisanId}&tarih={tarihText}")
                   ?? new List<DoluAralikDto>();
        }

        public async Task<RandevuCreateResponse> RandevuOlusturAsync(RandevuCreateRequest request)
        {
            var resp = await _http.PostAsJsonAsync("randevular", request);

            if (resp.IsSuccessStatusCode)
            {
                return await resp.Content.ReadFromJsonAsync<RandevuCreateResponse>()
                       ?? new RandevuCreateResponse
                       {
                           Success = false,
                           Message = "Sunucu boş cevap döndü."
                       };
            }

            if (resp.StatusCode == HttpStatusCode.Conflict)
            {
                var conflict = await resp.Content.ReadFromJsonAsync<RandevuCreateResponse>();
                return conflict ?? new RandevuCreateResponse
                {
                    Success = false,
                    Message = "Seçilen saat dolu."
                };
            }

            var hata = await resp.Content.ReadAsStringAsync();
            return new RandevuCreateResponse
            {
                Success = false,
                Message = string.IsNullOrWhiteSpace(hata) ? "Randevu oluşturulamadı." : hata
            };
        }

        public async Task<CalismaSaatleriKaydetResponse> SaveBerberCalismaSaatleriAsync(CalismaSaatleriKaydetRequest request)
        {
            var resp = await _http.PostAsJsonAsync("calisma-saatleri/berber", request);

            if (resp.IsSuccessStatusCode)
            {
                return await resp.Content.ReadFromJsonAsync<CalismaSaatleriKaydetResponse>()
                       ?? new CalismaSaatleriKaydetResponse
                       {
                           Success = false,
                           Message = "Sunucu boş cevap döndü."
                       };
            }

            var hata = await resp.Content.ReadAsStringAsync();
            return new CalismaSaatleriKaydetResponse
            {
                Success = false,
                Message = string.IsNullOrWhiteSpace(hata) ? "İşlem başarısız." : hata
            };
        }

        public async Task<SifreDegistirResponse> SifreDegistirAsync(int kullaniciId, string guncelSifre, string yeniSifre)
        {
            var resp = await _http.PostAsJsonAsync("kullanicilar/sifre-degistir",
                new SifreDegistirRequest
                {
                    KullaniciId = kullaniciId,
                    GuncelSifre = guncelSifre,
                    YeniSifre = yeniSifre
                });

            if (resp.IsSuccessStatusCode)
            {
                return await resp.Content.ReadFromJsonAsync<SifreDegistirResponse>()
                       ?? new SifreDegistirResponse
                       {
                           Success = false,
                           Message = "Sunucu boş cevap döndü."
                       };
            }

            var hata = await resp.Content.ReadAsStringAsync();
            return new SifreDegistirResponse
            {
                Success = false,
                Message = string.IsNullOrWhiteSpace(hata) ? "İşlem başarısız." : hata
            };
        }

        // Wrapper'lar (ister kullan, ister sayfada direkt UploadPhotoAsync çağır)
        public async Task<(bool ok, string? url, string? error)> UploadProfilFotoAsync(int kullaniciId, Stream fileStream, string fileName)
        {
            var (ok, status, body, data) =
                await UploadPhotoDebugAsync<PhotoUploadResponse>($"photos/profil/{kullaniciId}", fileStream, fileName);

            if (!ok) return (false, null, $"Status: {status} | Body: {body}");
            if (data == null) return (false, null, $"JSON okunamadı. Status: {status} | Body: {body}");
            if (!data.Basarili) return (false, null, data.Mesaj ?? "Yükleme başarısız.");

            return (true, data.Url, null);
        }

        public async Task<(bool ok, string? url, string? error)> UploadDukkanFotoAsync(int berberId, Stream fileStream, string fileName)
        {
            var (ok, status, body, data) =
                await UploadPhotoDebugAsync<PhotoUploadResponse>($"photos/dukkan/{berberId}", fileStream, fileName);

            if (!ok) return (false, null, $"Status: {status} | Body: {body}");
            if (data == null) return (false, null, $"JSON okunamadı. Status: {status} | Body: {body}");
            if (!data.Basarili) return (false, null, data.Mesaj ?? "Yükleme başarısız.");

            return (true, data.Url, null);
        }
    }
}