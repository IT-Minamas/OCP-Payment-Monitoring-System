using Microsoft.Data.SqlClient;

namespace OCPPaymentSystemAPI.Data
{
    public class RunningNumberData
    {
        private readonly Database _database;

        public RunningNumberData(Database database)
        {
            _database = database;
        }

        public async Task<string> GenerateMemoNumberAsync(string companyCode)
        {
            using SqlConnection conn = _database.GetConnection();
            await conn.OpenAsync();
            SqlTransaction trans = conn.BeginTransaction();

            try
            {
                string yearMonth = DateTime.Now.ToString("yyyyMM");
                int year = DateTime.Now.Year;
                int month = DateTime.Now.Month;

                string sql = @"
SELECT fldRegion
FROM vw_Company
WHERE fldCode=@CompanyCode";

                SqlCommand cmd = new(sql, conn, trans);
                cmd.Parameters.AddWithValue("@CompanyCode", companyCode);

                object regionObj = await cmd.ExecuteScalarAsync();
                string region = regionObj?.ToString() ?? "";

                sql = @"
SELECT a.fldLastNumber
FROM tbdRunningNumber AS a
WHERE a.fldModule=@Module
AND a.fldCompanyCode=@CompanyCode
AND a.fldYearMonth=@YearMonth";

                cmd = new(sql, conn, trans);
                cmd.Parameters.AddWithValue("@Module", "MEM");
                cmd.Parameters.AddWithValue("@CompanyCode", companyCode);
                cmd.Parameters.AddWithValue("@YearMonth", yearMonth);

                object obj = await cmd.ExecuteScalarAsync();

                int runningNo;
                if (obj == null)
                {
                    runningNo = 1;
                    sql = @"
INSERT INTO tbdRunningNumber(fldModule,fldCompanyCode,fldYearMonth,fldLastNumber)
VALUES(@Module,@CompanyCode,@YearMonth,@RunningNo)";

                    cmd = new(sql, conn, trans);
                    cmd.Parameters.AddWithValue("@Module", "MEM");
                    cmd.Parameters.AddWithValue("@CompanyCode", companyCode);
                    cmd.Parameters.AddWithValue("@YearMonth", yearMonth);
                    cmd.Parameters.AddWithValue("@RunningNo", runningNo);

                    await cmd.ExecuteNonQueryAsync();
                }
                else
                {
                    runningNo = Convert.ToInt32(obj) + 1;

                    sql = @"
UPDATE tbdRunningNumber
SET fldLastNumber=@RunningNo
WHERE fldModule=@Module
AND fldCompanyCode=@CompanyCode
AND fldYearMonth=@YearMonth";

                    cmd = new(sql, conn, trans);
                    cmd.Parameters.AddWithValue("@RunningNo", runningNo);
                    cmd.Parameters.AddWithValue("@Module", "MEM");
                    cmd.Parameters.AddWithValue("@CompanyCode", companyCode);
                    cmd.Parameters.AddWithValue("@YearMonth", yearMonth);

                    await cmd.ExecuteNonQueryAsync();
                }

                trans.Commit();

                string romanMonth = ToRoman(month);
                return $"M.{runningNo}/OCP-TREA/{companyCode}-TBS/{region}/{romanMonth}/{year}";
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        private string ToRoman(int month)
        {
            string[] roman =
            {
        "",
        "I",
        "II",
        "III",
        "IV",
        "V",
        "VI",
        "VII",
        "VIII",
        "IX",
        "X",
        "XI",
        "XII"
            };

            return roman[month];
        }

    }
}