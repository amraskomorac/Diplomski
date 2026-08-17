using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Diplomski.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSession();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    )
);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Prijava";
        options.AccessDeniedPath = "/Auth/Prijava";
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.ExecuteSqlRawAsync("CREATE TABLE IF NOT EXISTS goriva (id INT NOT NULL AUTO_INCREMENT PRIMARY KEY, vozilo_id INT NOT NULL, datum DATETIME(6) NOT NULL, litara DECIMAL(10,2) NOT NULL, cijena DECIMAL(10,2) NOT NULL, kilometraza INT NOT NULL, korisnik_id INT NOT NULL, INDEX IX_goriva_vozilo_id (vozilo_id), INDEX IX_goriva_korisnik_id (korisnik_id), CONSTRAINT FK_goriva_vozila FOREIGN KEY (vozilo_id) REFERENCES vozila(id) ON DELETE CASCADE, CONSTRAINT FK_goriva_korisnici FOREIGN KEY (korisnik_id) REFERENCES korisnici(id) ON DELETE CASCADE)");
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Registracija}/{id?}")
    .RequireAuthorization();

app.Run();
