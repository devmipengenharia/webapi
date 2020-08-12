using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SEO.Models;
using SEO.Service;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Script.Serialization;
using WebApi.Models;


namespace WebApi.Controllers
{
    public class WebApiController : ApiController
    {

        string resposta = string.Empty;        
        Banco.crud.manipulabanco mb;

        /*métodos*/

        [HttpGet, Route("v1/DataAtual")]
        public string DataAtual()
        {
            return DateTime.Now.ToString("dd/MM/yyyy");
        }

        [HttpGet, Route("v1/Login/{cpf}/{senha}")]
        //public string Login(string cpf, string senha)
        public RetornoWS Login(string cpf, string senha)
        {
            var model = new SEO.Models.LoginModel();
            model.UserName = cpf;
            model.Password = senha;
            //resposta = Login(model);
            return Login(model);
        }
        public RetornoWS Login(LoginModel model)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            js.MaxJsonLength = Int32.MaxValue;
            UsuarioLogado UsuarioLogado = new UsuarioLogado();
            try
            {
                //Condições para verificar se os campos foram preenchido
                if (String.IsNullOrEmpty(model.UserName))
                    throw new Exception("o Campo Login deve ser preenchido.");
                if (String.IsNullOrEmpty(model.Password))
                    throw new Exception("O Campo Senha deve ser preenchido.");
                UsuarioService UsuarioService = new UsuarioService();

                //Método para verificar os campos e trazer os dados do usuários
                SEO_Usuario user = UsuarioService.Login(model.UserName, model.Password);

                if (user == null)
                    throw new Exception("Usuário ou senha incorreta.");

                //Alimentando o objeto Usuário Logado
                UsuarioLogado.Id = user.U_Id;
                UsuarioLogado.Nome = user.U_Nome;
                UsuarioLogado.Hash = user.U_Hash;
                UsuarioLogado.CPF = user.U_CPF;
                UsuarioLogado.Chapa = user.U_Chapa;

                List<Colaborador> ListColaboradores = UsuarioService.GetColaboradores();
                List<Obra> ListObra = new ObraService().GetObras(user.U_Id);
                return new RetornoWS { Erro = false, UsuarioLogado = UsuarioLogado, Colaboradores = ListColaboradores, Obras = ListObra };

            }
            catch (Exception ex)
            {
                return new RetornoWS { Erro = true, Message = ex.Message };
            }
        }


        [HttpPost, Route("v1/UpdateData/{user}")]
        public RetornoWS UpdateData(string user, HttpRequestMessage request)
        {
            var json = request.Content.ReadAsStringAsync().Result;
            JavaScriptSerializer js = new JavaScriptSerializer();
            if (json == null)
            {
                return new RetornoWS { Erro = true, Message = "Erro" };
            }
            else
            {
                List<Obra> ListaObras = new List<Obra>();
                JObject jo = JObject.Parse(json);
                var Obras = jo.SelectToken("Obras");
                foreach (var i in Obras)
                {
                    string obra = i.ToString();
                    obra = obra.Replace("\"" + "HorasTotaisApropriacao" + "\"" + ": null", "\"" + "HorasTotaisApropriacao" + "\"" + ": 0");
                    var ObraJson = JsonConvert.DeserializeObject<Obra>(obra);
                    ListaObras.Add(ObraJson);                
                }
                UpdateData(ListaObras, user);
                mb = new Banco.crud.manipulabanco();
                var model = new SEO.Models.LoginModel();
                model.UserName = mb.pesquisar("select U_UserName from SEO_Usuario where U_Id = " + "'" + user + "'");
                model.Password = mb.pesquisar("select U_Password from SEO_Usuario where U_Id = " + "'" + user + "'");
                var login = Login(model);
                UsuarioLogado UsuarioLogado = new UsuarioLogado();
                var Usuario = new UsuarioService().Login(model.UserName, model.Password);
                UsuarioLogado.Id = Usuario.U_Id;
                UsuarioLogado.Nome = Usuario.U_Nome;
                UsuarioLogado.Hash = Usuario.U_Hash;
                UsuarioLogado.CPF = Usuario.U_CPF;
                UsuarioLogado.Chapa = Usuario.U_Chapa;

                List<Colaborador> ListColaboradores = new UsuarioService().GetColaboradores();
                List<Obra> ListObra = new ObraService().GetObras(Usuario.U_Id);
                return new RetornoWS { Erro = false, UsuarioLogado = UsuarioLogado, Colaboradores = ListColaboradores, Obras = ListObra };                
                
            }
        }

        //Método para Atualizar os dados

        //public string UpdateData(string obra, string user)
        public static RetornoWS UpdateData(List<Obra> obra, string user)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            try
            {
                //Condição para verificar se o objeto obra está vazio
                //if (string.IsNullOrEmpty(obra))
                if (obra == null)
                    throw new Exception("objeto vazio");

                //List<Obra> Obras = js.Deserialize<List<Obra>>(obra);
                List<Obra> Obras = obra;
                //UsuarioLogado usuario = js.Deserialize<UsuarioLogado>(user);

                UsuarioLogado usuario = new UsuarioLogado();
                Util.sql = "select * from SEO_Usuario where U_Id = " + "'" + user + "'";
                using (SqlConnection conn = new SqlConnection(Util.datasource))
                {
                    SqlCommand cmd = new SqlCommand(Util.sql, conn);
                    SqlDataReader dr = null;
                    conn.Open();
                    dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        usuario.Chapa = dr["U_Chapa"].ToString();
                        usuario.CPF = dr["U_CPF"].ToString();
                        usuario.Id = Guid.Parse(dr["U_Id"].ToString());
                        usuario.Hash = Guid.Parse(dr["U_Hash"].ToString());
                        usuario.Nome = dr["U_Nome"].ToString();
                    }
                    dr.Close();
                    dr.Dispose();
                    conn.Close();
                    conn.Dispose();
                }

