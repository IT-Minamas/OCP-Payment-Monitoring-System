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

        private async Task<(List<ApproverModel> To, List<ApproverModel> Cc)>
        GetApprovedEmailAsync(SqlConnection conn, string strMemoNo)
        {
            List<ApproverModel> to = new();
            List<ApproverModel> cc = new();

            string sql = @"EXEC sp_GetNextApprover @strMemoNo";
            using SqlCommand cmd = new(sql, conn);
            cmd.Parameters.Add("@strMemoNo", SqlDbType.NVarChar).Value = strMemoNo;
            using SqlDataReader dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                var approver = new ApproverModel
                {
                    Employee_ID = dr["Employee_ID"].ToString() ?? "",
                    Employee_Name = dr["Employee_Name"].ToString() ?? "",
                    Business_Title = dr["Business_Title"].ToString() ?? ""
                };

                //If Production server, use the actual email address from the database; otherwise, use a test email address for non-production environments.
                if (AppEnvironment.IsProductionServer())
                {
                    approver.Email = dr["Email"].ToString() ?? "";
                    //approver.Email = "rizky.y.pratomo@gmail.com";
                }
                else
                {
                    approver.Email = "rizky.y.pratomo@sdguthrie.com";
                }


                if (!string.IsNullOrWhiteSpace(approver.Email))
                {
                    to.Add(approver);
                }
            }
            dr.Close();

            sql = @"EXEC sp_GetRequester @strMemoNo";
            using SqlCommand cmd1 = new(sql, conn);
            cmd1.Parameters.Add("@strMemoNo", SqlDbType.NVarChar).Value = strMemoNo;
            using SqlDataReader dr1 = await cmd1.ExecuteReaderAsync();

            while (await dr1.ReadAsync())
            {
                var approver = new ApproverModel
                {
                    Employee_ID = dr1["Employee_ID"].ToString() ?? "",
                    Employee_Name = dr1["Employee_Name"].ToString() ?? "",
                    Business_Title = dr1["Business_Title"].ToString() ?? ""
                };

                //If Production server, use the actual email address from the database; otherwise, use a test email address for non-production environments.
                if (AppEnvironment.IsProductionServer())
                {
                    approver.Email = dr1["Email"].ToString() ?? "";
                }
                else
                {
                    approver.Email = "rizky.y.pratomo@sdguthrie.com";
                }

                if (!string.IsNullOrWhiteSpace(approver.Email))
                {
                    cc.Add(approver);
                }
            }

            return (to, cc);
        }

        private async Task<(List<ApproverModel> To, List<ApproverModel> Cc)>
