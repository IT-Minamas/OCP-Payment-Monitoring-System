using Microsoft.Data.SqlClient;
using OCPPaymentSystemAPI.Models;

namespace OCPPaymentSystemAPI.Data
{
    public class SupplierData
    {
        private readonly Database _database;

        public SupplierData(Database database)
        {
            _database = database;
        }

        public async Task<List<SupplierResponse>> GetAllAsync(string MillCode)
        {
            List<SupplierResponse> list = new();

            using SqlConnection conn =
                _database.GetConnection();

            await conn.OpenAsync();

            string sql = @"

SELECT

SUPPLIER_CODE,
SUPPLIER_NAME,
SUPPLIER_ADDR1,
SUPPLIER_ADDR2,
SUPPLIER_TELNO,
SUPPLIER_FAXNO,
SUPPLIER_HNDNO,
SAP_CODE

FROM [172.16.192.10].[SAP_Replicate_New].[dbo].[SW_SUPPLIER]

WHERE

Client_ID=@fldMillCode AND SUPP_TYPE=2 AND UACTIVE='Y'

ORDER BY SUPPLIER_NAME

";

            SqlCommand cmd = new(sql, conn);
            cmd.Parameters.AddWithValue("@fldMillCode", MillCode);

            SqlDataReader dr =
                await cmd.ExecuteReaderAsync();

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