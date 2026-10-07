using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using OCPPaymentSystemAPI.Helpers;
using OCPPaymentSystemAPI.Models;
using System.Data;

namespace OCPPaymentSystemAPI.Data
{
    public class EmailData
    {
        private readonly Database _database;
        private readonly EmailHelper _emailHelper;

        public EmailData(
            Database database,
            EmailHelper emailHelper)
        {
            _database = database;
            _emailHelper = emailHelper;
        }

        private async Task<(int TemplateID, string Subject, string Body)>
        GetTemplateAsync(
            SqlConnection conn,
            int approvalLevel)
        {
            string sql = @"

SELECT
    fldTemplateID,
    fldSubject,
    fldBody

FROM tbdEmailTemplate

WHERE fldApprovalLevel=@ApprovalLevel
AND fldIsActive=1

";

            SqlCommand cmd = new(sql, conn);

            cmd.Parameters.Add("@ApprovalLevel", SqlDbType.Int)
                .Value = approvalLevel;

            using SqlDataReader dr = await cmd.ExecuteReaderAsync();

            if (!await dr.ReadAsync())
                throw new Exception("Email template not found.");

            return
            (
                dr.GetInt32(0),
                dr["fldSubject"].ToString() ?? "",
                dr["fldBody"].ToString() ?? ""
            );
        }

        private async Task<(List<ApproverModel> To, List<ApproverModel> Cc)>
        GetRecipientAsync(SqlConnection conn, int intApprovalLevel, string strMillCode)
        {
            List<ApproverModel> to = new();
            List<ApproverModel> cc = new();

            string sql = @"EXEC sp_GetNextApprover @intApprovalLevel,@strMillCode";

            using SqlCommand cmd = new(sql, conn);
            cmd.Parameters.Add("@intApprovalLevel", SqlDbType.Int).Value = intApprovalLevel;
            cmd.Parameters.Add("@strMillCode", SqlDbType.NVarChar).Value = strMillCode;

            using SqlDataReader dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                var approver = new ApproverModel
                {
                    Employee_ID = dr["Employee_ID"].ToString() ?? "",
                    Employee_Name = dr["Employee_Name"].ToString() ?? "",
                    Business_Title = dr["Business_Title"].ToString() ?? "",
                    Email = dr["Email"].ToString() ?? ""
                };

                if (!string.IsNullOrWhiteSpace(approver.Email))
                {
                    to.Add(approver);
                    cc.Add(approver);
                }
            }

            return (to, cc);
        }

        private async Task<string> GetMillManagerEmailAsync(SqlConnection conn, string memoNo)
        {
            string sql = @"
SELECT TOP 1 mp.Email
FROM tbdApproval a
JOIN [CentralAuthentication].dbo.tblManPower mp
    ON a.fldApprovedBy COLLATE Latin1_General_CI_AI=mp.Employee_ID COLLATE Latin1_General_CI_AI
WHERE a.fldNo=@MemoNo
AND a.fldApprovalLevel=10
ORDER BY mp.Period DESC";

            SqlCommand cmd = new(sql, conn);
            cmd.Parameters.Add("@MemoNo", SqlDbType.NVarChar).Value = memoNo;

            object result = await cmd.ExecuteScalarAsync();
            return result?.ToString() ?? "";
        }

        private async Task UpdateEmailSentAsync(
            SqlConnection conn,
            string memoNo,
            int approvalLevel)
        {
            string sql = @"
UPDATE tbdApproval
SET fldEmailSentOn=GETDATE()
WHERE fldNo=@MemoNo
AND fldApprovalLevel=@ApprovalLevel
";

            SqlCommand cmd = new(sql, conn);

            cmd.Parameters.Add("@MemoNo",
                SqlDbType.NVarChar).Value = memoNo;

            cmd.Parameters.Add("@ApprovalLevel",
                SqlDbType.Int).Value = approvalLevel;

            await cmd.ExecuteNonQueryAsync();
        }

        private string ReplaceTemplate(
            string body,
            string memoNo)
        {
            body = body.Replace("{MemoNo}", memoNo);

            body = body.Replace("{SystemDate}",
                DateTime.Now.ToString("dd-MMM-yyyy HH:mm"));

            return body;
        }

        public async Task SendApproverEmailAsync(MemoApproveRequest request)
        {
            using SqlConnection conn =
                _database.GetConnection();

            await conn.OpenAsync();

            //cari dulu approval level dan mill code dari memo no
            string memoNo = request.MemoNo;

            var template =
                await GetTemplateAsync(
                    conn,
                    request.ApprovalLevel);

            var recipient =
                await GetRecipientAsync(
                    conn,
                    request.ApprovalLevel,
                    "M412");

            string body =
                ReplaceTemplate(
                    template.Body,
                    memoNo);

            await _emailHelper.SendAsync(
                recipient.To,
                recipient.Cc,
                template.Subject,
                body);

            await UpdateEmailSentAsync(
                conn,
                memoNo,
                approvalLevel);
        }
    }
}