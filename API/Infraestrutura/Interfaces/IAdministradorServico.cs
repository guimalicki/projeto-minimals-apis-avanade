using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MinimalApi.Dominio.Entidades;
using MinimalApi.DTOs;

namespace MinimalApi.Dominio.Interfaces;

// Interface IAdministradorServico que define o contrato para o serviço de administrador
public interface IAdministradorServico
{
   Administrador Login(LoginDTO loginDTO);
   Administrador Incluir(Administrador administrador);
   List<Administrador> Todos(int? pagina);

   Administrador BuscaPorId(int id);
}