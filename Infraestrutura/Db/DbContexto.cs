using System.Data.Entity;
using Microsoft.EntityFrameworkCore;
using MinimalApi.Dominio.Entidades;
using DbContext = Microsoft.EntityFrameworkCore.DbContext;


namespace MinimalApi.Infraestrutura.Db;

// Classe DbContexto que representa o contexto do banco de dados e herda de DbContext
public class DbContexto : DbContext
{
    // Propriedade para acessar as configurações da aplicação
    private readonly IConfiguration _configuracaoAppSettings;

    // Construtor que recebe as configurações da aplicação via injeção de dependência
    public DbContexto(IConfiguration configuracaoAppSettings) //Construtor
    {
        _configuracaoAppSettings = configuracaoAppSettings;
    }

    // Propriedades DbSet que representam as tabelas do banco de dados
    public Microsoft.EntityFrameworkCore.DbSet<Administrador> Administradores { get; set; } = default!;
    public Microsoft.EntityFrameworkCore.DbSet<Veiculo> Veiculos { get; set; } = default!;


    //Seed para cadastrar um Administrador inicial no banco de dados
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Administrador>().HasData(
            new Administrador
            {
                Id = 1,
                Email = "administrador@teste.com",
                Senha = "123456",
                Perfil = "Admin"
            }
        );
    }

    // Configuração do contexto do banco de dados, incluindo a string de conexão

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Verifica se as opções já foram configuradas
        if (!optionsBuilder.IsConfigured)
        {
            var stringConexao = _configuracaoAppSettings.GetConnectionString("mysql")?.ToString();
            if (!string.IsNullOrEmpty(stringConexao))
            {
                optionsBuilder.UseMySql(stringConexao,
                ServerVersion.AutoDetect(stringConexao)
                );
            }
        }
    }
}

