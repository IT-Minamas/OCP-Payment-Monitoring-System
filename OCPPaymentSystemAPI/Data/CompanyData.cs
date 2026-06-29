using Microsoft.Data.SqlClient;
using OCPPaymentSystemAPI.Models;

namespace OCPPaymentSystemAPI.Data
{
    public class CompanyData
    {
        private readonly Database _database;

        public CompanyData(Database database)
        {
            _database = database;
        }

        public async Task<List<CompanyResponse>> SearchAsync()
        {
            List<CompanyResponse> list = new();

            using SqlConnection conn = _database.GetConnection();

            await conn.OpenAsync();

            string sql = @"

SELECT

fldCode,fldSAPCode,fldName

FROM [172.16.192.10].[SAP_Replicate_New].[dbo].[ADM_Company]

WHERE

fldIsActive=1

ORDER BY fldName

";

            SqlCommand cmd = new(sql, conn);

            SqlDataReader dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                list.Add(new CompanyResponse
                {
                    Code = dr["fldCode"].ToString(),
                    Name = dr["fldName"].ToString(),
                    SAPCode = dr["fldSAPCode"].ToString()
                });
            }

            return list;
        }
    }
}
