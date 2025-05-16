
using Microsoft.EntityFrameworkCore;
using SatrancAPI.Datas;
using SatrancAPI.Entities.Models;
using SatrancAPI.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthorization();

// CORS politikasý ekleme (frontend ile iletiþim için)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin()
                         .AllowAnyMethod()
                         .AllowAnyHeader());
});

// DbContext'i servis olarak ekleme
builder.Services.AddDbContext<SatrancDbContext>(options =>
    options.UseSqlServer("Server=CAN\\SQLEXPRESS01;Database=SatrançTakipDB;User Id=sa;Password=1;TrustServerCertificate=True;"));

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

// CORS politikasýný etkinleþtir
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

// 1. Tüm oyunlarý getir
app.MapGet("/api/oyunlar", async (SatrancDbContext db) =>
{
    var oyunlar = await db.Oyunlar
        .Include(o => o.BeyazOyuncu)
        .Include(o => o.SiyahOyuncu)
        .ToListAsync();
    return Results.Ok(oyunlar);
});

// 2. ID'ye göre oyunu getir
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

// 3. Yeni oyun oluþtur
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

// 4. Bir oyundaki tüm taþlarý getir
app.MapGet("/api/oyunlar/{oyunId}/taslar", async (Guid oyunId, SatrancDbContext db) =>
{
    var taslar = await db.Taslar
        .Where(t => t.OyunId == oyunId && t.AktifMi)
        .ToListAsync();

    return Results.Ok(taslar);
});

// 5. Bir taþýn geçerli hamlelerini getir
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
            return Results.Ok(new { message = "Hamle baþarýyla yapýldý" });
        else
            return Results.BadRequest("Geçersiz hamle");
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

// 8. Yeni oyuncu oluþtur
app.MapPost("/api/oyuncular", async (Oyuncu oyuncu, SatrancDbContext db) =>
{
    oyuncu.Id = Guid.NewGuid();
    db.Oyuncular.Add(oyuncu);
    await db.SaveChangesAsync();
    return Results.Created($"/api/oyuncular/{oyuncu.Id}", oyuncu);
});

// 9. Tüm oyuncularý getir
app.MapGet("/api/oyuncular", async (SatrancDbContext db) =>
{
    var oyuncular = await db.Oyuncular.ToListAsync();
    return Results.Ok(oyuncular);
});
// Oyuncu güncelleme
app.MapPut("/api/oyuncular/{id}", async (Guid id, Oyuncu oyuncu, SatrancDbContext db) =>
{
    var mevcutOyuncu = await db.Oyuncular.FindAsync(id);
    if (mevcutOyuncu == null) return Results.NotFound();

    mevcutOyuncu.isim = oyuncu.isim;
    mevcutOyuncu.email = oyuncu.email;
    // Diðer alanlar...

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


app.Run();

// API istekleri için basit sýnýflar (DTO yerine record kullanýyoruz)
public record OyunOlusturRequest(Guid BeyazOyuncuId, Guid SiyahOyuncuId);
public record HamleRequest(Guid TasId, int HedefX, int HedefY);