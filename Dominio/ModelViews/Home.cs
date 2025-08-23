namespace MinimalApi.Dominio.ModelViews;

// Model View para a Home. É usado somente para retornar uma mensagem de boas-vindas e o link para a documentação da API.
public struct Home
{
    public string Mensagem { get => "Bem vindo a API de veículos - Minimal API"; }
    public string Doc { get => "/swagger"; }

}