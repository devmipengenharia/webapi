using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Models
{
    public class Projeto
    {
        private string codigo;
        private string descricao;

        public string Codigo
        {
            get { return codigo; }
            set { codigo = value; }
        }
        public string Descricao
        {
            get { return descricao; }
            set { descricao = value; }
        }
    }
}