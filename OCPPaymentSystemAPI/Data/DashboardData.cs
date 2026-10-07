using Microsoft.Data.SqlClient;
using OCPPaymentSystemAPI.Models;
using System.Data;

namespace OCPPaymentSystemAPI.Data
{
    public class DashboardData
    {
        private readonly Database _database;

        public DashboardData(Database database)
        {
            _database = database;
        }

        public async Task<DashboardResponse> SummaryAsync(
            DashboardRequest request)
        {
            DashboardResponse result = new();
            using SqlConnection conn = _database.GetConnection();

            await conn.OpenAsync();

            //-------------------------------
            // Company Filter
            //-------------------------------
            string companyFilter = "";
            if (request.CompanyAccess.Count > 0)
            {
                List<string> param = new();

                for (int i = 0;i < request.CompanyAccess.Count;i++)
                {
                    param.Add("@Company" + i);
                }

                companyFilter = $" AND fldCompanyCode IN ({string.Join(",", param)}) ";
            }


            //-------------------------------
            // Draft
            //-------------------------------
            //string sql = @"SELECT COUNT(*) FROM vw_SearchMemo WHERE ApprovalLevel=0" + companyFilter;
            //SqlCommand cmd = new(sql, conn);

            //for (int i = 0;i < request.CompanyAccess.Count;i++)
            //{
            //    cmd.Parameters.Add("@Company" + i,SqlDbType.VarChar).Value = request.CompanyAccess[i];
            //}
            //result.Draft = Convert.ToInt32(await cmd.ExecuteScalarAsync());

            //-------------------------------
            // Approval Summary
            //-------------------------------
            string sql =
            @"
            SELECT
                a.ApprovalLevel,
                CASE
                    WHEN a.ApprovalStatus LIKE '%Rejected%' THEN 'Rejected by ' + b.fldDescription
                    WHEN a.ApprovalStatus LIKE '%Approved%' THEN 'Approved by ' + b.fldDescription
                    ELSE a.ApprovalStatus
                END AS ApprovalName,
                COUNT(*) AS Total
            FROM vw_SearchMemo a
            LEFT JOIN tbdApprovalLevel AS b ON a.ApprovalLevel = b.fldApprovalLevel
            WHERE 1=1
            "
            +
            companyFilter
            +
            @"
            GROUP BY a.ApprovalLevel, a.ApprovalStatus, b.fldDescription
            ORDER BY a.ApprovalLevel
            ";

            SqlCommand cmd = new(sql, conn);
            for (int i = 0;i < request.CompanyAccess.Count;i++)
            {
                cmd.Parameters.Add("@Company" + i,SqlDbType.VarChar).Value = request.CompanyAccess[i];
            }
            SqlDataReader dr = await cmd.ExecuteReaderAsync();
            while (await dr.ReadAsync())
            {
                result.Approval.Add(
                    new DashboardApproval
                    {
                        ApprovalLevel = Convert.ToInt32(dr["ApprovalLevel"]),
                        ApprovalName = dr["ApprovalName"].ToString() ?? "",

                        Total = Convert.ToInt32(dr["Total"])
                    });
            }

            return result;
        }
    }
}