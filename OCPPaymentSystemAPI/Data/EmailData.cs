using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using OCPPaymentSystemAPI.Helpers;
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

        private async Task<(List<string> To, List<string> Cc)>
GetRecipientAsync(
    SqlConnection conn,
    int templateID)
        {
            List<string> to = new();
            List<string> cc = new();

            string sql = @"

SELECT
    fldRecipientType,
    fldEmailAddress

FROM tbdEmailRecipient

WHERE fldTemplateID=@TemplateID
AND fldIsActive=1

";

            SqlCommand cmd = new(sql, conn);

            cmd.Parameters.Add("@TemplateID", SqlDbType.Int)
                .Value = templateID;

            using SqlDataReader dr =
                await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                string type =
                    dr["fldRecipientType"].ToString() ?? "";

                string email =
                    dr["fldEmailAddress"].ToString() ?? "";

                switch (type.ToUpper())
                {
                    case "TO":
                        to.Add(email);
                        break;

                    case "CC":
                        cc.Add(email);
                        break;
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

        public async Task SendSubmitEmailAsync(
            string memoNo,
            int approvalLevel)
        {
            using SqlConnection conn =
                _database.GetConnection();

            await conn.OpenAsync();

            var template =
                await GetTemplateAsync(
                    conn,
                    approvalLevel);

            var recipient =
                await GetRecipientAsync(
                    conn,
                    template.TemplateID);

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