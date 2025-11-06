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
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ===== CORS =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("Allowlocalhost",
        p => p.WithOrigins("http://localhost:3000", "http://192.168.249.63:3000") // Aquí se puede agregar otros orígenes permitidos
              .AllowAnyHeader()
              .AllowAnyMethod()
        // .AllowCredentials() // <-- habilítar solo si realmente lo se necesita
        );
});

// ===== Controllers + JSON =====
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        // Mantiene los nombres EXACTOS de tus propiedades C#
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
      opt.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
      {
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateIssuerSigningKey = true,
          ValidIssuer = jwt["Issuer"],
          ValidAudience = jwt["Audience"],
          IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
          ClockSkew = TimeSpan.Zero // tokens expiran exactamente cuando deben
      };
  });

builder.Services.AddAuthorization();

var app = builder.Build();

// ===== Pipeline =====
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();
app.UseCors("Allowlocalhost"); // 1) CORS
app.UseAuthentication();            // 2) Auth (JWT)
app.UseAuthorization();             // 3) Authorization
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
