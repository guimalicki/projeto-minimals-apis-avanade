using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinimalApi.DTOs

{
    // DTO para veículo. Usado para transferir dados de veículo da aplicação cliente para o servidor.
    public record VeiculoDTO
    {
        public string Nome { get; set; } = default!;
        public string Marca { get; set; } = default!;
        public int Ano { get; set; } = default!;
    }
}