using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinimalApi.DTOs
{
    // DTO para login. Usado para transferir dados de login da aplicação cliente para o servidor.
    public record LoginDTO
    {
        public string Email { get; set; } = default!;
        public string Senha { get; set; } = default!;
    }
}