                ObraService obrasService = new ObraService();

                foreach (Obra item in Obras)
                {
                    foreach (var item2 in item.ApropriacaoColaborador)
                    {
                        ApropriacaoColaborador aprop = item2;

                        //Condição para verficiar se é um novo Colaborador para a obra
                        if (String.IsNullOrEmpty(item2.Id.ToString()) || item2.Id.ToString().Equals("00000000-0000-0000-0000-000000000000"))
                            aprop = obrasService.InsertApropriacaoColaborador(item.Id.ToString(), item2);

                        foreach (var item3 in item2.ListApropriacaoAtividade)
                        {
                            //Condição para verficiar se é uma nova Atividade para o(s) Colaborador(es)
                            if (String.IsNullOrEmpty(item3.Id.ToString()) || item3.Id.ToString().Equals("00000000-0000-0000-0000-000000000000"))
                                obrasService.InsertA_Atividade(aprop.Id.ToString(), item3);
                            else
                            {
                                obrasService.UpdateA_Atividade(item3);
                            }                                
                        }
                    }
                }               
                List<Colaborador> ListColaboradores = new UsuarioService().GetColaboradores();
                List<Obra> ListObra = new ObraService().GetObras(usuario.Id);
                return new RetornoWS { Erro = false, Message = "Itens Atualizados Com Sucesso.", Colaboradores = ListColaboradores, Obras = ListObra };                               
                //return js.Serialize(new RetornoWS { Erro = false, Message = "Itens Atualizados Com Sucesso.", Colaboradores = ListColaboradores, Obras = ListObra });

            }
            catch (Exception ex)
            {
                //return js.Serialize(new RetornoWS { Erro = true, Message = ex.Message });
                return  new RetornoWS { Erro = true, Message = ex.Message };
            }
        }

        [HttpGet, Route("v1/UsuarioAutenticado/{usuario}/{senha}")]
        public string UsuarioAutenticado(string usuario, string senha)
        {

            mb = new Banco.crud.manipulabanco();
            if (mb.pesquisar("select count(*) from Usuario where login = " + "'" + usuario + "'") == "0")
                resposta = "Usuário inválido";
            else
            {
                Util.sql = "select cast(codigo as varchar(10)) + ';' + cast(tipoautenticacao as varchar(10)) from Usuario where login = " + "'" + usuario + "'";
                string[] VetorUsuario = mb.pesquisar(Util.sql, 1, ";").Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                bool achou = false;
                if (VetorUsuario[1] == "1") // AD
                {
                    string ldap = "LDAP://DC=DMIP,DC=local";
                    DirectoryEntry directoryRoot = new DirectoryEntry(ldap);
                    DirectorySearcher buscador = new DirectorySearcher(directoryRoot, "(&(objectClass=User)(objectCategory=Person))");
                    buscador.Filter = string.Format("(&(objectCategory=person)(anr={0}))", usuario);
                    SearchResultCollection resultados = buscador.FindAll();
                    foreach (SearchResult resultado in resultados)
                    {
                        DirectoryEntry de = resultado.GetDirectoryEntry();
                        if ((string)de.Properties["SAMAccountName"][0] == usuario)
                        {
                            int flags = (int)de.Properties["userAccountControl"].Value;
                            if ((flags == 512) || (flags == 66048)) // usuário ativo no AD
                            {
                                PrincipalContext contexto = new PrincipalContext(ContextType.Domain);
                                GroupPrincipal grupoSEO = GroupPrincipal.FindByIdentity(contexto, "SEO_Grupo");
                                if (grupoSEO != null)
                                {
                                    foreach (Principal p in grupoSEO.GetMembers())
                                    {
                                        UserPrincipal usuarioSEO = p as UserPrincipal;
                                        if (usuarioSEO.SamAccountName == usuario) // encontrou o usuário
                                        {
                                            achou = contexto.ValidateCredentials(usuario, senha); // validar a senha
                                            resposta = achou ? VetorUsuario[0] : "Usuário não encontrado no activity directory";
                                            break;
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
                else
                {
                    if (VetorUsuario[1] == "2") // Usuário do Sistema
                    {
                        if (mb.pesquisar("select count(*) from Usuario where login = " + "'" + usuario + "'" + " and senha = " + "'" + senha + "'") == "0")
                            resposta = "Usuário inválido";
                        else
                            resposta = VetorUsuario[0];
                    }
                    else // toten
                    {
                        if (mb.pesquisar("select count(*) from Usuario where IPToten = " + "'" + usuario + "'" + " and senha = " + "'" + senha + "'") == "0")
                            resposta = "Usuário inválido";
                        else
                            resposta = VetorUsuario[0];
                    }
                }
            }
            return resposta;
        }
    }
}
