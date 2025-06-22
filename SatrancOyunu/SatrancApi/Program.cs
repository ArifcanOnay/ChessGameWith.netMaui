using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using SatrancAPI.Datas;
using SatrancAPI.Entities.Models;
using SatrancAPI.Services;
using SatrancApi.Services; // ICurrentUserService için
#nullable disable 

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthorization();

//  HttpContextAccessor ve CurrentUserService ekleme
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// CORS politikası ekleme (frontend ile iletişim için)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin()
                         .AllowAnyMethod()
                         .AllowAnyHeader());
});

// DbContext'i servis olarak ekleme
builder.Services.AddDbContext<SatrancDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Servisler ekleme
builder.Services.AddScoped<TahtaYoneticisi>();
builder.Services.AddScoped<OyunYoneticisi>();

// Swagger ekleme
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Satranç API",
        Version = "v1",
        Description = "Satranç oyunu için API"
    });
});

var app = builder.Build();

app.UseCors("AllowAll");

// Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// API endpoint'leri

// Tüm oyunları getir
app.MapGet("/api/oyunlar", async (SatrancDbContext db) =>
{
    var oyunlar = await db.Oyunlar
        .Include(o => o.BeyazOyuncu)
        .Include(o => o.SiyahOyuncu)
        .ToListAsync();
    return Results.Ok(oyunlar);
});

// Id'ye göre oyun getir
app.MapGet("/api/oyunlar/{id}", async (Guid id, SatrancDbContext db) =>
{
    var oyun = await db.Oyunlar
        .Include(o => o.BeyazOyuncu)
        .Include(o => o.SiyahOyuncu)
        .Include(o => o.Hamleler)
        .FirstOrDefaultAsync(o => o.OyunId == id);

    if (oyun == null)
        return Results.NotFound();

    return Results.Ok(oyun);
});

