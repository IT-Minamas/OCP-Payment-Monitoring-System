using Microsoft.Data.SqlClient;
using OCPPaymentSystemAPI.Helpers;
using System.Data;

namespace OCPPaymentSystemAPI.Data
{
    public class Database
    {
        private readonly IConfiguration _configuration;

        public Database(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public SqlConnection GetConnection()
        {
            string key = AppEnvironment.IsProductionServer() ? "PRDConnection" :
                         AppEnvironment.IsQASServer() ? "QASConnection" : "";

            if (string.IsNullOrEmpty(key))
                throw new InvalidOperationException("Environment PRD/QAS tidak dikenali.");

            string connectionString = _configuration.GetConnectionString(key)
                ?? throw new InvalidOperationException($"Connection string '{key}' tidak ditemukan.");

            return new SqlConnection(connectionString);
        }
    }
}
