using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MinimalApi.Dominio.Entidades;

namespace MinimalApi.Dominio.Interfaces
{
    // Interface IVeiculoServico que define o contrato para o serviço de veículo
    public interface IVeiculoServico
    {
        List<Veiculo> Todos(int pagina = 1, string? nome = null, string? marca = null);

        Veiculo? BuscaPorId(int id);

        void Incluir(Veiculo veiculo);

        void Atualizar(Veiculo veiculo);

        void ApagarPorId(Veiculo veiculo);

    }
}