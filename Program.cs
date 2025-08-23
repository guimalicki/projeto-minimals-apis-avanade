using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Dominio.ModelViews;
using MinimalApi.Dominio.Servicos;
using MinimalApi.DTOs;
using MinimalApi.Infraestrutura.Db;

#region Builder

// Cria o builder da aplicação. O builder é responsável por configurar os serviços e o pipeline de middleware da aplicação.
var builder = WebApplication.CreateBuilder(args);

// Adiciona os serviços ao contêiner de injeção de dependência.
builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();
builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();

// Configuração do Entity Framework com MySQL
builder.Services.AddDbContext<DbContexto>(options =>
{
    options.UseMySql(builder.Configuration.GetConnectionString("mysql"),
    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql"))
    );
});

//Configuração do Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Constrói a aplicação.
var app = builder.Build();
#endregion

#region Home
// Define o endpoint raiz ("/") que retorna uma mensagem de boas-vindas.
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home"); // Adiciona a tag "Home" ao endpoint raiz para melhor organização no Swagger.
#endregion

#region Administradores
// Define o endpoint para o login de administradores. O endpoint recebe um objeto LoginDTO no corpo da requisição e utiliza o serviço de administrador para validar o login.
app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) =>
{
    if (administradorServico.Login(loginDTO) != null)
    {
        return Results.Ok("Login realizado com sucesso!");
    }
    else
    {
        return Results.Unauthorized();
    }
}).WithTags("Administradores"); // Adiciona a tag "Administrador" ao endpoint de login para melhor organização no Swagger.
#endregion

#region Veiculos
// Define o endpoint para listar todos os veículos. O endpoint utiliza o serviço de veículo para obter a lista de veículos.
app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{
    Veiculo veiculo = new Veiculo
    {
        Nome = veiculoDTO.Nome,
        Marca = veiculoDTO.Marca,
        Ano = veiculoDTO.Ano
    };
    veiculoServico.Incluir(veiculo);
    return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
}).WithTags("Veículos"); // Adiciona a tag "Veículo" ao endpoint de inclusão para melhor organização no Swagger.

//realiza a busca do veículo por ID
app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaPorId(id);
    if (veiculo == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(veiculo);
}).WithTags("Veículos"); // Adiciona a tag "Veículo" ao endpoint de obtenção por ID para melhor organização no Swagger.

app.MapGet("/veiculos", (IVeiculoServico veiculoServico) =>
{
    List<Veiculo> veiculos = veiculoServico.Todos();
    if (veiculos.Count <= 0)
    {
        return Results.NoContent();
    }
    return Results.Ok(veiculos);
}).WithTags("Veículos");

//Realiza a atualização do veículo
app.MapPut("/veiculos/{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{
    Veiculo veiculo = veiculoServico.BuscaPorId(id);
    if (veiculo == null)
    {
        return Results.NotFound();
    }

    veiculo.Nome = veiculoDTO.Nome;
    veiculo.Marca = veiculoDTO.Marca;
    veiculo.Ano = veiculoDTO.Ano;

    veiculoServico.Atualizar(veiculo);

    return Results.Ok(veiculo);
}).WithTags("Veículos");

app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
    Veiculo veiculo = veiculoServico.BuscaPorId(id);
    if (veiculo == null) return Results.NotFound();

    veiculoServico.ApagarPorId(veiculo);
    return Results.NoContent();

}).WithTags("Veículos");

#endregion

#region App
// Configuração do middleware para o Swagger
app.UseSwagger();
app.UseSwaggerUI();
#endregion

// Inicia a aplicação.
app.Run();