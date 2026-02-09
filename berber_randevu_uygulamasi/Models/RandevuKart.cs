using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace berber_randevu_uygulamasi.Models
{
    public class RandevuKart
    {
        // Randevu ID (butonlarda lazım olur)
        public int RandevuID { get; set; }

        // Saat & Tarih (ekranda düzgün gözüksün diye string)
        public string SaatText { get; set; } = "";     // "14:30"
        public string TarihText { get; set; } = "";    // "01.02.2026"

        // Müşteri bilgileri
        public string MusteriAdSoyad { get; set; } = "";
        public string MusteriFoto { get; set; } = "default_berber.png";

        // Hizmet bilgileri
        public string HizmetAdi { get; set; } = "";
        public decimal ToplamUcret { get; set; }

        // Geçmiş randevular için
        public string DurumText { get; set; } = "";
    }
}
