using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Linq;
using System.Threading.Tasks;
using MinimalApi.Dominio.Entidades;
using MinimalApi.Dominio.Interfaces;
using MinimalApi.DTOs;
using MinimalApi.Infraestrutura.Db;

namespace MinimalApi.Dominio.Servicos
{
    public class AdministradorServico : IAdministradorServico
    {
        private readonly DbContexto _contexto;

        public AdministradorServico(DbContexto db)
        {
            _contexto = db;
        }

        public Administrador BuscaPorId(int id)
        {
            return _contexto.Administradores.Find(id);
        }

        public Administrador Incluir(Administrador adm)
        {
            _contexto.Administradores.Add(adm);
            _contexto.SaveChanges();
            return adm;
        }

        public Administrador? Login(LoginDTO loginDTO)
        {
            Administrador adm = _contexto.Administradores.Where(a =>
            a.Email == loginDTO.Email && a.Senha == loginDTO.Senha).FirstOrDefault();
        
            return adm;
        }

        public List<Administrador> Todos(int? pagina)
        {
             var query = _contexto.Administradores.AsQueryable();

            int itensPorPagina = 10;

            if (pagina != null)
                query = query.Skip(((int)pagina - 1) * itensPorPagina).Take(itensPorPagina);

            return query.ToList();
        }
    }
}