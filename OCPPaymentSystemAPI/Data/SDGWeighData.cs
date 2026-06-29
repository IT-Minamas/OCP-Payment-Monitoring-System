using Microsoft.Data.SqlClient;
using OCPPaymentSystemAPI.Models;

namespace OCPPaymentSystemAPI.Data
{
    public class SDGWeighData
    {
        private readonly Database _database;

        public SDGWeighData(Database database)
        {
            _database = database;
        }

        public async Task<List<SDGWeighResponse>> SearchAsync(
            string supplierCode,
            DateTime dateFrom,
            DateTime dateTo)
        {
            List<SDGWeighResponse> list = new();

            using SqlConnection conn = _database.GetConnection();

            await conn.OpenAsync();

            string sql = @"

SELECT

POSTDATE,
LORRY_NO,
DRIVER_CODE,
SERIAL_NO,
0 AS BUNCH_COUNT,
WEIGHT_IN,
WEIGHT_OUT,
NETT_WEIGHT,
fldMillCode,
SUPPLIER_CODE

FROM [172.16.192.10].[SAP_Replicate_New].[dbo].[SW_PURCHASE]

WHERE

SUPPLIER_CODE=@SupplierCode

AND

POSTDATE BETWEEN @DateFrom
AND @DateTo

ORDER BY POSTDATE

";

            SqlCommand cmd = new(sql, conn);

            cmd.Parameters.AddWithValue("@SupplierCode", supplierCode);
            cmd.Parameters.AddWithValue("@DateFrom", dateFrom);
            cmd.Parameters.AddWithValue("@DateTo", dateTo);

            SqlDataReader dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                list.Add(new SDGWeighResponse
                {
                    MillCode = dr["fldMillCode"].ToString(),
                    SupplierCode = dr["SUPPLIER_CODE"].ToString(),
                    Date = Convert.ToDateTime(dr["POSTDATE"]),
                    PoliceNo = dr["LORRY_NO"].ToString(),
                    Driver = dr["DRIVER_CODE"].ToString(),
                    TicketNo = dr["SERIAL_NO"].ToString(),
                    BunchCount = Convert.ToInt32(dr["BUNCH_COUNT"]),
                    Bruto = Convert.ToDecimal(dr["WEIGHT_IN"]),
                    Tarra = Convert.ToDecimal(dr["WEIGHT_OUT"]),
                    Netto = Convert.ToDecimal(dr["NETT_WEIGHT"])
                });
            }

            return list;

        }
    }
}
