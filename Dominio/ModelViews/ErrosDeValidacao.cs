using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinimalApi.Dominio.ModelViews
{
    public struct ErrosDeValidacao
    {
        public ErrosDeValidacao()
        {
            Mensagens = new List<string>();
        }
        public List<string> Mensagens { get; set; }

    }
}