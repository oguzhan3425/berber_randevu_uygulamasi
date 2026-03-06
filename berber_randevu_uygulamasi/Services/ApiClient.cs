using berber_randevu_uygulamasi.Models.Dtos;
using System;
using System.IO;
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