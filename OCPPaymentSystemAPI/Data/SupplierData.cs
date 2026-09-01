using Microsoft.Data.SqlClient;
using OCPPaymentSystemAPI.Models;
using System.Data;

namespace OCPPaymentSystemAPI.Data
{
    public class SupplierData
    {
        private readonly Database _database;

        public SupplierData(Database database)
        {
            _database = database;
        }

        public async Task<List<SupplierResponse>> GetAllAsync(string MillCode = "", string CompanyCode = "")
        {
            List<SupplierResponse> list = new();

            using SqlConnection conn =
                _database.GetConnection();

            await conn.OpenAsync();

            string sql =
            @"
SELECT *
FROM [172.16.192.10].[SAP_Replicate_New].[dbo].[SW_SUPPLIER]
WHERE SUPP_TYPE=2 
";

            if (!string.IsNullOrWhiteSpace(MillCode))
            {
                sql += @"AND Client_ID = @fldMillCode";
            }
            if (!string.IsNullOrWhiteSpace(CompanyCode))
            {
                sql += @"
AND Client_ID IN
(
    SELECT fldSAPVirtualCode
    FROM [172.16.192.10].[SAP_Replicate_New].[dbo].[vw_UnitSetup2]
    WHERE fldCompanyCode = @CompanyCode
    AND fldIsActive = 1
    AND fldType = 'M'
)
";
            }

            sql += @"
ORDER BY TRIM(SUPPLIER_NAME)
";
            SqlCommand cmd = new(sql, conn);

            if (!string.IsNullOrWhiteSpace(MillCode))
            {
                cmd.Parameters.Add("@fldMillCode", SqlDbType.VarChar).Value = MillCode;
            }

            if (string.IsNullOrWhiteSpace(MillCode) && !string.IsNullOrWhiteSpace(CompanyCode)
            )
            {
                cmd.Parameters.Add("@CompanyCode", SqlDbType.VarChar).Value = CompanyCode;
            }

            SqlDataReader dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                list.Add(new SupplierResponse
                {
                    Code = dr["SUPPLIER_CODE"].ToString(),
                    Name = dr["SUPPLIER_NAME"].ToString(),
                    Address1 = dr["SUPPLIER_ADDR1"].ToString(),
                    Address2 = dr["SUPPLIER_ADDR2"].ToString(),
                    Phone1 = dr["SUPPLIER_TELNO"].ToString(),
                    Phone2 = dr["SUPPLIER_FAXNO"].ToString(),
                    HandPhone = dr["SUPPLIER_HNDNO"].ToString(),
                    SAPCode = dr["SAP_CODE"].ToString()
                });
            }

            return list;
        }
    }
}