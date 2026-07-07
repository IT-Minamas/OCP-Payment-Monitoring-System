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
            DashboardResponse result =
                new();


            using SqlConnection conn =
                _database.GetConnection();


            await conn.OpenAsync();



            //-------------------------------
            // Company Filter
            //-------------------------------


            string companyFilter = "";


            if (request.CompanyAccess.Count > 0)
            {
                List<string> param =
                    new();


                for (int i = 0;
                    i < request.CompanyAccess.Count;
                    i++)
                {
                    param.Add("@Company" + i);
                }


                companyFilter =
                    $" AND fldCompanyCode IN ({string.Join(",", param)}) ";
            }



            //-------------------------------
            // Draft
            //-------------------------------


            string sql =
            @"
            SELECT COUNT(*)
            FROM vw_SearchMemo
            WHERE ApprovalLevel=0
            "
            +
            companyFilter;


            SqlCommand cmd =
                new(sql, conn);



            for (int i = 0;
                i < request.CompanyAccess.Count;
                i++)
            {
                cmd.Parameters.Add(
                    "@Company" + i,
                    SqlDbType.VarChar).Value =
                    request.CompanyAccess[i];
            }



            result.Draft =
                Convert.ToInt32(
                    await cmd.ExecuteScalarAsync());



            //-------------------------------
            // Approval Summary
            //-------------------------------


            sql =
            @"

            SELECT

                ApprovalLevel,

                REPLACE(
                    ApprovalStatus,
                    'Waiting for ',
                    ''
                ) AS ApprovalName,

                COUNT(*) Total

            FROM vw_SearchMemo

            WHERE ApprovalLevel>0

            "
            +
            companyFilter
            +
            @"

            GROUP BY

                ApprovalLevel,

                ApprovalStatus

            ORDER BY

                ApprovalLevel

            ";



            cmd =
                new(sql, conn);



            for (int i = 0;
                 i < request.CompanyAccess.Count;
                 i++)
            {
                cmd.Parameters.Add(
                    "@Company" + i,
                    SqlDbType.VarChar).Value =
                    request.CompanyAccess[i];
            }



            SqlDataReader dr =
                await cmd.ExecuteReaderAsync();



            while (await dr.ReadAsync())
            {
                result.Approval.Add(
                    new DashboardApproval
                    {

                        ApprovalLevel =
                            Convert.ToInt32(
                                dr["ApprovalLevel"]),


                        ApprovalName =
                            dr["ApprovalName"]
                            .ToString() ?? "",


                        Total =
                            Convert.ToInt32(
                                dr["Total"])

                    });
            }



            return result;
        }
    }
}