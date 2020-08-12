using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SEO.Models;
using System.Globalization;

namespace SEO.Service
{
  public class ObraService
  {
    ApropriacaoHorasSEOEntities contexto = new ApropriacaoHorasSEOEntities(WebApi.Util.datasource);

    #region Método para alimentar a lista de objetos para serem enviados para o Aplicativo
    public List<Obra> GetObras(Guid? Id)
    {
      
	  // Em todas as Consultas os campos [Id] indica a chave unica da tabela e não deve ser repetir, o padrão utilizado é UniqueIdentifier.
	  
	  //Query para buscar a Obra pelo Id do Usuário logado
      String query = String.Format(@"SELECT 
	                                      O_Id [Id]
	                                    , O_Nome [Nome]
	                                    , O_Codigo [Codigo]
                                     FROM SEO_Obras O
                                      INNER JOIN SEO_UsuarioObra OU on UO_IdObra = O.O_Id AND OU.UO_Ativo = 1
                                     WHERE OU.UO_IdUsuario = '{0}'", Id.Value);

      List<Obra> Obras = contexto.Database.SqlQuery<Obra>(query).ToList();

      if (Obras != null)
      {
        foreach (Obra obra in Obras)
        {
          //Query para buscar os dados das Atividades por Obra
          query = String.Format(@"SELECT 
                                    A.A_Id [Id]
                                  , A.A_Codigo [Codigo]
                                  , A.A_Nome [Nome]
                                  , A.A_Descricao [Descricao]
                                FROM SEO_AtividadeObra AO
                                 INNER JOIN SEO_Atividade A on A.A_Id = AO.AO_IdAtividade
                                WHERE AO_IdObra = '{0}'", obra.Id);

          obra.Atividades = contexto.Database.SqlQuery<Atividade>(query).ToList();

          //Query para buscar os dados de Ponto por Colaboradores
          query = String.Format(@"SELECT 
	                                  AC_id [Id]
	                                , CONVERT(VARCHAR(10), AC_DataPonto, 103) [DataPonto]
	                                , AC_HorasPonto [HorasPonto]
                                  , AC_IdColaborador [IdColaborador]
                                FROM SEO_ApropriacaoColaborador
                                WHERE AC_IdObra = '{0}' AND AC_DataPonto BETWEEN DATEADD(DAY,-10,GETDATE()) and AC_DataPonto", obra.Id);

          obra.ApropriacaoColaborador = contexto.Database.SqlQuery<ApropriacaoColaborador>(query).ToList();

          //Query para buscar os dados das Atividades por Usuário Logado
		  
          query = string.Format(@"SELECT 
	                                      A.A_Id [Id]
	                                    , A.A_Nome [Nome]
	                                    , A.A_Descricao [Descricao]
	                                    , A.A_Codigo [Codigo]
                                    FROM SEO_AtividadeUsuario AU
	                                    INNER JOIN SEO_AtividadeObra AO ON AO.AO_Id = AU.AU_IdAtividadeObra 
	                                    INNER JOIN SEO_Atividade A ON A.A_Id = AO.AO_IdAtividade AND A.A_Ativo = 1
                                    WHERE AU.AU_IdUsuario = '{0}' AND AO.AO_IdObra = '{1}'", Id.Value, obra.Id);

          obra.ListAtividadeUsuario = contexto.Database.SqlQuery<Atividade>(query).ToList();

          //Condição para verificar se existe Colaboradores para a Obra
          if (obra.ApropriacaoColaborador != null)
          {
            foreach (ApropriacaoColaborador item in obra.ApropriacaoColaborador)
            {
              //Query para buscar as horas apropriadas do colaborador
              query = string.Format(@"SELECT 
	                                        AA_Id [Id]
	                                      , AA_HorasApropriadas [HorasApropriadas]
	                                      , CAST(AA_DataSincronizacao AS varchar(20)) [DataSincronizacao] 
                                      FROM SEO_ApropriacaoAtividade
                                      WHERE AA_IdApropriacaoColaborador = '{0}'", item.Id);
									  //Incluir Filtro de Data

              item.ListApropriacaoAtividade = contexto.Database.SqlQuery<ApropriacaoAtividade>(query).ToList();

              foreach (ApropriacaoAtividade item1 in item.ListApropriacaoAtividade)
              {
                //Query para buscar as Atividades do colaborador
                query = string.Format(@"SELECT 
	                                        A.A_Id [Id]
	                                      , A.A_Nome [Nome]
	                                      , A.A_Codigo [Codigo] 
                                        , A.A_Descricao [Descricao]
                                      FROM SEO_Atividade A
                                        INNER JOIN SEO_ApropriacaoAtividade AA ON AA.AA_IdAtividade = A.A_Id
                                      WHERE AA_Id = '{0}'", item1.Id);

                item1.Atividade = contexto.Database.SqlQuery<Atividade>(query).FirstOrDefault();
              }

              //Query para Buscar dados do Colaborador Corrente
              query = string.Format(@"SELECT 
	                                        C_Id [Id]
	                                      , C_Chapa [Chapa]
	                                      , C_Nome [Nome]
                                      FROM SEO_Colaborador
                                      WHERE C_Id = '{0}' AND C_Ativo = 1", item.IdColaborador);
              item.Colaborador = contexto.Database.SqlQuery<Colaborador>(query).FirstOrDefault();
              
			  // Este indicador indica que este colaborador é da Obra
			  item.Colaborador.Check = true;
            }
          }
        }
      }
      else
        Obras = new List<Obra>();

      return Obras;
    }

    //Método para buscar todas as atividades ativas independente da Obra, é necessário para a inclusão de atividade que não faz parte da Obra do Supervisor. 
    public List<Atividade> GetAtividades()
    {
      try
      {
        return contexto.Database.SqlQuery<Atividade>(@"SELECT
                                                       	  A_Id	Id
	                                                      , A_Nome Nome
	                                                      , A_Codigo Codigo
	                                                      , A_Descricao Descricao
                                                       FROM SEO_Atividade
                                                       WHERE 
                                                        A_Ativo = 1").ToList();
      }
      catch (Exception)
      {
        throw;
      }
    }
	#endregion
    #region  Metodos para Atualização de dados Aplicativo Vs Retaguarda(Banco de Dados)
	//Método para inserir um colaborador que executou uma atividade em uma Obra que ele não esta associado.
    public ApropriacaoColaborador InsertApropriacaoColaborador(string IdObra, ApropriacaoColaborador apropriacaocolaborador)
    {
      try
      {
        if (apropriacaocolaborador == null)
          throw new Exception("Nenhum valor encontrado.");
        if (string.IsNullOrEmpty(IdObra))
          throw new Exception("IdObra está vazio");

        string dia = apropriacaocolaborador.DataPonto.Split('/')[0];
        string mes = apropriacaocolaborador.DataPonto.Split('/')[1];
        string ano = apropriacaocolaborador.DataPonto.Split('/')[2];

        string data = string.Format("{0}/{1}/{2}", mes, dia, ano);

        apropriacaocolaborador.Id = Guid.NewGuid();

        string query = string.Format(@"INSERT INTO SEO_ApropriacaoColaborador VALUES 
                                        (
                                          '{0}'
                                        , '{1}'
                                        , '{2}'
                                        , '{3}'
                                        , '{4}')"
                                        , apropriacaocolaborador.Id
                                        , apropriacaocolaborador.IdColaborador
                                        , apropriacaocolaborador.HorasPonto
                                        , data
                                        , IdObra);

        contexto.Database.ExecuteSqlCommand(query);

        return apropriacaocolaborador;
      }
      catch (Exception ex)
      {
        throw new Exception(ex.Message);
      }
    }

    // Inserir Horas Apropriada Por Atividades
    public void InsertA_Atividade(string IdApropriacaoColaborador, ApropriacaoAtividade A_atividade)
    {
      try
      {
        if (A_atividade == null)
          throw new Exception("Nenhum Apropriação Atividade encontrado.");
        if (A_atividade.Atividade == null)
          throw new Exception("Nenhuma Atividade encontrado.");
        if (string.IsNullOrEmpty(IdApropriacaoColaborador))
          throw new Exception("IdApropriacaoColaborador está vazio");

          A_atividade.Id = Guid.NewGuid();

        string query = string.Format(@"INSERT INTO SEO_ApropriacaoAtividade VALUES 
                                       (
                                           '{0}'
                                         , '{1}'
                                         , '{2}'
                                         , '{3}'
                                         , '{4}'
                                       )",
                                        A_atividade.Id
                                      , A_atividade.Atividade.Id
                                      , A_atividade.HorasApropriadas
                                      , IdApropriacaoColaborador
                                      , DateTime.Now);

        contexto.Database.ExecuteSqlCommand(query);
      }
      catch (Exception ex)
      {
        throw new Exception(ex.Message);
      }
    }

    //Método para atualizar as Horas Apropriada Por Atividades
    public void UpdateA_Atividade(ApropriacaoAtividade A_atividade)
    {
      try
      {
        if (A_atividade == null)
          throw new Exception("Nenhum Apropriação Atividade encontrado.");

       
        string query = string.Format(@"UPDATE SEO_ApropriacaoAtividade SET 
                                          AA_HorasApropriadas = '{0}'
                                        , AA_DataSincronizacao = '{1}' 
                                      WHERE AA_Id = '{2}'"
                                    , A_atividade.HorasApropriadas.ToString().Replace(",",".")
                                    , DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")
                                    , A_atividade.Id);

        contexto.Database.ExecuteSqlCommand(query);
      }
      catch (Exception ex)
      {
        throw new Exception(ex.Message);
      }
    }
	#endregion

  }
}