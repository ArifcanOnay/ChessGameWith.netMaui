using Microsoft.EntityFrameworkCore;
using SatrancApi.Entities.Models;
using SatrancAPI.Datas;
using SatrancAPI.Entities.HamleSınıfları;
using SatrancAPI.Entities.Models;

namespace SatrancAPI.Services
{
    public class OyunYoneticisi
    {
        private readonly SatrancDbContext _dbContext;
        private readonly TahtaYoneticisi _tahtaYoneticisi;
        private Guid _aktifOyunId;

        public OyunYoneticisi(SatrancDbContext dbContext, TahtaYoneticisi tahtaYoneticisi)
        {
            _dbContext = dbContext;
            _tahtaYoneticisi = tahtaYoneticisi;
        }

        // Yeni oyun oluşturur
        public async Task<Oyun> YeniOyunOlustur(Guid beyazOyuncuId, Guid siyahOyuncuId)
        {
            var beyazOyuncu = await _dbContext.Oyuncular.FindAsync(beyazOyuncuId);
            var siyahOyuncu = await _dbContext.Oyuncular.FindAsync(siyahOyuncuId);

            if (beyazOyuncu == null || siyahOyuncu == null)
                throw new Exception("Oyuncular bulunamadı");

            var oyun = new Oyun
            {
                OyunId = Guid.NewGuid(),
                BeyazOyuncuId = beyazOyuncuId,
                SiyahOyuncuId = siyahOyuncuId,
                BeyazOyuncu = beyazOyuncu,
                SiyahOyuncu = siyahOyuncu,
                BaslangicTarihi = DateTime.Now,
                Durum = Durum.Oynaniyor,
                BeyazSkor = 0,
                SiyahSkor = 0,
                SiraKimin = 0,
                ToplamHamleSayisi = 0,
                SahDurumu = false
            };

            _dbContext.Oyunlar.Add(oyun);
            BaslangicTaslariniOlustur(oyun);
            await _dbContext.SaveChangesAsync();
            return oyun;
        }

        private void BaslangicTaslariniOlustur(Oyun oyun)
        {
            // BEYAZ PİYONLAR
            for (int y = 0; y < 8; y++)
            {
                _dbContext.Taslar.Add(new Tas
                {
                    TasId = Guid.NewGuid(),
                    OyunId = oyun.OyunId,
                    OyuncuId = oyun.BeyazOyuncuId,
                    renk = Renk.Beyaz,
                    turu = TasTuru.Piyon,
                    X = 6,
                    Y = y,
                    AktifMi = true,
                    HicHareketEtmediMi = true,
                    TasSimgesi = "♙"
                });
            }

            // BEYAZ DİĞER TAŞLAR
            var beyazTaslar = new[]
            {
                new { Tur = TasTuru.Kale, Sembol = "♖", Y = 0 },
                new { Tur = TasTuru.At, Sembol = "♘", Y = 1 },
                new { Tur = TasTuru.Fil, Sembol = "♗", Y = 2 },
                new { Tur = TasTuru.Vezir, Sembol = "♕", Y = 3 },
                new { Tur = TasTuru.Şah, Sembol = "♔", Y = 4 },
                new { Tur = TasTuru.Fil, Sembol = "♗", Y = 5 },
                new { Tur = TasTuru.At, Sembol = "♘", Y = 6 },
                new { Tur = TasTuru.Kale, Sembol = "♖", Y = 7 }
            };

            foreach (var tasInfo in beyazTaslar)
            {
                _dbContext.Taslar.Add(new Tas
                {
                    TasId = Guid.NewGuid(),
                    OyunId = oyun.OyunId,
                    OyuncuId = oyun.BeyazOyuncuId,
                    renk = Renk.Beyaz,
                    turu = tasInfo.Tur,
                    X = 7,
                    Y = tasInfo.Y,
                    AktifMi = true,
                    HicHareketEtmediMi = true,
                    TasSimgesi = tasInfo.Sembol
                });
            }

            // SİYAH PİYONLAR
            for (int y = 0; y < 8; y++)
            {
                _dbContext.Taslar.Add(new Tas
                {
                    TasId = Guid.NewGuid(),
                    OyunId = oyun.OyunId,
                    OyuncuId = oyun.SiyahOyuncuId,
                    renk = Renk.Siyah,
                    turu = TasTuru.Piyon,
                    X = 1,
                    Y = y,
                    AktifMi = true,
                    HicHareketEtmediMi = true,
                    TasSimgesi = "♟"
                });
            }

            // SİYAH DİĞER TAŞLAR
            var siyahTaslar = new[]
            {
                new { Tur = TasTuru.Kale, Sembol = "♜", Y = 0 },
                new { Tur = TasTuru.At, Sembol = "♞", Y = 1 },
                new { Tur = TasTuru.Fil, Sembol = "♝", Y = 2 },
                new { Tur = TasTuru.Vezir, Sembol = "♛", Y = 3 },
                new { Tur = TasTuru.Şah, Sembol = "♚", Y = 4 },
                new { Tur = TasTuru.Fil, Sembol = "♝", Y = 5 },
                new { Tur = TasTuru.At, Sembol = "♞", Y = 6 },
                new { Tur = TasTuru.Kale, Sembol = "♜", Y = 7 }
            };

            foreach (var tasInfo in siyahTaslar)
            {
                _dbContext.Taslar.Add(new Tas
                {
                    TasId = Guid.NewGuid(),
                    OyunId = oyun.OyunId,
                    OyuncuId = oyun.SiyahOyuncuId,
                    renk = Renk.Siyah,
                    turu = tasInfo.Tur,
                    X = 0,
                    Y = tasInfo.Y,
                    AktifMi = true,
                    HicHareketEtmediMi = true,
                    TasSimgesi = tasInfo.Sembol
                });
            }
        }

