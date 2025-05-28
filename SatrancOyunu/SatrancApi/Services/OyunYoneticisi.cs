using Microsoft.EntityFrameworkCore;
using SatrancAPI.Datas;
using SatrancAPI.Entities.HamleSınıfları;
using SatrancAPI.Entities.Models;

namespace SatrancAPI.Services
{
    public class OyunYoneticisi
    {
        private readonly SatrancDbContext _dbContext;
        private readonly TahtaYoneticisi _tahtaYoneticisi;

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
                SiraKimin = 0, // Beyaz başlar
                ToplamHamleSayisi = 0,
                SahDurumu = false
            };

            _dbContext.Oyunlar.Add(oyun);

            // Başlangıç taşlarını oluştur
            BaslangicTaslariniOlustur(oyun);

            await _dbContext.SaveChangesAsync();
            return oyun;
        }

        // Başlangıç taşlarını oluşturur
        private void BaslangicTaslariniOlustur(Oyun oyun)
        {
            // Beyaz piyonlar
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
                    HicHareketEtmediMi = true
                });
            }

            // Beyaz diğer taşlar
            _dbContext.Taslar.Add(new Tas { TasId = Guid.NewGuid(), OyunId = oyun.OyunId, OyuncuId = oyun.BeyazOyuncuId, renk = Renk.Beyaz, turu = TasTuru.Kale, X = 7, Y = 0, AktifMi = true, HicHareketEtmediMi = true });
            _dbContext.Taslar.Add(new Tas { TasId = Guid.NewGuid(), OyunId = oyun.OyunId, OyuncuId = oyun.BeyazOyuncuId, renk = Renk.Beyaz, turu = TasTuru.At, X = 7, Y = 1, AktifMi = true, HicHareketEtmediMi = true });
            _dbContext.Taslar.Add(new Tas { TasId = Guid.NewGuid(), OyunId = oyun.OyunId, OyuncuId = oyun.BeyazOyuncuId, renk = Renk.Beyaz, turu = TasTuru.Fil, X = 7, Y = 2, AktifMi = true, HicHareketEtmediMi = true });
            _dbContext.Taslar.Add(new Tas { TasId = Guid.NewGuid(), OyunId = oyun.OyunId, OyuncuId = oyun.BeyazOyuncuId, renk = Renk.Beyaz, turu = TasTuru.Vezir, X = 7, Y = 3, AktifMi = true, HicHareketEtmediMi = true });
            _dbContext.Taslar.Add(new Tas { TasId = Guid.NewGuid(), OyunId = oyun.OyunId, OyuncuId = oyun.BeyazOyuncuId, renk = Renk.Beyaz, turu = TasTuru.Şah, X = 7, Y = 4, AktifMi = true, HicHareketEtmediMi = true });
            _dbContext.Taslar.Add(new Tas { TasId = Guid.NewGuid(), OyunId = oyun.OyunId, OyuncuId = oyun.BeyazOyuncuId, renk = Renk.Beyaz, turu = TasTuru.Fil, X = 7, Y = 5, AktifMi = true, HicHareketEtmediMi = true });
            _dbContext.Taslar.Add(new Tas { TasId = Guid.NewGuid(), OyunId = oyun.OyunId, OyuncuId = oyun.BeyazOyuncuId, renk = Renk.Beyaz, turu = TasTuru.At, X = 7, Y = 6, AktifMi = true, HicHareketEtmediMi = true });
            _dbContext.Taslar.Add(new Tas { TasId = Guid.NewGuid(), OyunId = oyun.OyunId, OyuncuId = oyun.BeyazOyuncuId, renk = Renk.Beyaz, turu = TasTuru.Kale, X = 7, Y = 7, AktifMi = true, HicHareketEtmediMi = true });

            // Siyah piyonlar
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
                    HicHareketEtmediMi = true
                });
            }

            // Siyah diğer taşlar
            _dbContext.Taslar.Add(new Tas { TasId = Guid.NewGuid(), OyunId = oyun.OyunId, OyuncuId = oyun.SiyahOyuncuId, renk = Renk.Siyah, turu = TasTuru.Kale, X = 0, Y = 0, AktifMi = true, HicHareketEtmediMi = true });
            _dbContext.Taslar.Add(new Tas { TasId = Guid.NewGuid(), OyunId = oyun.OyunId, OyuncuId = oyun.SiyahOyuncuId, renk = Renk.Siyah, turu = TasTuru.At, X = 0, Y = 1, AktifMi = true, HicHareketEtmediMi = true });
            _dbContext.Taslar.Add(new Tas { TasId = Guid.NewGuid(), OyunId = oyun.OyunId, OyuncuId = oyun.SiyahOyuncuId, renk = Renk.Siyah, turu = TasTuru.Fil, X = 0, Y = 2, AktifMi = true, HicHareketEtmediMi = true });
            _dbContext.Taslar.Add(new Tas { TasId = Guid.NewGuid(), OyunId = oyun.OyunId, OyuncuId = oyun.SiyahOyuncuId, renk = Renk.Siyah, turu = TasTuru.Vezir, X = 0, Y = 3, AktifMi = true, HicHareketEtmediMi = true });
            _dbContext.Taslar.Add(new Tas { TasId = Guid.NewGuid(), OyunId = oyun.OyunId, OyuncuId = oyun.SiyahOyuncuId, renk = Renk.Siyah, turu = TasTuru.Şah, X = 0, Y = 4, AktifMi = true, HicHareketEtmediMi = true });
            _dbContext.Taslar.Add(new Tas { TasId = Guid.NewGuid(), OyunId = oyun.OyunId, OyuncuId = oyun.SiyahOyuncuId, renk = Renk.Siyah, turu = TasTuru.Fil, X = 0, Y = 5, AktifMi = true, HicHareketEtmediMi = true });
            _dbContext.Taslar.Add(new Tas { TasId = Guid.NewGuid(), OyunId = oyun.OyunId, OyuncuId = oyun.SiyahOyuncuId, renk = Renk.Siyah, turu = TasTuru.At, X = 0, Y = 6, AktifMi = true, HicHareketEtmediMi = true });
            _dbContext.Taslar.Add(new Tas { TasId = Guid.NewGuid(), OyunId = oyun.OyunId, OyuncuId = oyun.SiyahOyuncuId, renk = Renk.Siyah, turu = TasTuru.Kale, X = 0, Y = 7, AktifMi = true, HicHareketEtmediMi = true });
        }

        // Bir oyunun tahta durumunu veritabanından alır ve tahta yöneticisine yükler
        public async Task TahtayiYukle(Guid oyunId)
        {
            var taslar = await _dbContext.Taslar
                .Where(t => t.OyunId == oyunId && t.AktifMi)
                .ToListAsync();

            _tahtaYoneticisi.TahtayiOlustur(taslar);
        }

        // ✅ EKSİK METOT - Tahta döndürür
        public async Task<Tas[,]> TahtayiGetir(Guid oyunId)
        {
            await TahtayiYukle(oyunId);
            return _tahtaYoneticisi.TahtayiGetir();
        }

        // ✅ EKSİK METOT - Şah tehdit durumunu kontrol et
        public bool SahTehditAltindaMi(Renk sahRengi)
        {
            return _tahtaYoneticisi.SahTehditAltindaMi(sahRengi);
        }

        // ✅ EKSİK METOT - Şah mat durumunu kontrol et
        public bool SahMatMi(Renk sahRengi)
        {
            return _tahtaYoneticisi.SahMatMi(sahRengi);
        }

        // ✅ EKSİK METOT - Oyun durumunu getir
        public async Task<object> OyunDurumunuGetir(Guid oyunId)
        {
            var oyun = await _dbContext.Oyunlar.FindAsync(oyunId);
            if (oyun == null)
                return null;

            await TahtayiYukle(oyunId);

            // Son hamleyi al
            var sonHamle = await _dbContext.Hamleler
                .Where(h => h.OyunId == oyunId)
                .OrderByDescending(h => h.HamleTarihi)
                .FirstOrDefaultAsync();

            // Sıradaki oyuncuyu belirle
            bool beyazSirasi = sonHamle == null || sonHamle.OyuncuId == oyun.SiyahOyuncuId;
            Renk siradakiRenk = beyazSirasi ? Renk.Beyaz : Renk.Siyah;

            // Şah durumlarını kontrol et
            bool beyazSahTehdit = SahTehditAltindaMi(Renk.Beyaz);
            bool siyahSahTehdit = SahTehditAltindaMi(Renk.Siyah);
            bool beyazSahMat = beyazSahTehdit && SahMatMi(Renk.Beyaz);
            bool siyahSahMat = siyahSahTehdit && SahMatMi(Renk.Siyah);

            return new
            {
                Durum = oyun.Durum,
                SiradakiOyuncuRengi = siradakiRenk,
                BeyazSahTehditAltinda = beyazSahTehdit,
                SiyahSahTehditAltinda = siyahSahTehdit,
                BeyazSahMat = beyazSahMat,
                SiyahSahMat = siyahSahMat,
                OyunBittiMi = oyun.Durum != Durum.Oynaniyor
            };
        }

        // ✅ EKSİK METOT - Piyon terfi
        public async Task<bool> PiyonTerfiEt(Guid oyunId, Guid piyonId, TasTuru yeniTasTuru)
        {
            var piyon = await _dbContext.Taslar.FirstOrDefaultAsync(t => t.TasId == piyonId && t.AktifMi);
            if (piyon == null || piyon.turu != TasTuru.Piyon)
                return false;

            // Piyonun terfi hattında olduğunu kontrol et
            bool terfiPozisyonundaMi = (piyon.renk == Renk.Beyaz && piyon.X == 0) ||
                                     (piyon.renk == Renk.Siyah && piyon.X == 7);
            if (!terfiPozisyonundaMi)
                return false;

            // Piyonu istenen türe yükselt
            piyon.turu = yeniTasTuru;
            piyon.SonHareketTarihi = DateTime.Now;

            await _dbContext.SaveChangesAsync();
            return true;
        }

        // Hamle yapar
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

            // Sıra kontrolü - son hamleye bakarak
            var sonHamle = await _dbContext.Hamleler
                .Where(h => h.OyunId == oyunId)
                .OrderByDescending(h => h.HamleTarihi)
                .FirstOrDefaultAsync();

            bool beyazinSirasi = sonHamle == null || sonHamle.OyuncuId == oyun.SiyahOyuncuId;

            if ((beyazinSirasi && tas.renk != Renk.Beyaz) || (!beyazinSirasi && tas.renk != Renk.Siyah))
                return false;

            // Hedef konumdaki taş (varsa)
            var hedefTas = await _dbContext.Taslar
                .FirstOrDefaultAsync(t => t.OyunId == oyunId && t.X == hedefX && t.Y == hedefY && t.AktifMi);

            // Hamle yap
            if (!_tahtaYoneticisi.HamleYap(tas, hedefX, hedefY))
                return false;

            // Taşın konumunu güncelle
            int eskiX = tas.X;
            int eskiY = tas.Y;
            tas.X = hedefX;
            tas.Y = hedefY;
            tas.HicHareketEtmediMi = false;
            tas.SonHareketTarihi = DateTime.Now;

            // Hedef konumdaki taş varsa ele al
            if (hedefTas != null)
            {
                hedefTas.AktifMi = false;
            }

            // Hamle sayısını artır
            oyun.ToplamHamleSayisi++;

            // Hamle kaydet
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

            // Hamle sonrası kontroller
            Renk rakipRenk = tas.renk == Renk.Beyaz ? Renk.Siyah : Renk.Beyaz;
            bool sahTehditAltinda = _tahtaYoneticisi.SahTehditAltindaMi(rakipRenk);
            bool sahMat = _tahtaYoneticisi.SahMatMi(rakipRenk);

            // Oyun durumunu güncelle
            oyun.SahDurumu = sahTehditAltinda;

            // Şah mat durumunu kontrol et
            if (sahMat)
            {
                // Oyunu bitir ve kazananı belirle
                oyun.Durum = Durum.Bitiyor;
                oyun.BitisTarihi = DateTime.Now;
                oyun.KazananOyuncu = tas.renk == Renk.Beyaz ? "Beyaz" : "Siyah";
                oyun.BitisNedeni = "Şah Mat";

                // Kazanan oyuncunun skorunu artır
                if (tas.renk == Renk.Beyaz)
                {
                    oyun.BeyazSkor += 1;
                }
                else
                {
                    oyun.SiyahSkor += 1;
                }
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }

        // Geçerli hamleleri getir
        public async Task<List<(int X, int Y)>> GecerliHamleleriGetir(Guid oyunId, Guid tasId)
        {
            // Tahtayı yükle
            await TahtayiYukle(oyunId);
            var tas = await _dbContext.Taslar.FirstOrDefaultAsync(t => t.TasId == tasId && t.AktifMi);
            if (tas == null)
                return new List<(int, int)>();

            // HamleFactory ile taşın geçerli hamlelerini bul
            var hamleSinifi = HamleFactory.HamleSinifiGetir(tas.turu);
            var tahta = _tahtaYoneticisi.TahtayiGetir();
            var hamleler = hamleSinifi.getGecerliHamleler(tas, tahta);
            return hamleler;
        }

        // ✅ EKSİK METOT - Oyunu bitir
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

        // ✅ EKSİK METOT - Oyun istatistiklerini getir
        public async Task<object> OyunIstatistikleriniGetir(Guid oyunId)
        {
            var oyun = await _dbContext.Oyunlar
                .Include(o => o.BeyazOyuncu)
                .Include(o => o.SiyahOyuncu)
                .Include(o => o.Hamleler)
                .FirstOrDefaultAsync(o => o.OyunId == oyunId);

            if (oyun == null)
                return null;

            var aktifTaslar = await _dbContext.Taslar
                .Where(t => t.OyunId == oyunId && t.AktifMi)
                .ToListAsync();

            return new
            {
                OyunId = oyun.OyunId,
                Durum = oyun.Durum,
                BaslangicTarihi = oyun.BaslangicTarihi,
                BitisTarihi = oyun.BitisTarihi,
                ToplamHamleSayisi = oyun.ToplamHamleSayisi,
                BeyazOyuncu = oyun.BeyazOyuncu?.isim,
                SiyahOyuncu = oyun.SiyahOyuncu?.isim,
                BeyazSkor = oyun.BeyazSkor,
                SiyahSkor = oyun.SiyahSkor,
                KazananOyuncu = oyun.KazananOyuncu,
                BitisNedeni = oyun.BitisNedeni,
                AktifTasSayisi = aktifTaslar.Count,
                BeyazTasSayisi = aktifTaslar.Count(t => t.renk == Renk.Beyaz),
                SiyahTasSayisi = aktifTaslar.Count(t => t.renk == Renk.Siyah)
            };
        }
    }
}