using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Proyecto_Analisis_API.Class
{
    public class ClassConect
    {
        public bool CUDQuery(string query, SqlParameter[] parameters, string connectionString)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddRange(parameters);
                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                // Log detailed SQL error
                Console.WriteLine("Error de SQL: " + ex.Message);
                Console.WriteLine("Detalles del Error SQL: " + ex.StackTrace);
                return false;
            }
            catch (Exception ex)
            {
                // Log general error
                Console.WriteLine("Error genérico: " + ex.Message);
                Console.WriteLine("Detalles del Error: " + ex.StackTrace);
                return false;
            }
        }



    }
}
