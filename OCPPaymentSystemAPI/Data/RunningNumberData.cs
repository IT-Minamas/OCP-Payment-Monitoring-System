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

        public async Task<string> GenerateMemoNumberAsync()
        {
            using SqlConnection conn = _database.GetConnection();

            await conn.OpenAsync();

            SqlTransaction trans = conn.BeginTransaction();

            try
            {
                string yearMonth = DateTime.Now.ToString("yyyyMM");

                string sql = @"

SELECT fldLastNumber

FROM tbdRunningNumber

WHERE fldModule=@Module

AND fldYearMonth=@YearMonth

";

                SqlCommand cmd = new(sql, conn, trans);

                cmd.Parameters.Add("@Module", System.Data.SqlDbType.NVarChar).Value = "MEM";

                cmd.Parameters.Add("@YearMonth", System.Data.SqlDbType.Char).Value = yearMonth;

                object obj = await cmd.ExecuteScalarAsync();

                int runningNo = 1;

                if (obj == null)
                {
                    sql = @"

INSERT INTO tbdRunningNumber

VALUES

(@Module,@YearMonth,1)

";

                    cmd = new(sql, conn, trans);

                    cmd.Parameters.Add("@Module", System.Data.SqlDbType.NVarChar).Value = "MEM";

                    cmd.Parameters.Add("@YearMonth", System.Data.SqlDbType.Char).Value = yearMonth;

                    await cmd.ExecuteNonQueryAsync();
                }
                else
                {
                    runningNo = Convert.ToInt32(obj) + 1;

                    sql = @"

UPDATE tbdRunningNumber

SET fldLastNumber=@RunningNo

WHERE fldModule=@Module

AND fldYearMonth=@YearMonth

";

                    cmd = new(sql, conn, trans);

                    cmd.Parameters.Add("@RunningNo", System.Data.SqlDbType.Int).Value = runningNo;

                    cmd.Parameters.Add("@Module", System.Data.SqlDbType.NVarChar).Value = "MEM";

                    cmd.Parameters.Add("@YearMonth", System.Data.SqlDbType.Char).Value = yearMonth;

                    await cmd.ExecuteNonQueryAsync();
                }

                trans.Commit();

                return $"MEM{yearMonth}{runningNo:00000}";
            }
            catch
            {
                trans.Rollback();

                throw;
            }
        }
    }
}