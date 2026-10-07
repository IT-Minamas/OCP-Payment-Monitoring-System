using Microsoft.Data.SqlClient;
using OCPPaymentSystemAPI.Models;

namespace OCPPaymentSystemAPI.Data
{
    public class ApproverData
    {
        private readonly Database _database;

        public ApproverData(Database database)
        {
            _database = database;
        }

        public async Task<List<ApproverResponse>> SearchAsync()
        {
            List<ApproverResponse> list = new();
            using SqlConnection conn = _database.GetConnection();
            await conn.OpenAsync();

            string sql = @"SELECT fldApprovalLevel, fldDescription FROM tbdApprovalLevel WHERE fldIsActive=1 ORDER BY fldSequence";

            SqlCommand cmd = new(sql, conn);
            SqlDataReader dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                list.Add(new ApproverResponse
                {
                    Code = dr["fldApprovalLevel"].ToString(),
                    Name = dr["fldDescription"].ToString(),
                });
            }

            return list;
        }
    }
}
