//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.

//// Configura CORS para permitir cualquier origen
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAllOrigins",
//        builder => builder.AllowAnyOrigin()  // Permitir solicitudes desde cualquier origen
//                          .AllowAnyHeader()  // Permitir cualquier encabezado
//                          .AllowAnyMethod()); // Permitir cualquier método (GET, POST, etc.)
//});

//builder.Services.AddControllers();
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//// Usa CORS
//app.UseCors("AllowAllOrigins");
//app.UseAuthorization();
//app.MapControllers();
//app.Run();


///SEGUNDA VERSION DE API FUNCIONAL ---------
///

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ===== CORS =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("Allowlocalhost", p =>
        p.WithOrigins(
              "http://localhost:3000",
              "http://192.168.249.63:3000",
              "http://cjuarez99-001-site1.anytempurl.com"   // <- tu dominio público (http)
                                                            // añade "https://..." cuando migres a https
          )
         .AllowAnyHeader()
         .AllowAnyMethod()
    // .AllowCredentials() // habilítalo solo si realmente lo necesitas
    );
});

// ===== Controllers + JSON =====
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNamingPolicy = null;
        opts.JsonSerializerOptions.DictionaryKeyPolicy = null;
    });

// ===== Swagger =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ===== JWT Auth =====
var jwt = builder.Configuration.GetSection("Jwt");
var keyBytes = Encoding.UTF8.GetBytes(jwt["Key"]!);

builder.Services
  .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  .AddJwtBearer(opt =>
  {
      opt.TokenValidationParameters = new TokenValidationParameters
      {
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateIssuerSigningKey = true,
          ValidIssuer = jwt["Issuer"],
          ValidAudience = jwt["Audience"],
          IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
          ClockSkew = TimeSpan.Zero
      };
  });

builder.Services.AddAuthorization();

var app = builder.Build();

// ===== Swagger siempre (si gustas condicionarlo a Dev, ok) =====
app.UseSwagger();
app.UseSwaggerUI();

// ===== Static Files =====
// Sirve wwwroot
app.UseStaticFiles();

// Asegura carpeta wwwroot/uploads y mapea /uploads
var webRoot = app.Environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
var uploadsPath = Path.Combine(webRoot, "uploads");
Directory.CreateDirectory(uploadsPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads",
    ServeUnknownFileTypes = true,              // opcional, por si no detecta MIME
    DefaultContentType = "application/octet-stream"
});

// ⚠️ Si tu sitio público está en HTTP, mejor NO fuerces HTTPS.
// app.UseHttpsRedirection(); // <-- desactivado mientras uses sólo http

app.UseCors("Allowlocalhost");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();


///FIN DE VERSION


//PRIMER VERSION DE API--------------

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.

//builder.Services.AddControllers();
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();
