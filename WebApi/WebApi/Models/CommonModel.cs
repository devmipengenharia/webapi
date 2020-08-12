using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SEO.Models
{
    public class RetornoWS
    {
        public bool Erro { get; set; }
        public string Message { get; set; }
        public UsuarioLogado UsuarioLogado { get; set; }
        public List<Colaborador> Colaboradores { get; set; }
        public List<Obra> Obras { get; set; }
    }

    public class LoginModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class UsuarioLogado
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public Guid? Hash { get; set; }
        public string Chapa { get; set; }
        public string CPF { get; set; }
    }

    public class Colaborador
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public string Chapa { get; set; }
        public bool Check { get; set; }
    }

    public class Obra
    {
        public Obra()
        {
            Atividades = new List<Atividade>();
            ApropriacaoColaborador = new List<ApropriacaoColaborador>();
            ListAtividadeUsuario = new List<Atividade>();
        }

        public Guid Id { get; set; }
        public string Nome { get; set; }
        public string Codigo { get; set; }
        public List<Atividade> ListAtividadeUsuario { get; set; }
        public List<Atividade> Atividades { get; set; }
        public List<ApropriacaoColaborador> ApropriacaoColaborador { get; set; }
    }

    public class DadosApp
    {
        public DadosApp()
        {
            this.Obra = new List<Obra>();
        }

        public List<Obra> Obra { get; set; }
        public UsuarioLogado User { get; set; }
    }

    public class Atividade
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public string Codigo { get; set; }
        public string Descricao { get; set; }
    }

    public class ApropriacaoColaborador
    {
        public ApropriacaoColaborador()
        {
            ListApropriacaoAtividade = new List<ApropriacaoAtividade>();
            Colaborador = new Colaborador();
        }

        public Guid Id { get; set; }
        public decimal HorasPonto { get; set; }
        public decimal HorasTotaisApropriacao { get; set; }
        public string DataPonto { get; set; }
        public Guid IdColaborador { get; set; }
        public Colaborador Colaborador { get; set; }
        public List<ApropriacaoAtividade> ListApropriacaoAtividade { get; set; }
    }

    public class ApropriacaoAtividade
    {
        public Guid Id { get; set; }
        public Atividade Atividade { get; set; }
        public decimal HorasApropriadas { get; set; }
        public string DataSincronizcao { get; set; }
    }

    public class RetornoAPP
    {
        public RetornoAPP()
        {
            this.Erro = false;
        }

        public bool Erro { get; set; }
        public string Message { get; set; }
        public UsuarioLogado UsuarioLogado { get; set; }
        public List<Colaborador> Colaboradores { get; set; }
    }
}