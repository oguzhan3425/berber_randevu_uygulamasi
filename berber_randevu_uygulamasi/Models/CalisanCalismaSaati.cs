namespace berber_randevu_uygulamasi.Models
{
    public class CalisanCalismaSaati
    {
        public int ID { get; set; }

        public int CalisanID { get; set; }

        // 1 = Pazartesi ... 7 = Pazar
        public short Gun { get; set; }

        public bool AcikMi { get; set; }

        public TimeSpan? Acilis { get; set; }

        public TimeSpan? Kapanis { get; set; }
    }
}
