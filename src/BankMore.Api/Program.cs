using System.Text;
using BankMore.Domain.Interfaces;
using BankMore.Infrastructure.Repositorios;
using BankMore.Infrastructure.Mensageria;
using KafkaFlow;
using KafkaFlow.Serializer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var kafkaBootstrap = builder.Configuration["Kafka:BootstrapServers"];
var kafkaProtocol = builder.Configuration["Kafka:SecurityProtocol"] ?? "Plaintext";
var kafkaUsername = builder.Configuration["Kafka:SaslUsername"];
var kafkaPassword = builder.Configuration["Kafka:SaslPassword"];
var redisConnection = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BankMore API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Digite 'Bearer' [espaço] e o seu token.\r\n\r\nExemplo: \"Bearer 12345abcdef\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] { }
        }
    });
});

if (string.IsNullOrEmpty(builder.Configuration["Redis:ConnectionString"]))
{
    builder.Services.AddDistributedMemoryCache();
}
else
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
        options.InstanceName = "BankMore_";
    });
}

var connectionString = builder.Configuration.GetConnectionString("OracleConnection") ?? "User Id=system;Password=Bruninha04@@;Data Source=localhost:1521/xe;";
builder.Services.AddSingleton(connectionString);

builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>(sp => new UsuarioRepositorio(connectionString));
builder.Services.AddScoped<IAgenciaRepositorio, AgenciaRepositorio>(sp => new AgenciaRepositorio(connectionString));
builder.Services.AddScoped<IContaCorrenteRepositorio, ContaCorrenteRepositorio>(sp => new ContaCorrenteRepositorio(connectionString));
builder.Services.AddScoped<IMovimentoRepositorio, MovimentoRepositorio>(sp => new MovimentoRepositorio(connectionString));
builder.Services.AddScoped<ITransferenciaRepositorio, TransferenciaRepositorio>(sp => new TransferenciaRepositorio(connectionString));
builder.Services.AddScoped<IIdempotenciaRepositorio, IdempotenciaRepositorio>(sp => new IdempotenciaRepositorio(connectionString));
builder.Services.AddScoped<ITarifaRepositorio, TarifaRepositorio>(sp => new TarifaRepositorio(connectionString));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(BankMore.Application.Handlers.CriarContaHandler).Assembly));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ChaveSuperSecretaBankMore2026!@#$"))
        };
    });

if (!string.IsNullOrEmpty(kafkaBootstrap))
{
    builder.Services.AddKafka(kafka => kafka
        .AddCluster(cluster => cluster
            .WithBrokers(new[] { kafkaBootstrap })
            .WithSecurityInformation(si => {
                if (kafkaProtocol.Equals("SaslSsl", StringComparison.OrdinalIgnoreCase)) {
                    si.SecurityProtocol = KafkaFlow.Configuration.SecurityProtocol.SaslSsl;
                    si.SaslMechanism = KafkaFlow.Configuration.SaslMechanism.Plain;
                    si.SaslUsername = kafkaUsername;
                    si.SaslPassword = kafkaPassword;
                }
            })
            .AddProducer("BankMoreProducer", producer => producer
                .DefaultTopic("transferencias-realizadas")
                .AddMiddlewares(m => m.AddSerializer<JsonCoreSerializer>())
            )
        )
    );
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
