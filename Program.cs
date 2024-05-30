using RealtYeahBackend.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using RealtYeahBackend.Services;
using Microsoft.IdentityModel.Tokens;
using RealtYeahBackend.Helpers;
using System.Text;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<RealtyeahContext>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization();
builder.Services.AddCors();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
var key = Encoding.ASCII.GetBytes(builder.Configuration.GetSection("AppSettings").Get<AppSettings>().Secret);
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            var userService = context.HttpContext.RequestServices.GetRequiredService<UserService>();
            var userId = context.Principal.Identity.Name;
            var user = userService.GetByName(userId);
            if (user == null)
            {
                context.Fail("Unauthorized");
            }
            return Task.CompletedTask;
        }
    };
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = AuthOptions.ISSUER,

        ValidateAudience = true,
        ValidAudience = AuthOptions.AUDIENCE,

        ValidateLifetime = true
    };
});
builder.Services.AddSingleton(resolver => resolver.GetRequiredService<CspParameters>());
CspParameters csp = new CspParameters() { KeyContainerName = "login", ProviderType = 1, };
using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(4096, csp))
{
    rsa.PersistKeyInCsp = false;
}
builder.Services.AddSingleton(csp);
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<OperationService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();
