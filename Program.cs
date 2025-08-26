using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Enuns;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.Dominio.ModelViews;
using MinimalApi.Dominio.Servicos;
using MinimalApi.DTOs;
using MinimalApi.Infraestrutura.Db;

#region Builder

// Cria o builder da aplicação. O builder é responsável por configurar os serviços e o pipeline de middleware da aplicação.
var builder = WebApplication.CreateBuilder(args);

var key = builder.Configuration.GetSection("Jwt").ToString();
if (string.IsNullOrEmpty(key)) key = "123456";   

builder.Services.AddAuthentication(option =>
{
    option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(option =>
{
    option.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});



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
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT aqui"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string [] { }
        }
    });
});

builder.Services.AddAuthorization();

// Constrói a aplicação.
var app = builder.Build();
#endregion

#region Home
// Define o endpoint raiz ("/") que retorna uma mensagem de boas-vindas.
app.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home"); // Adiciona a tag "Home" ao endpoint raiz para melhor organização no Swagger.
#endregion

#region Administradores

string GerarTokenJWT(Administrador administrador)
{
    if (string.IsNullOrEmpty(key)) return string.Empty;

    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>()
    {
        new Claim("Email", administrador.Email),
        new Claim("Perfil", administrador.Perfil),
        new Claim(ClaimTypes.Role, administrador.Perfil)
    };

    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.Now.AddDays(1),
        signingCredentials: credentials
    );
    return new JwtSecurityTokenHandler().WriteToken(token); 
}

// Define o endpoint para o login de administradores. O endpoint recebe um objeto LoginDTO no corpo da requisição e utiliza o serviço de administrador para validar o login.
app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) =>
{
    Administrador adm = administradorServico.Login(loginDTO);

    if (adm != null)
    {
        string token = GerarTokenJWT(adm);
        return Results.Ok(new AdmLogado
        {
            Email = adm.Email,
            Perfil = adm.Perfil,
            Token = token
        });
    }
    else
    {
        return Results.Unauthorized();
    }
}).AllowAnonymous().WithTags("Administradores"); // Adiciona a tag "Administrador" ao endpoint de login para melhor organização no Swagger.

app.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) =>
{
    var validacao = new ErrosDeValidacao();

    if (string.IsNullOrEmpty(administradorDTO.Email)) validacao.Mensagens.Add("Email não pode ser vazio");
    if (string.IsNullOrEmpty(administradorDTO.Senha)) validacao.Mensagens.Add("Senha não pode ser vazia");
    if (administradorDTO.Perfil == null) validacao.Mensagens.Add("Perfil não pode ser vazio");

    if (validacao.Mensagens.Count > 0) return Results.BadRequest(validacao);

    Administrador adm = new Administrador
    {
        Email = administradorDTO.Email,
        Senha = administradorDTO.Senha,
        Perfil = administradorDTO.Perfil.ToString() ?? Perfil.Editor.ToString()
    };

    administradorServico.Incluir(adm);

    return Results.CreatedAtRoute($"/administrador/{adm.Id}, adm");

}).RequireAuthorization(new AuthorizeAttribute {Roles = "Adm"}).WithTags("Administradores");

app.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorServico administradorServico) =>
{
    var adms = new List<AdministradorModelView>();
    List<Administrador> administradors = administradorServico.Todos(pagina);
    foreach (var adm in administradors)
    {
        adms.Add(new AdministradorModelView
        {
            Id = adm.Id,
            Email = adm.Email,
            Perfil = adm.Perfil
        });
    }
    return Results.Ok(administradors);
}).RequireAuthorization(new AuthorizeAttribute {Roles = "Adm"}).WithTags("Administradores");

app.MapGet("/administradores/{id}", ([FromRoute] int id, IAdministradorServico administradorServico) =>
{
    Administrador adm = administradorServico.BuscaPorId(id);
    if (adm == null) return Results.NotFound();
    return Results.Ok(new AdministradorModelView
    {
        Id = adm.Id,
        Email = adm.Email,
        Perfil = adm.Perfil
        
    });
}).RequireAuthorization(new AuthorizeAttribute {Roles = "Adm"}).WithTags("Administradores");
#endregion

#region Veiculos

ErrosDeValidacao validaDTO(VeiculoDTO veiculoDTO)
{
    var validacao = new ErrosDeValidacao();

    if (string.IsNullOrEmpty(veiculoDTO.Nome))
        validacao.Mensagens.Add("O nome não pode ser Vazio");

    if (string.IsNullOrEmpty(veiculoDTO.Marca))
        validacao.Mensagens.Add("A Marca não pode ficar em branco");

    if (veiculoDTO.Ano < 1900)
        validacao.Mensagens.Add("Veículo muito antigo. Somente anos superiores a 1900.");

    return validacao;
}

// Define o endpoint para cadastrar os veículos
app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{
    
    var validacao = validaDTO(veiculoDTO);
    if (validacao.Mensagens.Count > 0)
        return Results.BadRequest(validacao);

    Veiculo veiculo = new Veiculo
    {
        Nome = veiculoDTO.Nome,
        Marca = veiculoDTO.Marca,
        Ano = veiculoDTO.Ano
    };
    veiculoServico.Incluir(veiculo);
    return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
}).RequireAuthorization().WithTags("Veículos"); // Adiciona a tag "Veículo" ao endpoint de inclusão para melhor organização no Swagger.

//realiza a busca do veículo por ID
app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaPorId(id);
    if (veiculo == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(veiculo);
}).RequireAuthorization().WithTags("Veículos"); // Adiciona a tag "Veículo" ao endpoint de obtenção por ID para melhor organização no Swagger.

app.MapGet("/veiculos", (IVeiculoServico veiculoServico) =>
{
    List<Veiculo> veiculos = veiculoServico.Todos();
    if (veiculos.Count <= 0)
    {
        return Results.NoContent();
    }
    return Results.Ok(veiculos);
}).RequireAuthorization().WithTags("Veículos");

//Realiza a atualização do veículo
app.MapPut("/veiculos/{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{

    Veiculo veiculo = veiculoServico.BuscaPorId(id);
    if (veiculo == null)
    {
        return Results.NotFound();
    }

    var validacao = validaDTO(veiculoDTO);
    if (validacao.Mensagens.Count > 0)
        return Results.BadRequest(validacao);

    veiculo.Nome = veiculoDTO.Nome;
    veiculo.Marca = veiculoDTO.Marca;
    veiculo.Ano = veiculoDTO.Ano;

    veiculoServico.Atualizar(veiculo);

    return Results.Ok(veiculo);
}).RequireAuthorization(new AuthorizeAttribute {Roles = "Adm"}).WithTags("Veículos");

app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
    Veiculo veiculo = veiculoServico.BuscaPorId(id);
    if (veiculo == null) return Results.NotFound();

    veiculoServico.ApagarPorId(veiculo);
    return Results.NoContent();

}).RequireAuthorization(new AuthorizeAttribute {Roles = "Adm"}).WithTags("Veículos");

#endregion

#region App
// Configuração do middleware para o Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();
#endregion


// Inicia a aplicação.
app.Run();