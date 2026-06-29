using Microsoft.Data.SqlClient;
using OCPPaymentSystemAPI.Models.Workflow;
using System.Data;

namespace OCPPaymentSystemAPI.Data
{
    public class WorkflowData
    {
        private readonly Database _database;

        public WorkflowData(Database database)
        {
            _database = database;
        }

        private async Task<int> GetFirstApprovalLevel(
    SqlConnection conn,
    SqlTransaction trans)
        {
            string sql = @"

SELECT MIN(fldApprovalLevel)

FROM tbdApprovalLevel

";

            SqlCommand cmd = new(sql, conn, trans);

            object obj =
                await cmd.ExecuteScalarAsync();

            if (obj == DBNull.Value)
                throw new Exception(
                    "Approval Level belum dikonfigurasi.");

            return Convert.ToInt32(obj);
        }

        private async Task<bool> AttachmentComplete(
    SqlConnection conn,
    SqlTransaction trans,
    string memoNo)
        {
            string sql = @"

SELECT COUNT(DISTINCT fldDocumentType)

FROM tbdMemoAttachment

WHERE fldNo=@MemoNo

AND fldDocumentType IN
(
'INVOICE',
'BAP',
'FAKTURPAJAK'
)

";

            SqlCommand cmd = new(sql, conn, trans);

            cmd.Parameters.Add("@MemoNo", SqlDbType.NVarChar).Value = memoNo;

            int count = Convert.ToInt32(
                await cmd.ExecuteScalarAsync());

            return count == 3;
        }
        private async Task<bool> DetailExists(
            SqlConnection conn,
            SqlTransaction trans,
            string memoNo)
        {
            string sql = @"

SELECT COUNT(*)

FROM tbdMemoDetail

WHERE fldNo=@MemoNo

";

            SqlCommand cmd = new(sql, conn, trans);

            cmd.Parameters.Add("@MemoNo", SqlDbType.NVarChar).Value = memoNo;

            int count = Convert.ToInt32(
                await cmd.ExecuteScalarAsync());

            return count > 0;
        }

        private async Task<bool> MemoExists(
            SqlConnection conn,
            SqlTransaction trans,
            string memoNo)
        {
            string sql = @"

SELECT COUNT(*)

FROM tbdMemo

WHERE fldNo=@MemoNo

";

            SqlCommand cmd = new(sql, conn, trans);

            cmd.Parameters.Add("@MemoNo", SqlDbType.NVarChar).Value = memoNo;

            int count = Convert.ToInt32(
                await cmd.ExecuteScalarAsync());

            return count > 0;
        }

    }
}