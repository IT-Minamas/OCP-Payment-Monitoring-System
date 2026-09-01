using Microsoft.Data.SqlClient;
using OCPPaymentSystemAPI.Models;
using System.Data;

namespace OCPPaymentSystemAPI.Data
{
    public class OCPSupplierData
    {
        private readonly Database _database;

        public OCPSupplierData(Database database)
        {
            _database = database;
        }

        public async Task<List<SupplierBankModel>> GetBankAsync(
    SupplierBankRequest request)
        {
            List<SupplierBankModel> list = new();

            using SqlConnection conn =
                _database.GetConnection();

            await conn.OpenAsync();

            string sql = @"
SELECT
    b.fldBankCode,
    b.fldBankAccountNo,
    b.fldNameOnBankAccount,
    b.fldBankName,
    b.fldBankAddress1,
    b.fldBankAddress2,
    b.fldSKN
FROM [172.16.192.10].[SAP_Replicate_New].[dbo].[SW_SUPPLIER] AS a
JOIN vw_Supplier AS b
    ON a.Sap_Code = b.fldCode
WHERE a.SUPPLIER_CODE = @SupplierCode
  AND a.Client_ID = @MillCode
  AND a.SUPP_TYPE = 2
  AND a.Sap_Code <> ''
  AND a.uActive = 'Y'
";

            using SqlCommand cmd =
                new SqlCommand(sql, conn);

            cmd.Parameters.Add(
                "@SupplierCode",
                SqlDbType.NVarChar)
                .Value = request.SupplierCode;

            cmd.Parameters.Add(
                "@MillCode",
                SqlDbType.NVarChar)
                .Value = request.MillCode;

            using SqlDataReader dr =
                await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                list.Add(
                    new SupplierBankModel
                    {
                        Code =
                            dr["fldBankCode"] == DBNull.Value
                                ? ""
                                : dr["fldBankCode"].ToString() ?? "",

                        BankAccountNo =
                            dr["fldBankAccountNo"] == DBNull.Value
                                ? ""
                                : dr["fldBankAccountNo"].ToString() ?? "",

                        NameOnBankAccount =
                            dr["fldNameOnBankAccount"] == DBNull.Value
                                ? ""
                                : dr["fldNameOnBankAccount"].ToString() ?? "",

                        BankName =
                            dr["fldBankName"] == DBNull.Value
                                ? ""
                                : dr["fldBankName"].ToString() ?? "",

                        BankAddress1 =
                            dr["fldBankAddress1"] == DBNull.Value
                                ? ""
                                : dr["fldBankAddress1"].ToString() ?? "",

                        BankAddress2 =
                            dr["fldBankAddress2"] == DBNull.Value
                                ? ""
                                : dr["fldBankAddress2"].ToString() ?? "",

                        SKN =
                            dr["fldSKN"] == DBNull.Value
                                ? ""
                                : dr["fldSKN"].ToString() ?? ""
                    });
            }

            return list;
        }

        public async Task<List<OCPSupplierResponse>> SearchAsync(
            string MillCode)
        {
            List<OCPSupplierResponse> list = new();

            using SqlConnection conn = _database.GetConnection();

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

            SqlDataReader dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                list.Add(new OCPSupplierResponse
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
