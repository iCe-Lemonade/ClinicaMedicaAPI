
using ClinicaMedicaAPI.Data;
using ClinicaMedicaAPI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configuração da conexão com o MySQL
var connectionString = builder.Configuration
    .GetConnectionString("ClinicaConnection");

builder.Services.AddDbContext<ClinicaDbContext>(options =>
    options.UseMySQL(connectionString
        ?? throw new InvalidOperationException(
            "Conexão com MySQL não configurada.")));

// Registra o serviço de agendamento de consultas
builder.Services.AddScoped<ConsultaService>();

// Habilita os Controllers da API
builder.Services.AddControllers();

// Configuração do OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

// Disponibiliza o documento OpenAPI
// somente em ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Redirecionamento para HTTPS
app.UseHttpsRedirection();

// Mapeia as rotas dos Controllers
app.MapControllers();

app.Run();
