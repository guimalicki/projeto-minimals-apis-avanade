using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MinimalApi.Dominio.Enuns;

namespace MinimalApi.DTOs
{
    // DTO para login. Usado para transferir dados de login da aplicação cliente para o servidor.
    public record AdministradorDTO
    {
        public string Email { get; set; } = default!;
        public string Senha { get; set; } = default!;
        public Perfil? Perfil { get; set; } = default!;
    }
}