// Yeni oyun oluştur
app.MapPost("/api/oyunlar", async (OyunOlusturRequest request, OyunYoneticisi oyunYoneticisi) =>
{
    try
    {
        var yeniOyun = await oyunYoneticisi.YeniOyunOlustur(request.BeyazOyuncuId, request.SiyahOyuncuId);

        // SADECE GEREKLİ BİLGİLERİ DÖNDÜR
        var temizResponse = new
        {
            OyunId = yeniOyun.OyunId,
            BeyazOyuncuId = yeniOyun.BeyazOyuncuId,
            SiyahOyuncuId = yeniOyun.SiyahOyuncuId,
            BaslangicTarihi = yeniOyun.BaslangicTarihi,
            Durum = yeniOyun.Durum
        };

        return Results.Created($"/api/oyunlar/{yeniOyun.OyunId}", temizResponse);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

// Bir oyundaki tüm taşları getir
app.MapGet("/api/oyunlar/{oyunId}/taslar", async (Guid oyunId, SatrancDbContext db) =>
{
    var taslar = await db.Taslar
        .Where(t => t.OyunId == oyunId && t.AktifMi)
        .ToListAsync();

    return Results.Ok(taslar);
});

// Bir taşın geçerli hamlelerini getir
app.MapGet("/api/oyunlar/{oyunId}/taslar/{tasId}/gecerli-hamleler", async (Guid oyunId, Guid tasId, OyunYoneticisi oyunYoneticisi) =>
{
    try
    {
        var gecerliHamleler = await oyunYoneticisi.GecerliHamleleriGetir(oyunId, tasId);

        // JSON formatını düzenle - x ve y koordinatlarını düzgün döndür
        var duzenliHamleler = gecerliHamleler.Select(h => new {
            x = h.X,
            y = h.Y
        }).ToList();

        return Results.Ok(duzenliHamleler);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

//  Hamle endpoint'i - User tracking ile
app.MapPost("/api/oyunlar/{oyunId}/hamleler", async (Guid oyunId,
                                                    HamleRequest request,
                                                    OyunYoneticisi oyunYoneticisi,
                                                    SatrancDbContext db,
                                                    ICurrentUserService currentUserService) =>
{
    try
    {
        // Hamle yapan oyuncuyu belirle
        var hamleYapanTas = await db.Taslar
            .Include(t => t.Oyuncu)
            .FirstOrDefaultAsync(t => t.TasId == request.TasId);

        if (hamleYapanTas?.Oyuncu != null)
        {
            //  Hamle yapan oyuncuyu current user olarak set et
            currentUserService.SetCurrentUser(
                hamleYapanTas.Oyuncu.email!,
                hamleYapanTas.Oyuncu.Id,
                hamleYapanTas.Oyuncu.isim
            );
        }

        // Hedef konumda taş var mı kontrol et
        var hedefTas = await db.Taslar
            .Include(t => t.Oyuncu)
            .FirstOrDefaultAsync(t => t.OyunId == oyunId && t.X == request.HedefX && t.Y == request.HedefY && t.AktifMi);

        // Hamle yap
        var basarili = await oyunYoneticisi.HamleYap(oyunId, request.TasId, request.HedefX, request.HedefY);

        if (basarili)
        {
            //  Eğer taş yenildiyse, audit ile birlikte soft delete
            if (hedefTas != null)
            {
                Console.WriteLine($"🎯 TAŞ YENİLİYOR: {hedefTas.TasSimgesi} - Sahip: {hedefTas.Oyuncu?.isim}");
                Console.WriteLine($"🎯 YENEN OYUNCU: {hamleYapanTas?.Oyuncu?.isim}");

                // Soft delete - audit otomatik çalışacak
                db.SoftDelete(hedefTas);
                await db.SaveChangesAsync();

                Console.WriteLine($"✅ TAŞ YENİLDİ ve AUDİT KAYDEDİLDİ!");
            }

            return Results.Ok(new { message = "Hamle başarıyla yapıldı" });
        }
        else
        {
            return Results.BadRequest("Geçersiz hamle - Taş hareket kurallarına aykırı");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Hamle hatası: {ex.Message}");
        return Results.BadRequest($"Hamle hatası: {ex.Message}");
    }
});

// Hamleleri getir
app.MapGet("/api/oyunlar/{oyunId}/hamleler", async (Guid oyunId, SatrancDbContext db) =>
{
    var hamleler = await db.Hamleler
        .Where(h => h.OyunId == oyunId)
        .OrderBy(h => h.HamleTarihi)
        .ToListAsync();

    return Results.Ok(hamleler);
});

// Yeni oyuncu oluştur
app.MapPost("/api/oyuncular", async (OyuncuOlusturRequest request, SatrancDbContext db) =>
{
    var oyuncu = new Oyuncu
    {
        Id = Guid.NewGuid(),
        isim = request.isim,
        email = request.email,
        renk = request.renk
    };
    db.Oyuncular.Add(oyuncu);
    await db.SaveChangesAsync();
    return Results.Created($"/api/oyuncular/{oyuncu.Id}", oyuncu);
});

// Tüm oyuncuları getir
app.MapGet("/api/oyuncular", async (SatrancDbContext db) =>
{
    var oyuncular = await db.Oyuncular.ToListAsync();
    return Results.Ok(oyuncular);
});

// Oyuncu güncelleme
app.MapPut("/api/oyuncular/{id}", async (Guid id, OyuncuOlusturRequest request, SatrancDbContext db) =>
{
    var mevcutOyuncu = await db.Oyuncular.FindAsync(id);
    if (mevcutOyuncu == null) return Results.NotFound();

    mevcutOyuncu.isim = request.isim;
    mevcutOyuncu.email = request.email;
    mevcutOyuncu.renk = request.renk;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Oyuncu silme
app.MapDelete("/api/oyuncular/{id}", async (Guid id, SatrancDbContext db) =>
{
    var oyuncu = await db.Oyuncular.FindAsync(id);
    if (oyuncu == null) return Results.NotFound();

    db.Oyuncular.Remove(oyuncu);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapGet("/api/oyunlar/{oyunId}/durum", async (Guid oyunId, OyunYoneticisi oyunYoneticisi, SatrancDbContext db) =>
{
    var oyun = await db.Oyunlar.FindAsync(oyunId);
    if (oyun == null)
        return Results.NotFound();

    await oyunYoneticisi.TahtayiYukle(oyunId);

    // Son hamleyi al
    var sonHamle = await db.Hamleler
        .Where(h => h.OyunId == oyunId)
        .OrderByDescending(h => h.HamleTarihi)
        .FirstOrDefaultAsync();

    // Sıradaki oyuncuyu belirle
    bool beyazSirasi = sonHamle == null || sonHamle.OyuncuId == oyun.SiyahOyuncuId;
    Renk siradakiRenk = beyazSirasi ? Renk.Beyaz : Renk.Siyah;

    bool beyazSahTehdit = oyunYoneticisi.SahTehditAltindaMi(Renk.Beyaz);
    bool siyahSahTehdit = oyunYoneticisi.SahTehditAltindaMi(Renk.Siyah);

    bool beyazSahMat = beyazSahTehdit && oyunYoneticisi.SahMatMi(Renk.Beyaz);
    bool siyahSahMat = siyahSahTehdit && oyunYoneticisi.SahMatMi(Renk.Siyah);

    bool oyunBitti = beyazSahMat || siyahSahMat;
    string? kazanan = null;

    if (beyazSahMat)
    {
        kazanan = "Siyah";
        oyun.Durum = Durum.Bitiyor;
        oyun.KazananOyuncu = "Siyah";
        oyun.BitisNedeni = "Şah Mat";
        oyun.BitisTarihi = DateTime.Now;
    }
    else if (siyahSahMat)
    {
        kazanan = "Beyaz";
        oyun.Durum = Durum.Bitiyor;
        oyun.KazananOyuncu = "Beyaz";
        oyun.BitisNedeni = "Şah Mat";
        oyun.BitisTarihi = DateTime.Now;
    }

    if (oyunBitti)
    {
        await db.SaveChangesAsync();
    }

    // Debug logları
    Console.WriteLine($"Beyaz şah tehdit: {beyazSahTehdit}, Siyah şah tehdit: {siyahSahTehdit}");
    Console.WriteLine($"Beyaz şah mat: {beyazSahMat}, Siyah şah mat: {siyahSahMat}");
    Console.WriteLine($"Oyun bitti: {oyunBitti}, Kazanan: {kazanan}");

    return Results.Ok(new
    {
        Durum = oyun.Durum,
        SiradakiOyuncuRengi = (int)siradakiRenk,
        BeyazSahTehditAltinda = beyazSahTehdit,
        SiyahSahTehditAltinda = siyahSahTehdit,
        BeyazSahMat = beyazSahMat,
        SiyahSahMat = siyahSahMat,
        OyunBittiMi = oyunBitti,
        Kazanan = kazanan,
        BitisNedeni = oyun.BitisNedeni
    });
});

// Piyon terfi etme (Promotion)
app.MapPost("/api/oyunlar/{oyunId}/piyon-terfi", async (Guid oyunId, PiyonTerfiRequest request, SatrancDbContext db) =>
{
    try
    {
        // Taşın var olduğunu ve piyon olduğunu kontrol et
        var tas = await db.Taslar.FirstOrDefaultAsync(t => t.TasId == request.PiyonId && t.AktifMi);
        if (tas == null || tas.turu != TasTuru.Piyon)
            return Results.BadRequest("Geçersiz piyon");

        // Piyonun terfi hattında olduğunu kontrol et (beyaz için 0, siyah için 7)
        bool terfiPozisyonundaMi = (tas.renk == Renk.Beyaz && tas.X == 0) || (tas.renk == Renk.Siyah && tas.X == 7);
        if (!terfiPozisyonundaMi)
            return Results.BadRequest("Piyon terfi pozisyonunda değil");

        // Geçerli terfi türlerini kontrol et
        if (request.YeniTasTuru == TasTuru.Piyon || request.YeniTasTuru == TasTuru.Şah)
            return Results.BadRequest("Piyon, piyon veya şah'a terfi edemez");

        // Piyonu istenen türe yükselt
        tas.turu = request.YeniTasTuru;

        // Taş sembolünü güncelle
        tas.TasSimgesi = TasSimbolunuGetir(request.YeniTasTuru, tas.renk);

        await db.SaveChangesAsync();

        return Results.Ok(new
        {
            message = "Piyon başarıyla terfi edildi",
            tas = new
            {
                TasId = tas.TasId,
                X = tas.X,
                Y = tas.Y,
                turu = tas.turu,
                TasSimgesi = tas.TasSimgesi,
                renk = tas.renk
            }
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"Terfi hatası: {ex.Message}");
    }
});

// Kullanıcı kayıt endpoint'i
app.MapPost("/api/kullanicilar/kayit", async (KullaniciKayitRequest request, SatrancDbContext db) =>
{
    // Aynı email var mı kontrol et
    var mevcutKullanici = await db.Oyuncular
        .FirstOrDefaultAsync(o => o.email == request.Email);

    if (mevcutKullanici != null)
    {
        return Results.BadRequest(new { Mesaj = "Bu email adresi ile kayıtlı kullanıcı zaten mevcut" });
    }

    // Yeni kullanıcı oluştur
    var yeniOyuncu = new Oyuncu
    {
        Id = Guid.NewGuid(),
        isim = request.KullaniciAdi,
        email = request.Email,
        Sifre = request.Sifre,
        renk = Renk.Beyaz // Default renk, sonra değiştirilebilir
    };

    db.Oyuncular.Add(yeniOyuncu);
    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        Mesaj = "Kullanıcı başarıyla oluşturuldu",
        OyuncuId = yeniOyuncu.Id,
        Isim = yeniOyuncu.isim
    });
});

// Şifre değiştirme
app.MapPut("/api/kullanicilar/sifre-degistir", async (SifreEmailDegistirRequest request, SatrancDbContext db) =>
{
    try
    {
        var oyuncu = await db.Oyuncular
            .FirstOrDefaultAsync(o => o.email == request.Email);

        if (oyuncu == null)
        {
            return Results.NotFound(new { Mesaj = "Bu email ile kayıtlı kullanıcı bulunamadı" });
        }

        // Mevcut şifre kontrolü
        if (oyuncu.Sifre != request.EskiSifre)
        {
            return Results.BadRequest(new { Mesaj = "Mevcut şifre yanlış" });
        }

        // Yeni şifre doğrulama
        if (request.YeniSifre != request.YeniSifreTekrar)
        {
            return Results.BadRequest(new { Mesaj = "Yeni şifreler eşleşmiyor" });
        }

        // Şifre güncelleme
        oyuncu.Sifre = request.YeniSifre;

        await db.SaveChangesAsync();

        return Results.Ok(new { Mesaj = "Şifre başarıyla güncellendi" });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Hata: {ex.Message}");
    }
});

app.MapPost("/api/kullanicilar/login", async (LoginRequest request,
                                            SatrancDbContext db,
                                            ICurrentUserService currentUserService) =>
{
    // Kullanıcıyı email ile bul
    var kullanici = await db.Oyuncular
        .FirstOrDefaultAsync(o => o.email == request.Email);

    if (kullanici == null)
    {
        return Results.BadRequest(new { Mesaj = "E-posta adresi bulunamadı" });
    }

    // Şifre kontrolü
    if (kullanici.Sifre != request.Sifre)
    {
        return Results.BadRequest(new { Mesaj = "Şifre yanlış" });
    }

    //  Kullanıcı adını da set et
    currentUserService.SetCurrentUser(kullanici.email!, kullanici.Id, kullanici.isim);

    Console.WriteLine($" {kullanici.isim} ({kullanici.email}) - ID: {kullanici.Id}");

    // Başarılı giriş
    return Results.Ok(new
    {
        Mesaj = "Giriş başarılı",
        OyuncuId = kullanici.Id,
        KullaniciAdi = kullanici.isim,
        Email = kullanici.email
    });
});


//  Logout endpoint'i
app.MapPost("/api/kullanicilar/logout", async (LogoutRequest request, SatrancDbContext db) =>
{
    try
    {
        // Kullanıcı doğrulama
        var kullanici = await db.Oyuncular
            .FirstOrDefaultAsync(o => o.Id == request.KullaniciId);

        if (kullanici == null)
        {
            return Results.NotFound(new { Mesaj = "Kullanıcı bulunamadı" });
        }

        // Aktif oyunları kontrol et ve kaydet
        var aktifOyunlar = await db.Oyunlar
            .Where(o => (o.BeyazOyuncuId == request.KullaniciId || o.SiyahOyuncuId == request.KullaniciId)
                     && o.Durum == Durum.Aktif)
            .ToListAsync();

        foreach (var oyun in aktifOyunlar)
        {
            // Oyun durumunu 'Beklemede' yap
            oyun.Durum = Durum.Beklemede;

            // Çıkış notunu ekle
            oyun.BitisNedeni = $"Kullanıcı çıkış yaptı - {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        }
        //  Sadece gerekli alanları dolduran hamle kaydı
        if (aktifOyunlar.Any())
        {
            var logoutKaydı = new Hamle
            {
                HamleId = Guid.NewGuid(),
                OyunId = aktifOyunlar.First().OyunId, // İlk aktif oyun
                OyuncuId = request.KullaniciId,
                TasId = Guid.NewGuid(), // Dummy taş ID
                turu = TasTuru.Piyon, // Dummy taş türü
                BaslangicX = 0,
                BaslangicY = 0,
                HedefX = 0,
                HedefY = 0,
                HamleTarihi = DateTime.Now,
                Notasyon = "LOGOUT",
                RokMu = false
            };

            db.Hamleler.Add(logoutKaydı);
        }

        await db.SaveChangesAsync();

        // Logout kaydı (Audit Trail için)
        var logoutKaydi = new Hamle
        {
            HamleId = Guid.NewGuid(),
            OyuncuId = request.KullaniciId,
            HamleTarihi = DateTime.Now,
            Notasyon = "LOGOUT",
            RokMu = false
        };

        db.Hamleler.Add(logoutKaydi);
        await db.SaveChangesAsync();

        // Başarılı response
        return Results.Ok(new
        {
            Mesaj = "Çıkış başarıyla tamamlandı",
            CikisTarihi = DateTime.Now,
            KaydelilenOyunSayisi = aktifOyunlar.Count
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Logout hatası: {ex.Message}");
    }
});

// Audit Trail Endpoint'leri

// Oyuncu audit bilgileri
app.MapGet("/api/audit/oyuncu/{id}", async (Guid id, SatrancDbContext db) =>
{
    var oyuncu = await db.Oyuncular
        .IgnoreQueryFilters() // Silinmiş kayıtları da getir
        .FirstOrDefaultAsync(o => o.Id == id);

    if (oyuncu == null)
        return Results.NotFound();

    return Results.Ok(new
    {
        OyuncuBilgileri = new
        {
            oyuncu.Id,
            oyuncu.isim,
            oyuncu.email,
            oyuncu.renk
        },
        AuditBilgileri = new
        {
            KayitTarihi = oyuncu.CreatedDate,
            KayitEden = oyuncu.CreatedBy,      // Email adresi
            SonGuncelleme = oyuncu.UpdatedDate,
            SonGuncelleyen = oyuncu.UpdatedBy, // Email adresi
            SilindiMi = oyuncu.IsDeleted,
            SilinmeTarihi = oyuncu.DeletedDate,
            Silen = oyuncu.DeletedBy
        }
    });
});

// Tüm audit logları
app.MapGet("/api/audit/all", async (SatrancDbContext db) =>
{
    var oyuncuAuditler = await db.Oyuncular
        .IgnoreQueryFilters()
        .Select(o => new {
            Tip = "Oyuncu",
            Id = o.Id,
            Isim = o.isim,
            Email = o.email,
            KayitTarihi = o.CreatedDate,
            KayitEden = o.CreatedBy,
            SonGuncelleme = o.UpdatedDate,
            SonGuncelleyen = o.UpdatedBy,
            SilindiMi = o.IsDeleted
        })
        .ToListAsync();

    var oyunAuditler = await db.Oyunlar
        .IgnoreQueryFilters()
        .Select(o => new {
            Tip = "Oyun",
            Id = o.OyunId,
            Isim = "Oyun " + o.OyunId.ToString().Substring(0, 8),
            Email = "N/A",
            KayitTarihi = o.CreatedDate,
            KayitEden = o.CreatedBy,
            SonGuncelleme = o.UpdatedDate,
            SonGuncelleyen = o.UpdatedBy,
            SilindiMi = o.IsDeleted
        })
        .ToListAsync();

    var tumAuditler = oyuncuAuditler.Concat(oyunAuditler).OrderByDescending(a => a.KayitTarihi);

    return Results.Ok(tumAuditler);
});

// Hamle audit'i - Kim hangi hamleyi yaptı
app.MapGet("/api/audit/hamleler/{oyunId}", async (Guid oyunId, SatrancDbContext db) =>
{
    var hamleAuditler = await db.Hamleler
        .IgnoreQueryFilters()
        .Where(h => h.OyunId == oyunId)
        .Include(h => h.Oyuncu)
        .Select(h => new {
            HamleId = h.HamleId,
            HamleTarihi = h.HamleTarihi,
            Oyuncu = h.Oyuncu!.isim,
            OyuncuEmail = h.Oyuncu!.email,
            Notasyon = h.Notasyon,
            Baslangic = $"{h.BaslangicX},{h.BaslangicY}",
            Hedef = $"{h.HedefX},{h.HedefY}",
            TasTuru = h.turu.ToString(),
            AuditBilgileri = new
            {
                KayitEden = h.CreatedBy,
                KayitTarihi = h.CreatedDate,
                Guncelleyen = h.UpdatedBy,
                GuncellemeTarihi = h.UpdatedDate
            }
        })
        .OrderBy(h => h.HamleTarihi)
        .ToListAsync();

    return Results.Ok(hamleAuditler);
});

app.Run();

// Taş sembolü helper metodu
string TasSimbolunuGetir(TasTuru tur, Renk renk)
{
    if (renk == Renk.Beyaz)
    {
        return tur switch
        {
            TasTuru.Piyon => "♙",
            TasTuru.Kale => "♖",
            TasTuru.At => "♘",
            TasTuru.Fil => "♗",
            TasTuru.Vezir => "♕",
            TasTuru.Şah => "♔",
            _ => "?"
        };
    }
    else // Siyah
    {
        return tur switch
        {
            TasTuru.Piyon => "♟",
            TasTuru.Kale => "♜",
            TasTuru.At => "♞",
            TasTuru.Fil => "♝",
            TasTuru.Vezir => "♛",
            TasTuru.Şah => "♚",
            _ => "?"
        };
    }
}

// Record sınıfları
public class LoginRequest
{
    public string Email { get; set; }
    public string Sifre { get; set; }
}

//  LogoutRequest
public record LogoutRequest(Guid KullaniciId);

public record OyunOlusturRequest(Guid BeyazOyuncuId, Guid SiyahOyuncuId);
public record HamleRequest(Guid TasId, int HedefX, int HedefY);
public record OyuncuOlusturRequest(string isim, string email, Renk renk);
public record PiyonTerfiRequest(Guid PiyonId, TasTuru YeniTasTuru);
public record KullaniciKayitRequest(string KullaniciAdi, string Email, string Sifre);
public record SifreEmailDegistirRequest(string Email, string EskiSifre, string YeniSifre, string YeniSifreTekrar);