GetRejectedEmailAsync(SqlConnection conn, string strMemoNo)
        {
            List<ApproverModel> to = new();
            List<ApproverModel> cc = new();

            string sql = @"EXEC sp_GetRequester @strMemoNo";

            using SqlCommand cmd = new(sql, conn);
            cmd.Parameters.Add("@strMemoNo", SqlDbType.NVarChar).Value = strMemoNo;

            using SqlDataReader dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                var approver = new ApproverModel
                {
                    Employee_ID = dr["Employee_ID"].ToString() ?? "",
                    Employee_Name = dr["Employee_Name"].ToString() ?? "",
                    Business_Title = dr["Business_Title"].ToString() ?? ""
                };

                //If Production server, use the actual email address from the database; otherwise, use a test email address for non-production environments.
                if (AppEnvironment.IsProductionServer())
                {
                    approver.Email = dr["Email"].ToString() ?? "";
                    //approver.Email = "rizky.y.pratomo@gmail.com";
                }
                else
                {
                    approver.Email = "rizky.y.pratomo@sdguthrie.com";
                }


                if (!string.IsNullOrWhiteSpace(approver.Email))
                {
                    to.Add(approver);
                    //cc.Add(approver);
                }
            }

            return (to, cc);
        }

        private async Task UpdateEmailSentAsync(
            SqlConnection conn,
            string memoNo)
        {
            string sql = @"
                UPDATE a
                SET a.fldEmailSentOn = GETDATE()
                FROM tbdApproval AS a
                JOIN vw_SearchMemo AS b ON b.fldNo = a.fldNo AND b.ApprovalLevel = a.fldApprovalLevel
                WHERE a.fldNo = @MemoNo;
            ";

            SqlCommand cmd = new(sql, conn);
            cmd.Parameters.Add("@MemoNo", SqlDbType.NVarChar).Value = memoNo;

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task SendApproverEmailAsync(MemoApproveRequest request)
        {
            using SqlConnection conn =
                _database.GetConnection();

            await conn.OpenAsync();

            string memoNo = request.MemoNo;

            var recipient = await GetApprovedEmailAsync(conn,request.MemoNo);

            string template = @"
<p>Yth. [Nama Approver],</p>

<p>Terdapat permohonan pembayaran OCP kepada <b>[Nama Supplier]</b> yang menunggu pemeriksaan dan persetujuan Bp/Ibu.</p>

<p>Mohon agar permohonan tersebut dapat segera ditinjau dan disetujui agar dapat dilanjutkan ke tahapan persetujuan berikutnya hingga proses pembayaran.</p>

<p><b>Informasi Pembayaran:</b></p>
<ul>
    <li>Pabrik: [Pabrik]</li>
    <li>Tanggal Memo: [Tanggal Memo]</li>
    <li>Nomor Memo: [MemoNo]</li>
    <li>Supplier: [Nama Supplier]</li>
    <li>Nomor Invoice: [Nomor Invoice]</li>
    <li>Jumlah: Rp. [Jumlah]</li>
</ul>

<p>Silahkan masuk ke <a href=https://ocppayment.sdguthrie.com/Memo>link ini</a> (untuk jaringan internal/kantor SD Guthrie), atau <a href=https://ocppayment.minamasplantation.com/Memo>link ini</a> (untuk jaringan internet di luar SD Guthrie), untuk meninjau permohonan pembayaran.</p>

<p>Terima kasih atas perhatian dan kerja samanya.</p>

<p>Hormat kami,<br>
<b>OCP Payment Monitoring System</b></p>

<hr>
<p><small><i>Email ini dikirim secara otomatis. Mohon tidak membalas email ini.</i></small></p>
";

            template = template.Replace("[MemoNo]", memoNo);

            string approverName = "";
            foreach (var item in recipient.To)
            {
                approverName += "Bp/Ibu " + (string.IsNullOrWhiteSpace(item.Employee_Name) ? "Approver" : item.Employee_Name);
                //approverName += string.IsNullOrWhiteSpace(item.Business_Title) ? "Business Title" : item.Business_Title;
                approverName += ", ";
            }

            approverName = approverName.TrimEnd(',', ' ');
            template = template.Replace("[Nama Approver]", approverName);

            var memo = await GetMemoEmailDataAsync(conn, request.MemoNo);
            if (memo == null) throw new InvalidOperationException($"Memo {request.MemoNo} tidak ditemukan.");

            template = template.Replace("[Nama Supplier]", memo.fldSupplierName)
                .Replace("[Tanggal Memo]", memo.fldDate?.ToString("dd-MMM-yyyy", System.Globalization.CultureInfo.GetCultureInfo("en-US")) ?? "")
                .Replace("[Jumlah]", memo.fldAmount.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("id-ID")))
                .Replace("[Pabrik]", memo.MillAbbv)
                .Replace("[Nomor Invoice]", memo.fldInvoice);

            if (recipient.To != null && recipient.To.Count > 0)
            {
                await _emailHelper.SendAsync(
                recipient.To,
                recipient.Cc,
                "Persetujuan Pembayaran OCP – " + memoNo,
                template);
            }

            await UpdateEmailSentAsync(
                conn,
                memoNo);
        }

        public async Task SendRejectedEmailAsync(MemoRejectRequest request)
        {
            using SqlConnection conn =
                _database.GetConnection();

            await conn.OpenAsync();

            string memoNo = request.MemoNo;

            var recipient = await GetRejectedEmailAsync(conn, request.MemoNo);

            string template = @"
<p>Yth. [Nama Pemohon],</p>

<p>Kami informasikan bahwa memo pengajuan pembayaran OCP yang Bp/Ibu ajukan telah ditolak.</p>

<p>Berikut adalah informasi terkait pengajuan tersebut:</p>

<p><b>Informasi Pembayaran:</b></p>
<ul>
    <li>Pabrik: [Pabrik]</li>
    <li>Tanggal Memo: [Tanggal Memo]</li>
    <li>Nomor Memo: [MemoNo]</li>
    <li>Supplier: [Nama Supplier]</li>
    <li>Nomor Invoice: [Nomor Invoice]</li>
    <li>Jumlah: Rp. [Jumlah]</li>
</ul>

<p>Mohon agar Bp/Ibu dapat meninjau alasan penolakan tersebut dan mengajukan kembali memo untuk proses persetujuan.</p>

<p>Silahkan masuk ke <a href=https://ocppayment.sdguthrie.com/Memo>link ini</a> (untuk jaringan internal/kantor SD Guthrie), atau <a href=https://ocppayment.minamasplantation.com/Memo>link ini</a> (untuk jaringan internet di luar SD Guthrie), untuk meninjau penolakannya.</p>

<p>Terima kasih atas perhatian dan kerja samanya.</p>

<p>Hormat kami,<br>
<b>OCP Payment Monitoring System</b></p>

<hr>
<p><small><i>Email ini dikirim secara otomatis. Mohon tidak membalas email ini.</i></small></p>
";

            template = template.Replace("[MemoNo]", memoNo);

            string approverName = "";
            foreach (var item in recipient.To)
            {
                approverName += "Bp/Ibu " + (string.IsNullOrWhiteSpace(item.Employee_Name) ? "Approver" : item.Employee_Name);
                approverName += ", ";
            }

            approverName = approverName.TrimEnd(',', ' ');
            template = template.Replace("[Nama Pemohon]", approverName);

            var memo = await GetMemoEmailDataAsync(conn, request.MemoNo);
            if (memo == null) throw new InvalidOperationException($"Memo {request.MemoNo} tidak ditemukan.");

            template = template.Replace("[Nama Supplier]", memo.fldSupplierName)
                .Replace("[Tanggal Memo]", memo.fldDate?.ToString("dd-MMM-yyyy", System.Globalization.CultureInfo.GetCultureInfo("en-US")) ?? "")
                .Replace("[Jumlah]", memo.fldAmount.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("id-ID")))
                .Replace("[Pabrik]", memo.MillAbbv)
                .Replace("[Nomor Invoice]", memo.fldInvoice);

            await _emailHelper.SendAsync(
                recipient.To,
                recipient.Cc,
                "Pemberitahuan Penolakan Memo Pembayaran OCP – " + memoNo,
                template);

            await UpdateEmailSentAsync(
                conn,
                memoNo);
        }

        private async Task<MemoEmailModel?> GetMemoEmailDataAsync(SqlConnection conn, string memoNo)
        {
            string sql = @"
        SELECT fldSupplierName, fldDate, fldAmount, MillAbbv, fldInvoice
        FROM vw_SearchMemo
        WHERE fldNo = @MemoNo";

            using SqlCommand cmd = new(sql, conn);
            cmd.Parameters.Add("@MemoNo", SqlDbType.NVarChar).Value = memoNo;

            using SqlDataReader dr = await cmd.ExecuteReaderAsync();

            if (!await dr.ReadAsync())
                return null;

            return new MemoEmailModel
            {
                fldSupplierName = dr["fldSupplierName"]?.ToString() ?? "",
                fldDate = dr["fldDate"] == DBNull.Value ? null : Convert.ToDateTime(dr["fldDate"]),
                fldAmount = dr["fldAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["fldAmount"]),
                MillAbbv = dr["MillAbbv"]?.ToString() ?? "",
                fldInvoice = dr["fldInvoice"]?.ToString() ?? ""
            };
        }

    }
}