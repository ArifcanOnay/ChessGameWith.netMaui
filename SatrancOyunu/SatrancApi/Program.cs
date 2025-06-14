using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using SatrancAPI.Datas;
using SatrancAPI.Entities.Models;
using SatrancAPI.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthorization();

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
        Title = "Satran� API",
        Version = "v1",
        Description = "Satran� oyunu i�in API"
    });
});

var app = builder.Build();

// CORS politikas�n� etkinle�tir
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

// 1. T�m oyunlar� getir
app.MapGet("/api/oyunlar", async (SatrancDbContext db) =>
{
    var oyunlar = await db.Oyunlar
        .Include(o => o.BeyazOyuncu)
        .Include(o => o.SiyahOyuncu)
        .ToListAsync();
    return Results.Ok(oyunlar);
});

// 2. ID'ye g�re oyunu getir
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

// 3. Yeni oyun olu�tur
app.MapPost("/api/oyunlar", async (OyunOlusturRequest request, OyunYoneticisi oyunYoneticisi) =>
{
    try
    {
        var yeniOyun = await oyunYoneticisi.YeniOyunOlustur(request.BeyazOyuncuId, request.SiyahOyuncuId);
        return Results.Created($"/api/oyunlar/{yeniOyun.OyunId}", yeniOyun);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

// 4. Bir oyundaki t�m ta�lar� getir
app.MapGet("/api/oyunlar/{oyunId}/taslar", async (Guid oyunId, SatrancDbContext db) =>
{
    var taslar = await db.Taslar
        .Where(t => t.OyunId == oyunId && t.AktifMi)
        .ToListAsync();

    return Results.Ok(taslar);
});

// 5. Bir ta��n ge�erli hamlelerini getir
app.MapGet("/api/oyunlar/{oyunId}/taslar/{tasId}/gecerli-hamleler", async (Guid oyunId, Guid tasId, OyunYoneticisi oyunYoneticisi) =>
{
    try
    {
        var gecerliHamleler = await oyunYoneticisi.GecerliHamleleriGetir(oyunId, tasId);
        return Results.Ok(gecerliHamleler);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

// 6. Hamle yap
app.MapPost("/api/oyunlar/{oyunId}/hamleler", async (Guid oyunId, HamleRequest request, OyunYoneticisi oyunYoneticisi) =>
{
    try
    {
        var basarili = await oyunYoneticisi.HamleYap(oyunId, request.TasId, request.HedefX, request.HedefY);
        if (basarili)
            return Results.Ok(new { message = "Hamle ba�ar�yla yap�ld�" });
        else
            return Results.BadRequest("Ge�ersiz hamle");
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

// 7. Hamleleri getir
app.MapGet("/api/oyunlar/{oyunId}/hamleler", async (Guid oyunId, SatrancDbContext db) =>
{
    var hamleler = await db.Hamleler
        .Where(h => h.OyunId == oyunId)
        .OrderBy(h => h.HamleTarihi)
        .ToListAsync();

    return Results.Ok(hamleler);
});

// 8. Yeni oyuncu olu�tur
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

// 9. T�m oyuncular� getir
app.MapGet("/api/oyuncular", async (SatrancDbContext db) =>
{
    var oyuncular = await db.Oyuncular.ToListAsync();
    return Results.Ok(oyuncular);
});
// Oyuncu g�ncelleme
app.MapPut("/api/oyuncular/{id}", async (Guid id, OyuncuOlusturRequest request, SatrancDbContext db) =>
{
    var mevcutOyuncu = await db.Oyuncular.FindAsync(id);
    if (mevcutOyuncu == null) return Results.NotFound();

    mevcutOyuncu.isim = request.isim;
    mevcutOyuncu.email = request.email;
    mevcutOyuncu.renk = request.renk;
    // Diğer alanlar güncellenmiyor

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

// 10. Oyun durumunu getir (şah çekme, şah mat, vb.)
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
    
    // Şah çekme durumu
    bool beyazSahTehdit = oyunYoneticisi.SahTehditAltindaMi(Renk.Beyaz);
    bool siyahSahTehdit = oyunYoneticisi.SahTehditAltindaMi(Renk.Siyah);
    
    // Şah mat durumu
    bool beyazSahMat = beyazSahTehdit && oyunYoneticisi.SahMatMi(Renk.Beyaz);
    bool siyahSahMat = siyahSahTehdit && oyunYoneticisi.SahMatMi(Renk.Siyah);
    
    return Results.Ok(new
    {
        Durum = oyun.Durum,
        SiradakiOyuncuRengi = siradakiRenk,
        BeyazSahTehditAltinda = beyazSahTehdit, 
        SiyahSahTehditAltinda = siyahSahTehdit,
        BeyazSahMat = beyazSahMat,
        SiyahSahMat = siyahSahMat,
        OyunBittiMi = oyun.Durum != Durum.Oynaniyor
    });
});

// 11. Piyon terfi etme (Promotion)
app.MapPost("/api/oyunlar/{oyunId}/piyon-terfi", async (Guid oyunId, PiyonTerfiRequest request, SatrancDbContext db) =>
{
    // Taşın var olduğunu ve piyon olduğunu kontrol et
    var tas = await db.Taslar.FirstOrDefaultAsync(t => t.TasId == request.PiyonId && t.AktifMi);
    if (tas == null || tas.turu != TasTuru.Piyon)
        return Results.BadRequest("Geçersiz piyon");
    
    // Piyonun terfi hattında olduğunu kontrol et (beyaz için 0, siyah için 7)
    bool terfiPozisyonundaMi = (tas.renk == Renk.Beyaz && tas.X == 0) || (tas.renk == Renk.Siyah && tas.X == 7);
    if (!terfiPozisyonundaMi)
        return Results.BadRequest("Piyon terfi pozisyonunda değil");
    
    // Piyonu istenen türe yükselt
    tas.turu = request.YeniTasTuru;
    
    await db.SaveChangesAsync();
    return Results.Ok(new { message = "Piyon başarıyla terfi edildi", tas = tas });
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
        //oyuncu.UpdatedDate = DateTime.Now;
        //oyuncu.UpdatedBy = oyuncu.isim ?? "System";

        await db.SaveChangesAsync();

        return Results.Ok(new { Mesaj = "Şifre başarıyla güncellendi" });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Hata: {ex.Message}");
    }
});
app.MapPost("/api/kullanicilar/login", async (LoginRequest request, SatrancDbContext db) =>
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

    // Başarılı giriş
    return Results.Ok(new
    {
        Mesaj = "Giriş başarılı",
        OyuncuId = kullanici.Id,
        KullaniciAdi = kullanici.isim,
        Email = kullanici.email
    });
});



app.Run();

// API istekleri için basit sınıflar (DTO)
public class LoginRequest
{
    public string Email { get; set; }
    public string Sifre { get; set; }
}
public record OyunOlusturRequest(Guid BeyazOyuncuId, Guid SiyahOyuncuId);
public record HamleRequest(Guid TasId, int HedefX, int HedefY);
public record OyuncuOlusturRequest(string isim, string email, Renk renk);
public record PiyonTerfiRequest(Guid PiyonId, TasTuru YeniTasTuru);
public record KullaniciKayitRequest(string KullaniciAdi, string Email, string Sifre);
public record SifreEmailDegistirRequest(string Email, string EskiSifre, string YeniSifre, string YeniSifreTekrar);