        public async Task TahtayiYukle(Guid oyunId)
        {
            _aktifOyunId = oyunId;
            var taslar = await _dbContext.Taslar
                .Where(t => t.OyunId == oyunId && t.AktifMi)
                .ToListAsync();

            _tahtaYoneticisi.TahtayiOlustur(taslar);
        }

        public async Task<Tas[,]> TahtayiGetir(Guid oyunId)
        {
            await TahtayiYukle(oyunId);
            return _tahtaYoneticisi.TahtayiGetir();
        }

        public bool SahTehditAltindaMi(Renk sahRengi)
        {
            try
            {
                var sah = _dbContext.Taslar.FirstOrDefault(t =>
                    t.OyunId == _aktifOyunId &&
                    t.AktifMi &&
                    t.renk == sahRengi &&
                    t.turu == TasTuru.Şah);

                if (sah == null)
                    return false;

                var dusmanTaslari = _dbContext.Taslar
                    .Where(t => t.OyunId == _aktifOyunId && t.AktifMi && t.renk != sahRengi)
                    .ToList();

                foreach (var dusmanTas in dusmanTaslari)
                {
                    var gecerliHamleler = GecerliHamleleriGetir(dusmanTas);

                    if (gecerliHamleler.Any(h => h.X == sah.X && h.Y == sah.Y))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Şah tehdit kontrolü hatası: {ex.Message}");
                return false;
            }
        }

        public bool SahMatMi(Renk renk)
        {
            try
            {
                if (!SahTehditAltindaMi(renk))
                    return false;

                var taslar = _dbContext.Taslar
                    .Where(t => t.OyunId == _aktifOyunId && t.AktifMi && t.renk == renk)
                    .ToList();

                foreach (var tas in taslar)
                {
                    var gecerliHamleler = GecerliHamleleriGetir(tas);

                    foreach (var hamle in gecerliHamleler)
                    {
                        if (HamleIleŞahKurtulurMu(tas, hamle.X, hamle.Y, renk))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Şah-mat kontrolü hatası: {ex.Message}");
                return false;
            }
        }

        private bool HamleIleŞahKurtulurMu(Tas tas, int hedefX, int hedefY, Renk sahRengi)
        {
            try
            {
                int eskiX = tas.X;
                int eskiY = tas.Y;

                var hedefTas = _dbContext.Taslar
                    .FirstOrDefault(t => t.X == hedefX && t.Y == hedefY && t.AktifMi && t.OyunId == _aktifOyunId);

                bool hedefTasAktifDurumu = hedefTas?.AktifMi ?? false;

                tas.X = hedefX;
                tas.Y = hedefY;
                if (hedefTas != null)
                    hedefTas.AktifMi = false;

                TahtayiYukle(_aktifOyunId).Wait();
                bool sahHalaTehditAltinda = SahTehditAltindaMi(sahRengi);

                tas.X = eskiX;
                tas.Y = eskiY;
                if (hedefTas != null)
                    hedefTas.AktifMi = hedefTasAktifDurumu;

                TahtayiYukle(_aktifOyunId).Wait();

                return !sahHalaTehditAltinda;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hamle simülasyonu hatası: {ex.Message}");
                return false;
            }
        }

        private List<(int X, int Y)> GecerliHamleleriGetir(Tas tas)
        {
            try
            {
                var tahtaHamleler = _tahtaYoneticisi.GecerliHamleleriGetir(tas);
                return tahtaHamleler.Select(h => (h.x, h.y)).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Geçerli hamleler alma hatası: {ex.Message}");
                return new List<(int X, int Y)>();
            }
        }

        // ✅ GELİŞTİRİLMİŞ: ROK DESTEKLİ HAMLE YAPMA
        public async Task<bool> HamleYap(Guid oyunId, Guid tasId, int hedefX, int hedefY)
        {
            var oyun = await _dbContext.Oyunlar.FindAsync(oyunId);
            if (oyun == null || oyun.Durum != Durum.Oynaniyor)
                return false;

            await TahtayiYukle(oyunId);

            var tas = await _dbContext.Taslar
                .FirstOrDefaultAsync(t => t.TasId == tasId && t.AktifMi);

            if (tas == null)
                return false;

            // ✅ ROK HAMLESİ KONTROLÜ
            if (tas.turu == TasTuru.Şah && Math.Abs(hedefY - tas.Y) == 2)
            {
                Console.WriteLine("Rok hamlesi algılandı!");
                return await RokHamlesiYap(tas, hedefX, hedefY);
            }

            // TEK KİŞİLİK OYUN KONTROLÜ
            bool tekKislikOyun = oyun.BeyazOyuncuId == oyun.SiyahOyuncuId;

            if (!tekKislikOyun)
            {
                var sonHamle = await _dbContext.Hamleler
                    .Where(h => h.OyunId == oyunId)
                    .OrderByDescending(h => h.HamleTarihi)
                    .FirstOrDefaultAsync();

                bool beyazinSirasi = sonHamle == null || sonHamle.OyuncuId == oyun.SiyahOyuncuId;

                if ((beyazinSirasi && tas.renk != Renk.Beyaz) || (!beyazinSirasi && tas.renk != Renk.Siyah))
                    return false;
            }

            var hedefTas = await _dbContext.Taslar
                .FirstOrDefaultAsync(t => t.OyunId == oyunId && t.X == hedefX && t.Y == hedefY && t.AktifMi);

            if (!_tahtaYoneticisi.HamleYap(tas, hedefX, hedefY))
                return false;

            int eskiX = tas.X;
            int eskiY = tas.Y;
            tas.X = hedefX;
            tas.Y = hedefY;
            tas.HicHareketEtmediMi = false;
            tas.SonHareketTarihi = DateTime.Now;

            if (hedefTas != null)
            {
                hedefTas.AktifMi = false;
            }

            oyun.ToplamHamleSayisi++;

            var hamle = new Hamle
            {
                HamleId = Guid.NewGuid(),
                OyunId = oyunId,
                OyuncuId = tas.OyuncuId,
                TasId = tas.TasId,
                turu = tas.turu,
                BaslangicX = eskiX,
                BaslangicY = eskiY,
                HedefX = hedefX,
                HedefY = hedefY,
                HamleTarihi = DateTime.Now
            };

            _dbContext.Hamleler.Add(hamle);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<List<Koordinat>> GecerliHamleleriGetir(Guid oyunId, Guid tasId)
        {
            await TahtayiYukle(oyunId);
            var tas = await _dbContext.Taslar.FirstOrDefaultAsync(t => t.TasId == tasId && t.AktifMi);
            if (tas == null)
                return new List<Koordinat>();

            var hamleler = _tahtaYoneticisi.GecerliHamleleriGetir(tas);
            return hamleler.Select(h => new Koordinat(h.x, h.y)).ToList();
        }

        public async Task<bool> OyunuBitir(Guid oyunId, string bitisNedeni, string kazanan = null)
        {
            var oyun = await _dbContext.Oyunlar.FindAsync(oyunId);
            if (oyun == null)
                return false;

            oyun.Durum = Durum.Bitiyor;
            oyun.BitisTarihi = DateTime.Now;
            oyun.BitisNedeni = bitisNedeni;

            if (!string.IsNullOrEmpty(kazanan))
            {
                oyun.KazananOyuncu = kazanan;
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }

        // ✅ GELİŞTİRİLMİŞ ROK HAMLESİ YAPMA
        private async Task<bool> RokHamlesiYap(Tas sah, int hedefX, int hedefY)
        {
            try
            {
                // ✅ GELİŞTİRİLMİŞ ROK KONTROLÜ
                bool kisaRok = hedefY > sah.Y;

                // ✅ YENİ: Şah tehdit altında mı kontrol et
                if (SahTehditAltindaMi(sah.renk))
                {
                    Console.WriteLine("Rok reddedildi: Şah tehdit altında");
                    return false;
                }

                // ✅ YENİ: Rok yolundaki kareler tehdit altında mı?
                if (kisaRok)
                {
                    if (KareTehditAltindaMi(sah.X, sah.Y + 1, sah.renk) ||
                        KareTehditAltindaMi(sah.X, sah.Y + 2, sah.renk))
                    {
                        Console.WriteLine("Kısa rok reddedildi: Geçiş yolu tehdit altında");
                        return false;
                    }
                }
                else
                {
                    if (KareTehditAltindaMi(sah.X, sah.Y - 1, sah.renk) ||
                        KareTehditAltindaMi(sah.X, sah.Y - 2, sah.renk))
                    {
                        Console.WriteLine("Uzun rok reddedildi: Geçiş yolu tehdit altında");
                        return false;
                    }
                }

                int eskiSahY = sah.Y;
                sah.Y = hedefY;
                sah.HicHareketEtmediMi = false;
                sah.SonHareketTarihi = DateTime.Now;

                if (kisaRok)
                {
                    var kale = await _dbContext.Taslar
                        .FirstOrDefaultAsync(t => t.X == sah.X && t.Y == eskiSahY + 3 && t.AktifMi && t.OyunId == _aktifOyunId);

                    if (kale != null)
                    {
                        kale.Y = eskiSahY + 1;
                        kale.HicHareketEtmediMi = false;
                        kale.SonHareketTarihi = DateTime.Now;
                        Console.WriteLine($"Kısa rok: Kale {eskiSahY + 3} → {eskiSahY + 1}");
                    }
                }
                else
                {
                    var kale = await _dbContext.Taslar
                        .FirstOrDefaultAsync(t => t.X == sah.X && t.Y == eskiSahY - 4 && t.AktifMi && t.OyunId == _aktifOyunId);

                    if (kale != null)
                    {
                        kale.Y = eskiSahY - 1;
                        kale.HicHareketEtmediMi = false;
                        kale.SonHareketTarihi = DateTime.Now;
                        Console.WriteLine($"Uzun rok: Kale {eskiSahY - 4} → {eskiSahY - 1}");
                    }
                }

                var hamle = new Hamle
                {
                    HamleId = Guid.NewGuid(),
                    OyunId = sah.OyunId,
                    OyuncuId = sah.OyuncuId,
                    TasId = sah.TasId,
                    BaslangicX = sah.X,
                    BaslangicY = eskiSahY,
                    HedefX = hedefX,
                    HedefY = hedefY,
                    HamleTarihi = DateTime.Now,
                    Notasyon = kisaRok ? "O-O" : "O-O-O",
                    RokMu = true
                };

                _dbContext.Hamleler.Add(hamle);
                await _dbContext.SaveChangesAsync();

                Console.WriteLine($"Rok başarılı: {(kisaRok ? "Kısa" : "Uzun")} rok tamamlandı");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Rok hamlesi hatası: {ex.Message}");
                return false;
            }
        }

        // ✅ YENİ: Kare tehdit kontrolü metodu
        private bool KareTehditAltindaMi(int x, int y, Renk sahRengi)
        {
            try
            {
                var dusmanTaslari = _dbContext.Taslar
                    .Where(t => t.OyunId == _aktifOyunId && t.AktifMi && t.renk != sahRengi)
                    .ToList();

                foreach (var dusmanTas in dusmanTaslari)
                {
                    var gecerliHamleler = GecerliHamleleriGetir(dusmanTas);
                    if (gecerliHamleler.Any(h => h.X == x && h.Y == y))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kare tehdit kontrolü hatası: {ex.Message}");
                return true;
            }
        }

        public List<(int X, int Y)> SahiKurtaranHamleler(Guid tasId, Renk sahRengi)
        {
            try
            {
                if (!SahTehditAltindaMi(sahRengi))
                {
                    var tas = _dbContext.Taslar.FirstOrDefault(t => t.TasId == tasId && t.AktifMi);
                    return tas != null ? GecerliHamleleriGetir(tas) : new List<(int X, int Y)>();
                }

                var kurtaranHamleler = new List<(int X, int Y)>();
                var secilenTas = _dbContext.Taslar.FirstOrDefault(t => t.TasId == tasId && t.AktifMi);

                if (secilenTas == null) return kurtaranHamleler;

                var tumHamleler = GecerliHamleleriGetir(secilenTas);

                foreach (var hamle in tumHamleler)
                {
                    if (HamleIleŞahKurtulurMu(secilenTas, hamle.X, hamle.Y, sahRengi))
                    {
                        kurtaranHamleler.Add(hamle);
                    }
                }

                return kurtaranHamleler;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Şah kurtaran hamleler hatası: {ex.Message}");
                return new List<(int X, int Y)>();
            }
        }
    }
}