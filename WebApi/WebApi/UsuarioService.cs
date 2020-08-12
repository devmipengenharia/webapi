using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SEO.Models;

namespace SEO.Service
{
  public class UsuarioService
  {
    ApropriacaoHorasSEOEntities contexto = new ApropriacaoHorasSEOEntities(WebApi.Util.datasource);

    //Método para verificar se os campos estão corretos 
    public SEO_Usuario Login(string UserName, string Password)
    {
      return contexto.SEO_Usuario.Where(a => a.U_UserName.Equals(UserName) && a.U_Password.Equals(Password)).FirstOrDefault();
    }

    //Método para buscar todos os Colaboradores ativos
    public List<Colaborador> GetColaboradores()
    {
      List<Colaborador> colaboradores = contexto.Database.SqlQuery<Colaborador>(@"SELECT 
                                                                                     C_Id [Id]
                                                                                   , C_Nome [Nome]
                                                                                   , C_Chapa [Chapa] 
                                                                                 FROM SEO_Colaborador 
                                                                                 WHERE 
                                                                                   C_Ativo = 1").ToList();
      return colaboradores;
    }
  }
}