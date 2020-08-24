using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace WebApi
{
    public class Util
    {
        // produção 
        public static string datasource = "Data Source=mip09" + ";" + "Integrated Security=false;Initial Catalog= SEOApp; uid=seo_teste; Password=sql0506#Mip";
        public static string corpore = "corpore";
        public static string cpf = string.Empty;
        public static string senha = string.Empty;

        /*

        public static string datasource = "Data Source=mip09\\teste" + ";" + "Integrated Security=false;Initial Catalog= SEOApp; uid=seo; Password=sql0506#Mip;";
        public static string corpore = "[Corpore_V12.1.28_090620]"; */
        public static string sql = string.Empty;
    }

    namespace Banco
    {
        public abstract class crud
        {            
            public abstract void atualizar(string sql);
            public abstract void inserir(string sql);
            public abstract string pesquisar(string sql);

            public abstract string pesquisar(string sql, int totalcampos, string separador);

            public class manipulabanco : crud
            {
                public override void atualizar(string sql)
                {
                    using (SqlConnection conn = new SqlConnection(Util.datasource))
                    {
                        SqlCommand cmd = new SqlCommand(sql, conn);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        conn.Dispose();
                    }
                }

                public override void inserir(string sql)
                {
                    using (SqlConnection conn = new SqlConnection(Util.datasource))
                    {
                        SqlCommand cmd = new SqlCommand(sql, conn);
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        conn.Dispose();
                    }
                }

                public override string pesquisar(string sql)
                {
                    using (SqlConnection conn = new SqlConnection(Util.datasource))
                    {
                        string valor = string.Empty;
                        SqlCommand cmd = new SqlCommand(sql, conn);
                        SqlDataReader dr = null;
                        conn.Open();
                        dr = cmd.ExecuteReader();
                        while (dr.Read())
                            valor = dr[0].ToString();
                        dr.Close();
                        dr.Dispose();
                        conn.Close();
                        conn.Dispose();
                        return valor;
                    }
                }

                public override string pesquisar(string sql, int totalcampos, string separador)
                {
                    string valor = string.Empty;
                    using (SqlConnection conn = new SqlConnection(Util.datasource))
                    {
                        SqlCommand cmd = new SqlCommand(sql, conn);
                        SqlDataReader dr = null;
                        conn.Open();
                        dr = cmd.ExecuteReader();
                        while (dr.Read())
                        {
                            for (int i = 0; i < totalcampos; i++)
                            {
                                if (valor == "")
                                    valor = dr[0].ToString() + separador;
                                else
                                    valor += dr[i].ToString() + separador;
                            }
                        }
                        dr.Close();
                        dr.Dispose();
                        conn.Close();
                        conn.Dispose();
                        return valor;
                    }
                }
            }
        }
    }

}
