using Microsoft.Data.SqlClient;
using OCPPaymentSystemAPI.Helpers;
using OCPPaymentSystemAPI.Models;
using OCPPaymentSystemAPI.Models.Workflow;
using QuestPDF.Fluent;
using System.Data;
using OCPPaymentSystemAPI.Documents;
using System.Threading.Tasks;

using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace OCPPaymentSystemAPI.Data
{
    public class MemoData
    {
        private readonly Database _database;
        private readonly RunningNumberHelper _runningNumber;
        private readonly EmailData _emailData;

        public MemoData(
            Database database,
            RunningNumberHelper runningNumber,
            EmailData emailData)
        {
            _database = database;
            _runningNumber = runningNumber;
            _emailData = emailData;
        }

        private async Task<byte[]> GenerateMemoPdfAsync(
            SqlConnection conn,
            SqlTransaction trans,
            string memoNo,
            string approvedBy)
        {
            MemoPdfModel model =
                await GetMemoPdfDataAsync(
                    conn,
                    trans,
                    memoNo);

            model.ApprovedBy = approvedBy;
            model.ApprovedOn = DateTime.Now;

            MemoPdfGenerator pdf =
                new MemoPdfGenerator();

            return pdf.Generate(model);
        }

        private async Task SaveMemoPdfAttachmentAsync(
            SqlConnection conn,
            SqlTransaction trans,
            string memoNo,
            byte[] pdf,
            string userName)
        {
            string sql = @"

DELETE
FROM tbdMemoAttachment
WHERE fldNo=@MemoNo
AND fldDocumentType='Memo'

INSERT INTO tbdMemoAttachment
(
    fldNo,
    fldDocumentType,
    fldOriginalFileName,
    fldContentType,
    fldFileSize,
    fldFile,
    fldCreatedBy,
    fldCreatedOn
)
VALUES
(
    @MemoNo,
    'Memo',
    @FileName,
    @ContentType,
    @FileSize,
    @File,
    @CreatedBy,
    GETDATE()
)

";

            SqlCommand cmd =
                new SqlCommand(sql, conn, trans);

            cmd.Parameters.Add("@MemoNo", SqlDbType.NVarChar).Value = memoNo;
            cmd.Parameters.Add("@FileName", SqlDbType.NVarChar).Value = memoNo + ".pdf";
            cmd.Parameters.Add("@ContentType", SqlDbType.NVarChar).Value = "application/pdf";
            cmd.Parameters.Add("@FileSize", SqlDbType.BigInt).Value = pdf.Length;
            cmd.Parameters.Add("@File", SqlDbType.VarBinary).Value = pdf;
            cmd.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = userName;

            await cmd.ExecuteNonQueryAsync();
        }

        private async Task<MemoPdfModel> GetMemoPdfDataAsync(
            SqlConnection conn,
            SqlTransaction trans,
            string memoNo)
        {
            MemoPdfModel result = new();

            string sql = @"

SELECT
    m.fldNo MemoNo,
    m.fldDate MemoDate,
    c.fldName CompanyName,
    m.fldSupplierCode SupplierCode,
    s.fldName SupplierName,
    m.fldInvoice InvoiceNo,
    m.fldAmount Amount,
    m.fldRemarks Remarks,
    s.fldBankName,
    s.fldBankAccountNo,
    s.fldNameOnBankAccount,
    m.fldCreatedBy,
    m.fldCreatedOn
FROM tbdMemo m
INNER JOIN vw_Company c
    ON m.fldCompanyCode COLLATE Latin1_General_CI_AI=c.fldCode COLLATE Latin1_General_CI_AI
INNER JOIN vw_Supplier s
    ON m.fldSupplierCode COLLATE Latin1_General_CI_AI=s.fldCode COLLATE Latin1_General_CI_AI
WHERE m.fldNo=@MemoNo

";

            SqlCommand cmd =
                new SqlCommand(sql, conn, trans);

            cmd.Parameters.Add("@MemoNo", SqlDbType.NVarChar).Value = memoNo;

            SqlDataReader dr =
                await cmd.ExecuteReaderAsync();

            if (await dr.ReadAsync())
            {
                result.MemoNo = dr["MemoNo"].ToString() ?? "";
                result.MemoDate = Convert.ToDateTime(dr["MemoDate"]);
                result.CompanyName = dr["CompanyName"].ToString() ?? "";
                result.SupplierCode = dr["SupplierCode"].ToString() ?? "";
                result.SupplierName = dr["SupplierName"].ToString() ?? "";
                result.InvoiceNo = dr["InvoiceNo"].ToString() ?? "";
                result.Amount = Convert.ToDecimal(dr["Amount"]);
                result.Remarks = dr["Remarks"].ToString() ?? "";
                result.BankName = dr["fldBankName"].ToString() ?? "";
                result.AccountNo = dr["fldBankAccountNo"].ToString() ?? "";
                result.AccountName = dr["fldNameOnBankAccount"].ToString() ?? "";
                result.CreatedBy = dr["fldCreatedBy"].ToString() ?? "";
                result.CreatedOn = Convert.ToDateTime(dr["fldCreatedOn"]);
            }

            await dr.CloseAsync();

            return result;
        }

        public async Task<List<MemoDetailModel>>
        GetMemoDetailAsync(string memoNo)
        {

            List<MemoDetailModel> list = new();


            using SqlConnection con =
                _database.GetConnection();


            string sql =
            @"
SELECT
    a.fldNo,
    a.fldMillCode,
    a.fldTicketNo,
	b.PostDate,
	b.LORRY_NO,
	b.DRIVER_CODE,
	b.SERIAL_NO,
	b.BUNCH_WEIGHT,
	b.WEIGHT_IN,
	b.WEIGHT_OUT,
	b.NETT_WEIGHT,
	b.SUPPLIER_CODE,
	b.DELIVERY_ORDER_NO,
	b.DED_WT
FROM tbdMemoDetail AS a
JOIN vw_SDGWeighCheck AS b ON a.fldMillCode COLLATE SQL_Latin1_General_CP1_CI_AS=b.fldMillCode COLLATE SQL_Latin1_General_CP1_CI_AS AND a.fldTicketNo=b.SERIAL_NO
WHERE a.fldNo=@MemoNo
    ";

            using SqlCommand cmd =
                new(sql, con);

            cmd.Parameters.AddWithValue(
                "@MemoNo",
                memoNo);

            await con.OpenAsync();

            SqlDataReader dr =
                await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                list.Add(
                new MemoDetailModel
                {
                    MemoNo = dr["fldNo"].ToString(),
                    MillCode = dr["fldMillCode"].ToString(),
                    TicketNo = dr["fldTicketNo"].ToString(),

                    PostDate =
                        Convert.ToDateTime(
                            dr["PostDate"]),
                    LorryNo =
                        dr["LORRY_NO"]
                        .ToString(),
                    DriverCode =
                        dr["DRIVER_CODE"]
                        .ToString(),
                    SerialNo =
                        dr["SERIAL_NO"]
                        .ToString(),
                    BunchWeight =
                        dr["BUNCH_WEIGHT"] == DBNull.Value
                        ? 0
                        : Convert.ToDecimal(
                            dr["BUNCH_WEIGHT"]),
                    WeightIn =
                        dr["WEIGHT_IN"] == DBNull.Value
                        ? 0
                        : Convert.ToDecimal(
                            dr["WEIGHT_IN"]),
                    WeightOut =
                        dr["WEIGHT_OUT"] == DBNull.Value
                        ? 0
                        : Convert.ToDecimal(
                            dr["WEIGHT_OUT"]),
                    NettWeight =
                        dr["NETT_WEIGHT"] == DBNull.Value
                        ? 0
                        : Convert.ToDecimal(
                            dr["NETT_WEIGHT"]),
                    SupplierCode =
                        dr["SUPPLIER_CODE"]
                        .ToString(),
                    DELIVERY_ORDER_NO =
                        dr["DELIVERY_ORDER_NO"] == DBNull.Value
                        ? ""
                        : dr["DELIVERY_ORDER_NO"].ToString(),
                    DED_WT =
                        dr["DED_WT"] == DBNull.Value
                        ? 0
                        : Convert.ToDecimal(
                            dr["DED_WT"])
                });

            }


            return list;

        }

        public async Task<List<SDGWeighModel>>
        GetSDGWeighAsync(
            SDGWeighRequest request)
        {

            List<SDGWeighModel> list = new();

            using SqlConnection con =
                _database.GetConnection();

            string sql =
            @"
        SELECT
            a.PostDate,
            a.LORRY_NO,
            a.DRIVER_CODE,
            a.SERIAL_NO,
            a.BUNCH_WEIGHT,
            a.WEIGHT_IN,
            a.WEIGHT_OUT,
            a.NETT_WEIGHT,
            a.SUPPLIER_CODE,
            a.fldMillCode,
            a.DELIVERY_ORDER_NO,
            a.DED_WT,
            b.fldNo AS MemoNo
        FROM vw_SDGWeighCheck AS a
        LEFT JOIN tbdMemoDetail AS b ON a.SERIAL_NO=b.fldTicketNo AND a.fldMillCode COLLATE SQL_Latin1_General_CP1_CI_AS = b.fldMillCode COLLATE SQL_Latin1_General_CP1_CI_AS
        WHERE
            a.PostDate >= @DateFrom AND a.PostDate <= @DateTo
    ";

            if (!string.IsNullOrWhiteSpace(
                request.SupplierCode))
            {
                sql +=
                @"
            AND a.SUPPLIER_CODE = @SupplierCode
        ";
            }
            if (!string.IsNullOrWhiteSpace(
                request.MillCode))
            {
                sql +=
                @"
            AND a.fldMillCode = @MillCode
        ";
            }

            sql +=
            @"
        ORDER BY PostDate
    ";

            using SqlCommand cmd =
                new(sql, con);

            cmd.Parameters.AddWithValue(
                "@DateFrom",
                request.DateFrom);
            cmd.Parameters.AddWithValue(
                "@DateTo",
                request.DateTo);
            if (!string.IsNullOrWhiteSpace(
                request.SupplierCode))
            {
                cmd.Parameters.AddWithValue(
                    "@SupplierCode",
                    request.SupplierCode);
            }
            if (!string.IsNullOrWhiteSpace(
                request.MillCode))
            {
                cmd.Parameters.AddWithValue(
                    "@MillCode",
                    request.MillCode);
            }

            await con.OpenAsync();

            var dr =
                await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                list.Add(
                new SDGWeighModel
                {
                    PostDate =
                        Convert.ToDateTime(
                            dr["PostDate"]),
                    LorryNo =
                        dr["LORRY_NO"]
                        .ToString(),
                    DriverCode =
                        dr["DRIVER_CODE"]
                        .ToString(),
                    SerialNo =
                        dr["SERIAL_NO"]
                        .ToString(),
                    BunchWeight =
                        dr["BUNCH_WEIGHT"] == DBNull.Value
                        ? 0
                        : Convert.ToDecimal(
                            dr["BUNCH_WEIGHT"]),
                    WeightIn =
                        dr["WEIGHT_IN"] == DBNull.Value
                        ? 0
                        : Convert.ToDecimal(
                            dr["WEIGHT_IN"]),
                    WeightOut =
                        dr["WEIGHT_OUT"] == DBNull.Value
                        ? 0
                        : Convert.ToDecimal(
                            dr["WEIGHT_OUT"]),
                    NettWeight =
                        dr["NETT_WEIGHT"] == DBNull.Value
                        ? 0
                        : Convert.ToDecimal(
                            dr["NETT_WEIGHT"]),
                    SupplierCode =
                        dr["SUPPLIER_CODE"]
                        .ToString(),
                    MillCode =
                        dr["fldMillCode"]
                        .ToString(),
                    DELIVERY_ORDER_NO =
                        dr["DELIVERY_ORDER_NO"] == DBNull.Value
                        ? ""
                        : dr["DELIVERY_ORDER_NO"].ToString(),
                    DED_WT =
                        dr["DED_WT"] == DBNull.Value
                        ? 0
                        : Convert.ToDecimal(
                            dr["DED_WT"]),
                    MemoNo =
                        dr["MemoNo"] == DBNull.Value
                        ? ""
                        : dr["MemoNo"].ToString()
                });
            }

            return list;

        }

        public async Task<bool>
        SaveSDGWeighAsync(
            SDGWeighCheckRequest request)
        {

            using SqlConnection con =
                _database.GetConnection();


            await con.OpenAsync();


            SqlTransaction tran =
                con.BeginTransaction();


            try
            {

                //
                // DELETE OLD DETAIL
                //
                string deleteSql =
                @"
        DELETE FROM tbdMemoDetail
        WHERE fldNo=@MemoNo
        ";


                using (SqlCommand cmd =
                    new(deleteSql, con, tran))
                {

                    cmd.Parameters.AddWithValue(
                        "@MemoNo",
                        request.MemoNo);


                    await cmd.ExecuteNonQueryAsync();

                }



                //
                // INSERT NEW DETAIL
                //
                foreach (var ticket in request.Tickets)
                {

                    string insertSql =
                    @"
            INSERT INTO tbdMemoDetail
            (
                fldNo,
                fldMillCode,
                fldTicketNo
            )
            VALUES
            (
                @MemoNo,
                @MillCode,
                @Ticket
            )
            ";


                    using SqlCommand cmd =
                        new(
                            insertSql,
                            con,
                            tran);


                    cmd.Parameters.AddWithValue(
                        "@MemoNo",
                        request.MemoNo);


                    cmd.Parameters.AddWithValue(
                        "@MillCode",
                        request.MillCode);


                    cmd.Parameters.AddWithValue(
                        "@Ticket",
                        ticket);


                    await cmd.ExecuteNonQueryAsync();

                }


                tran.Commit();


                return true;

            }
            catch
            {

                tran.Rollback();

                throw;

            }

        }

        public async Task<bool> DeleteAttachmentAsync(
            string memoNo,
            string documentType)
        {

            using SqlConnection conn =
                _database.GetConnection();


            await conn.OpenAsync();


            string sql =
            @"
    DELETE FROM tbdMemoAttachment
    WHERE fldNo=@MemoNo
    AND fldDocumentType=@DocumentType
    ";


            SqlCommand cmd =
                new(sql, conn);


            cmd.Parameters.Add(
                "@MemoNo",
                SqlDbType.NVarChar)
                .Value = memoNo;


            cmd.Parameters.Add(
                "@DocumentType",
                SqlDbType.NVarChar)
                .Value = documentType;


            int row =
                await cmd.ExecuteNonQueryAsync();


            return row > 0;
        }

        public async Task<MemoAttachmentDownload?> DownloadAttachmentAsync(
            string memoNo,
            string documentType)
        {

            using SqlConnection conn =
                _database.GetConnection();


            await conn.OpenAsync();


            string sql =
            @"
    SELECT
        fldOriginalFileName,
        fldContentType,
        fldFile
    FROM tbdMemoAttachment
    WHERE fldNo=@MemoNo
    AND fldDocumentType=@DocumentType
    ";


            SqlCommand cmd =
                new(sql, conn);


            cmd.Parameters.Add(
                "@MemoNo",
                SqlDbType.NVarChar)
                .Value = memoNo;


            cmd.Parameters.Add(
                "@DocumentType",
                SqlDbType.NVarChar)
                .Value = documentType;


            SqlDataReader dr =
                await cmd.ExecuteReaderAsync();


            if (!await dr.ReadAsync())
                return null;


            return new MemoAttachmentDownload
            {
                FileName =
                    dr["fldOriginalFileName"].ToString() ?? "",

                ContentType =
                    dr["fldContentType"].ToString() ?? "",

                FileData =
                    (byte[])dr["fldFile"]
            };
        }

        public async Task<List<MemoAttachmentModel>>
            GetAttachmentListAsync(string memoNo)
        {
            List<MemoAttachmentModel> result = new();


            using SqlConnection conn =
                _database.GetConnection();


            await conn.OpenAsync();


            string sql =
            @"
    SELECT
        fldDocumentType,
        fldOriginalFileName,
        fldFileSize,
        fldCreatedOn,
        fldCreatedBy
    FROM tbdMemoAttachment
    WHERE fldNo=@MemoNo
    ";


            using SqlCommand cmd =
                new SqlCommand(sql, conn);


            cmd.Parameters.AddWithValue(
                "@MemoNo",
                memoNo);


            using SqlDataReader dr =
                await cmd.ExecuteReaderAsync();


            while (await dr.ReadAsync())
            {
                result.Add(
                    new MemoAttachmentModel
                    {
                        DocumentType =
                            dr["fldDocumentType"]
                            .ToString() ?? "",


                        FileName =
                            dr["fldOriginalFileName"]
                            .ToString() ?? "",


                        FileSize =
                            Convert.ToInt64(
                                dr["fldFileSize"]),


                        UploadOn =
                            dr["fldCreatedOn"]
                            == DBNull.Value
                            ? null
                            : Convert.ToDateTime(
                                dr["fldCreatedOn"]),


                        UploadedBy =
                            dr["fldCreatedBy"]
                            .ToString() ?? ""
                    });
            }


            return result;
        }

        public async Task<bool> UploadAttachmentAsync(
            MemoAttachmentRequest request)
        {

            if (request.file == null)
                throw new Exception("File is empty");


            byte[] fileBytes;


            using (var ms = new MemoryStream())
            {
                await request.file.CopyToAsync(ms);

                fileBytes = ms.ToArray();
            }


            using SqlConnection conn =
                _database.GetConnection();


            await conn.OpenAsync();


            string sql =
            @"
    DELETE FROM tbdMemoAttachment
    WHERE fldNo=@MemoNo
    AND fldDocumentType=@DocumentType


    INSERT INTO tbdMemoAttachment
    (
        fldNo,
        fldDocumentType,
        fldOriginalFileName,
        fldContentType,
        fldFileSize,
        fldFile,
        fldCreatedBy,
        fldCreatedOn
    )
    VALUES
    (
        @MemoNo,
        @DocumentType,
        @FileName,
        @ContentType,
        @FileSize,
        @File,
        @CreatedBy,
        GETDATE()
    )
    ";


            SqlCommand cmd =
                new SqlCommand(sql, conn);


            cmd.Parameters.Add(
                "@MemoNo",
                SqlDbType.NVarChar)
                .Value = request.memoNo;


            cmd.Parameters.Add(
                "@DocumentType",
                SqlDbType.NVarChar)
                .Value = request.type;


            cmd.Parameters.Add(
                "@FileName",
                SqlDbType.NVarChar)
                .Value = request.file.FileName;


            cmd.Parameters.Add(
                "@ContentType",
                SqlDbType.NVarChar)
                .Value = request.file.ContentType;


            cmd.Parameters.Add(
                "@FileSize",
                SqlDbType.BigInt)
                .Value = request.file.Length;


            cmd.Parameters.Add(
                "@File",
                SqlDbType.VarBinary)
                .Value = fileBytes;


            cmd.Parameters.Add(
                "@CreatedBy",
                SqlDbType.NVarChar)
                .Value = "SYSTEM";


            await cmd.ExecuteNonQueryAsync();


            return true;
        }
        public async Task<MemoResponse?> GetByNoAsync(string memoNo)
        {
            using SqlConnection conn = _database.GetConnection();
            await conn.OpenAsync();

            string sql = @"
SELECT
    fldNo,
    fldCompanyCode,
    fldSupplierCode,
    fldDate,
    fldAmount,
    fldRemarks,
    fldCreatedBy,
    fldCreatedOn,
    fldMillCode,
    ApprovalLevel
FROM vw_SearchMemo
WHERE fldNo=@MemoNo
";

            SqlCommand cmd = new(sql, conn);
            cmd.Parameters.Add("@MemoNo", SqlDbType.NVarChar).Value = memoNo;
            SqlDataReader dr = await cmd.ExecuteReaderAsync();
            if (await dr.ReadAsync())
            {
                return new MemoResponse
                {
                    MemoNo = dr["fldNo"].ToString() ?? "",
                    CompanyCode = dr["fldCompanyCode"].ToString() ?? "",
                    SupplierCode = dr["fldSupplierCode"].ToString() ?? "",
                    MemoDate = Convert.ToDateTime(dr["fldDate"]),
                    Amount = Convert.ToDecimal(dr["fldAmount"]),
                    Remarks = dr["fldRemarks"].ToString() ?? "",
                    CreatedBy = dr["fldCreatedBy"].ToString() ?? "",
                    CreatedOn = Convert.ToDateTime(dr["fldCreatedOn"]),
                    MillCode = dr["fldMillCode"].ToString() ?? "",
                    ApprovalLevel = Convert.ToInt32(dr["ApprovalLevel"])
                };
            }
            return null;
        }

        public async Task<bool> UpdateAsync(MemoUpdateRequest request)
        {
            using SqlConnection conn = _database.GetConnection();

            await conn.OpenAsync();

            string sql = @"

UPDATE tbdMemo

SET

fldCompanyCode=@CompanyCode,

fldSupplierCode=@SupplierCode,

fldDate=@MemoDate,

fldAmount=@Amount,

fldRemarks=@Remarks,

fldUpdatedBy=@UpdatedBy,

fldUpdatedOn=GETDATE(),

fldUpdatedIP=@UpdatedIP

WHERE

fldNo=@MemoNo

";

            SqlCommand cmd = new(sql, conn);

            cmd.Parameters.Add("@CompanyCode", SqlDbType.NVarChar).Value = request.CompanyCode;

            cmd.Parameters.Add("@SupplierCode", SqlDbType.NVarChar).Value = request.SupplierCode;

            cmd.Parameters.Add("@MemoDate", SqlDbType.Date).Value = request.MemoDate;

            cmd.Parameters.Add("@Amount", SqlDbType.Decimal).Value = request.Amount;
            cmd.Parameters["@Amount"].Precision = 18;
            cmd.Parameters["@Amount"].Scale = 2;

            cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar).Value = request.Remarks ?? "";

            cmd.Parameters.Add("@UpdatedBy", SqlDbType.NVarChar).Value = request.UserName;

            cmd.Parameters.Add("@UpdatedIP", SqlDbType.NVarChar).Value = request.UserIP;

            cmd.Parameters.Add("@MemoNo", SqlDbType.NVarChar).Value = request.MemoNo;

            int row = await cmd.ExecuteNonQueryAsync();

            return row > 0;
        }

        public async Task<bool> DeleteAsync(string memoNo)
        {
            using SqlConnection conn = _database.GetConnection();

            await conn.OpenAsync();

            SqlTransaction trans = conn.BeginTransaction();

            try
            {
                SqlCommand cmd;

                // ===========================================
                // Delete Approval
                // ===========================================

                string sql = @"
DELETE FROM tbdApproval
WHERE fldNo=@MemoNo
";

                cmd = new SqlCommand(sql, conn, trans);
                cmd.Parameters.Add("@MemoNo", SqlDbType.NVarChar).Value = memoNo;
                await cmd.ExecuteNonQueryAsync();

                // ===========================================
                // Delete Attachment
                // ===========================================

                sql = @"
DELETE FROM tbdMemoAttachment
WHERE fldNo=@MemoNo
";

                cmd = new SqlCommand(sql, conn, trans);
                cmd.Parameters.Add("@MemoNo", SqlDbType.NVarChar).Value = memoNo;
                await cmd.ExecuteNonQueryAsync();

                // ===========================================
                // Delete Memo Detail
                // ===========================================

                sql = @"
DELETE FROM tbdMemoDetail
WHERE fldNo=@MemoNo
";

                cmd = new SqlCommand(sql, conn, trans);
                cmd.Parameters.Add("@MemoNo", SqlDbType.NVarChar).Value = memoNo;
                await cmd.ExecuteNonQueryAsync();

                // ===========================================
                // Delete Memo Log
                // ===========================================

                sql = @"
DELETE FROM tbdMemoLog
WHERE fldNo=@MemoNo
";

                cmd = new SqlCommand(sql, conn, trans);
                cmd.Parameters.Add("@MemoNo", SqlDbType.NVarChar).Value = memoNo;
                await cmd.ExecuteNonQueryAsync();

                // ===========================================
                // Delete Memo Header
                // ===========================================

                sql = @"
DELETE FROM tbdMemo
WHERE fldNo=@MemoNo
";

                cmd = new SqlCommand(sql, conn, trans);
                cmd.Parameters.Add("@MemoNo", SqlDbType.NVarChar).Value = memoNo;

                int row = await cmd.ExecuteNonQueryAsync();

                trans.Commit();

                return row > 0;
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        public async Task<List<MemoResponse>> SearchAsync(
    MemoSearchRequest request)
        {
            List<MemoResponse> list = new();

            using SqlConnection conn = _database.GetConnection();
            await conn.OpenAsync();

            string sql = @"
SELECT a.fldNo,a.fldCompanyCode,a.fldSupplierCode,a.fldSupplierName,a.fldDate,a.fldAmount,a.fldRemarks,a.fldCreatedBy,a.fldCreatedOn,a.fldMillCode,a.ApprovalLevel,a.ApprovalStatus,a.fldApprovedBy,a.fldApprovedOn,a.ApprovalCreatedOn,
    MAX(
        CASE 
        WHEN B.fldDocumentType='Invoice'
        THEN B.fldOriginalFileName 
        END
    ) AS Invoice,
    MAX(
        CASE 
        WHEN B.fldDocumentType='BAP'
        THEN B.fldOriginalFileName 
        END
    ) AS BAP,
    MAX(
        CASE 
        WHEN B.fldDocumentType='FakturPajak'
        THEN B.fldOriginalFileName 
        END
    ) AS FakturPajak
FROM vw_SearchMemo AS a 
LEFT JOIN tbdMemoAttachment AS B ON a.fldNo=B.fldNo
WHERE 1=1 ";

            if (request.CompanyAccess != null && request.CompanyAccess.Count > 0)
            {
                List<string> parameters = new();
                for (int i = 0; i < request.CompanyAccess.Count; i++)
                {
                    parameters.Add("@CompanyCode" + i);
                }
                sql += $" AND a.fldCompanyCode IN ({string.Join(",", parameters)}) ";
            }

            if (request.ApprovalLevel >= 0) { sql += " AND a.ApprovalLevel=@ApprovalLevel "; }
            if (!string.IsNullOrWhiteSpace(request.millCode)) { sql += " AND a.fldMillCode=@millCode "; }
            if (!string.IsNullOrWhiteSpace(request.MemoNo)) { sql += " AND a.fldNo LIKE @MemoNo "; }
            if (!string.IsNullOrWhiteSpace(request.SupplierCode)) { sql += " AND a.fldSupplierCode=@SupplierCode "; }
            if (request.DateFrom != null) { sql += " AND a.fldDate>=@DateFrom "; }
            if (request.DateTo != null) { sql += " AND a.fldDate<=@DateTo "; }

            if (!string.IsNullOrWhiteSpace(request.CompanyCode)) { sql += " AND a.fldCompanyCode=@CompanyCode "; }
            if (request.AmountFrom != null) { sql += " AND a.fldAmount>=@AmountFrom "; }
            if (request.AmountTo != null) { sql += " AND a.fldAmount<=@AmountTo "; }
            if (!string.IsNullOrWhiteSpace(request.Remarks)) { sql += " AND a.fldRemarks LIKE @Remarks "; }

            sql += " GROUP BY a.fldNo,a.fldCompanyCode,a.fldSupplierCode,a.fldSupplierName,a.fldDate,a.fldAmount,a.fldRemarks,a.fldCreatedBy,a.fldCreatedOn,a.fldMillCode,a.ApprovalLevel,a.ApprovalStatus,a.fldApprovedBy,a.fldApprovedOn,a.ApprovalCreatedOn ORDER BY a.fldDate DESC ";

            SqlCommand cmd = new(sql, conn);

            if (request.CompanyAccess != null && request.CompanyAccess.Count > 0)
            {
                for (int i = 0; i < request.CompanyAccess.Count; i++)
                {
                    cmd.Parameters.Add("@CompanyCode" + i, SqlDbType.NVarChar).Value = request.CompanyAccess[i];
                }
            }
            if (request.ApprovalLevel >= 0) cmd.Parameters.Add("@ApprovalLevel", SqlDbType.NVarChar).Value = request.ApprovalLevel;
            if (!string.IsNullOrWhiteSpace(request.millCode)) cmd.Parameters.Add("@millCode", SqlDbType.NVarChar).Value = request.millCode;
            if (!string.IsNullOrWhiteSpace(request.MemoNo)) cmd.Parameters.Add("@MemoNo", SqlDbType.NVarChar).Value = "%" + request.MemoNo + "%";
            if (!string.IsNullOrWhiteSpace(request.SupplierCode)) cmd.Parameters.Add("@SupplierCode", SqlDbType.NVarChar).Value = request.SupplierCode;
            if (request.DateFrom != null) cmd.Parameters.Add("@DateFrom", SqlDbType.Date).Value = request.DateFrom.Value;
            if (request.DateTo != null) cmd.Parameters.Add("@DateTo", SqlDbType.Date).Value = request.DateTo.Value;

            if (!string.IsNullOrWhiteSpace(request.CompanyCode)) cmd.Parameters.Add("@CompanyCode", SqlDbType.VarChar).Value = request.CompanyCode;
            if (request.AmountFrom != null) cmd.Parameters.Add("@AmountFrom", SqlDbType.Decimal).Value = request.AmountFrom;
            if (request.AmountTo != null) cmd.Parameters.Add("@AmountTo", SqlDbType.Decimal).Value = request.AmountTo;
            if (!string.IsNullOrWhiteSpace(request.Remarks)) cmd.Parameters.Add("@Remarks", SqlDbType.VarChar).Value = "%" + request.Remarks + "%";

            SqlDataReader dr = await cmd.ExecuteReaderAsync();
            while (await dr.ReadAsync())
            {
                list.Add(new MemoResponse
                {
                    MemoNo = dr["fldNo"].ToString() ?? "",
                    CompanyCode = dr["fldCompanyCode"].ToString() ?? "",
                    SupplierCode = dr["fldSupplierCode"].ToString() ?? "",
                    SupplierName = dr["fldSupplierName"].ToString() ?? "",
                    MemoDate = Convert.ToDateTime(dr["fldDate"]),
                    Amount = Convert.ToDecimal(dr["fldAmount"]),
                    Remarks = dr["fldRemarks"].ToString() ?? "",
                    CreatedBy = dr["fldCreatedBy"].ToString() ?? "",
                    CreatedOn = Convert.ToDateTime(dr["fldCreatedOn"]),
                    ApprovalLevel = Convert.ToInt16(dr["ApprovalLevel"]),
                    ApprovalStatus = dr["ApprovalStatus"].ToString() ?? "",
                    ApprovedBy = dr["fldApprovedBy"].ToString() ?? "",
                    ApprovedOn = dr.GetDate("fldApprovedOn"),
                    ApprovalCreatedOn = dr.GetDate("ApprovalCreatedOn"),
                    MillCode = dr["fldMillCode"].ToString() ?? "",
                    Invoice = dr["Invoice"].ToString() ?? "",
                    BAP = dr["BAP"].ToString() ?? "",
                    FakturPajak = dr["FakturPajak"].ToString() ?? ""
                });
            }
            return list;
        }

        public async Task<string> CreateAsync(MemoCreateRequest request)
        {
            using SqlConnection conn = _database.GetConnection();
            await conn.OpenAsync();
            SqlTransaction trans = conn.BeginTransaction();

            try
            {
                string memoNo = await _runningNumber.GenerateMemoNumberAsync(request.CompanyCode);
                string sql = @"
INSERT INTO tbdMemo
(
    fldNo,
    fldCompanyCode,
    fldSupplierCode,
    fldDate,
    fldAmount,
    fldRemarks,
    fldCreatedBy,
    fldCreatedOn,
    fldCreatedIP,
    fldMillCode
)
VALUES
(
    @MemoNo,
    @CompanyCode,
    @SupplierCode,
    @MemoDate,
    @Amount,
    @Remarks,
    @CreatedBy,
    GETDATE(),
    @CreatedIP,
    @fldMillCode
)
";

                SqlCommand cmd = new(sql, conn, trans);
                cmd.Parameters.Add("@MemoNo", SqlDbType.NVarChar).Value = memoNo;
                cmd.Parameters.Add("@CompanyCode", SqlDbType.NVarChar).Value = request.CompanyCode;
                cmd.Parameters.Add("@SupplierCode", SqlDbType.NVarChar).Value = request.SupplierCode;
                cmd.Parameters.Add("@MemoDate", SqlDbType.Date).Value = request.MemoDate;
                cmd.Parameters.Add("@Amount", SqlDbType.Decimal).Value = request.Amount;
                cmd.Parameters["@Amount"].Precision = 18;
                cmd.Parameters["@Amount"].Scale = 2;
                cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar).Value = request.Remarks ?? "";
                cmd.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = request.UserName;
                cmd.Parameters.Add("@CreatedIP", SqlDbType.NVarChar).Value = request.UserIP;
                cmd.Parameters.Add("@fldMillCode", SqlDbType.NVarChar).Value = request.MillCode;
                await cmd.ExecuteNonQueryAsync();
                trans.Commit();
                return memoNo;
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        public async Task<bool> SubmitAsync(MemoSubmitRequest request)
        {
            using SqlConnection conn = _database.GetConnection();

            await conn.OpenAsync();

            SqlTransaction trans = conn.BeginTransaction();

            try
            {
                //====================================================
                // Check Memo
                //====================================================

                string sql = @"
SELECT COUNT(*)
FROM tbdMemo
WHERE fldNo=@MemoNo
";

                SqlCommand cmd = new(sql, conn, trans);

                cmd.Parameters.Add("@MemoNo", SqlDbType.NVarChar).Value = request.MemoNo;

                int memoCount = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                if (memoCount == 0)
                    throw new Exception("Memo not found.");

                //====================================================
                // Check Already Submitted
                //====================================================

                sql = @"
SELECT COUNT(*)
FROM tbdApproval
WHERE fldNo=@MemoNo
";

                cmd = new(sql, conn, trans);

                cmd.Parameters.Add("@MemoNo", SqlDbType.NVarChar).Value = request.MemoNo;

                int approvalCount = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                if (approvalCount > 0)
                    throw new Exception("Memo has already been submitted.");

                //====================================================
                // Get First Approval Level
                //====================================================

                sql = @"
SELECT TOP 1 fldApprovalLevel
FROM tbdApprovalLevel
ORDER BY fldSequence
";

                cmd = new(sql, conn, trans);

                int approvalLevel = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                //====================================================
                // Insert Approval
                //====================================================

                sql = @"

INSERT INTO tbdApproval
(
    fldNo,
    fldApprovalLevel,
    fldCurrentApproval,
    fldCreatedOn,
    fldCreatedBy,
    fldCreatedIP,
    fldApprovedOn,
    fldApprovedBy,
    fldStatus,
    fldRemarks,
    fldEmailSentOn
)
VALUES
(
    @MemoID,
    @ApprovalLevel,
    1,
    GETDATE(),
    @fldCreatedBy,
    @fldCreatedIP,
    NULL,
    NULL,
    'Waiting',
    NULL,
    NULL
)

";

                cmd = new(sql, conn, trans);

                cmd.Parameters.Add("@MemoID", SqlDbType.NVarChar).Value = request.MemoNo;

                cmd.Parameters.Add("@ApprovalLevel", SqlDbType.Int).Value = approvalLevel;

                cmd.Parameters.Add("@fldCreatedBy", SqlDbType.NVarChar).Value = request.UserName;

                cmd.Parameters.Add("@fldCreatedIP", SqlDbType.NVarChar).Value = request.UserIP;

                await cmd.ExecuteNonQueryAsync();

                //====================================================
                // Insert Memo Log
                //====================================================

                sql = @"

INSERT INTO tbdMemoLog
(
    fldNo,
    fldDateTime,
    fldAction,
    fldRemarks,
    fldUser,
    fldIPAddress
)

VALUES
(
    @MemoID,
    GETDATE(),
    @Action,
    @Remarks,
    @User,
    @IP
)

";

                cmd = new SqlCommand(sql, conn, trans);

                cmd.Parameters.Add("@MemoID", SqlDbType.NVarChar).Value = request.MemoNo;

                cmd.Parameters.Add("@Action", SqlDbType.NVarChar).Value = "SUBMIT";

                cmd.Parameters.Add("@Remarks", SqlDbType.NVarChar).Value =
                    "Memo submitted to approval workflow.";

                cmd.Parameters.Add("@User", SqlDbType.NVarChar).Value =
                    request.UserName;

                cmd.Parameters.Add("@IP", SqlDbType.NVarChar).Value =
                    request.UserIP;

                await cmd.ExecuteNonQueryAsync();

                trans.Commit();

                //RZK Sementara
                //await _emailData.SendSubmitEmailAsync(
                //    request.MemoNo,
                //    approvalLevel);

                return true;
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        public async Task<bool> ApproveAsync(MemoApproveRequest request)
        {
            using SqlConnection conn = _database.GetConnection();

            await conn.OpenAsync();

            SqlTransaction trans = conn.BeginTransaction();

            try
            {
                string sql = @"

SELECT
    fldApprovalLevel

FROM tbdApproval

WHERE fldNo=@MemoNo
AND fldCurrentApproval=1
AND fldStatus='Waiting'

";

                SqlCommand cmd = new(sql, conn, trans);

                cmd.Parameters.Add("@MemoNo",
                    SqlDbType.NVarChar).Value = request.MemoNo;

                object obj =
                    await cmd.ExecuteScalarAsync();

                if (obj == null)
                    throw new Exception("No active approval.");

                int currentLevel =
                    Convert.ToInt32(obj);

                sql = @"

SELECT
    fldApprovalLevel

FROM tbdApproval

WHERE fldNo=@MemoNo
AND fldCurrentApproval=1
AND fldStatus='Waiting'

";

                cmd = new(sql, conn, trans);

                cmd.Parameters.Add("@MemoNo",
                    SqlDbType.NVarChar).Value = request.MemoNo;

                obj =
                    await cmd.ExecuteScalarAsync();

                if (obj == null)
                    throw new Exception("No active approval.");

                currentLevel =
                    Convert.ToInt32(obj);

                sql = @"

UPDATE tbdApproval

SET

fldCurrentApproval=0,

fldApprovedBy=@User,

fldApprovedOn=GETDATE(),

fldStatus='Approved',

fldRemarks=@Remarks

WHERE fldNo=@MemoNo

AND fldApprovalLevel=@ApprovalLevel

";

                cmd = new(sql, conn, trans);

                cmd.Parameters.Add("@User",
                SqlDbType.NVarChar).Value = request.UserName;

                cmd.Parameters.Add("@Remarks",
                SqlDbType.NVarChar).Value = request.Remarks;

                cmd.Parameters.Add("@MemoNo",
                SqlDbType.NVarChar).Value = request.MemoNo;

                cmd.Parameters.Add("@ApprovalLevel",
                SqlDbType.Int).Value = currentLevel;

                await cmd.ExecuteNonQueryAsync();

                sql = @"

SELECT TOP 1

fldApprovalLevel

FROM tbdApprovalLevel

WHERE fldApprovalLevel>@CurrentLevel

AND fldIsActive=1

ORDER BY fldApprovalLevel

";

                cmd = new(sql, conn, trans);

                cmd.Parameters.Add("@CurrentLevel",
                SqlDbType.Int).Value = currentLevel;

                object nextLevelObj =
                await cmd.ExecuteScalarAsync();

                if (nextLevelObj != null)
                {
                    int nextLevel =
                        Convert.ToInt32(nextLevelObj);

                    sql = @"

INSERT INTO tbdApproval
(
    fldNo,
    fldApprovalLevel,
    fldCurrentApproval,
    fldCreatedOn,
    fldStatus
)

VALUES
(
    @MemoNo,
    @Level,
    1,
    GETDATE(),
    'Waiting'
)

";

                    cmd = new(sql, conn, trans);

                    cmd.Parameters.Add("@MemoNo",
                    SqlDbType.NVarChar).Value = request.MemoNo;

                    cmd.Parameters.Add("@Level",
                    SqlDbType.Int).Value = nextLevel;

                    await cmd.ExecuteNonQueryAsync();
                }

                if (currentLevel == 40)
                {
                    byte[] pdf =
                        await GenerateMemoPdfAsync(
                            conn,
                            trans,
                            request.MemoNo,
                            request.UserName);

                    await SaveMemoPdfAttachmentAsync(
                        conn,
                        trans,
                        request.MemoNo,
                        pdf,
                        request.UserName);
                }

                sql = @"

INSERT INTO tbdMemoLog
(
fldNo,
fldAction,
fldRemarks,
fldUser,
fldDateTime,
fldIPAddress
)

VALUES
(
@MemoNo,
'APPROVE',
@Remarks,
@User,
GETDATE(),
@IP
)

";

                cmd = new(sql, conn, trans);

                cmd.Parameters.Add("@MemoNo",
                SqlDbType.NVarChar).Value = request.MemoNo;

                cmd.Parameters.Add("@Remarks",
                SqlDbType.NVarChar).Value = request.Remarks;

                cmd.Parameters.Add("@User",
                SqlDbType.NVarChar).Value = request.UserName;

                cmd.Parameters.Add("@IP",
                SqlDbType.NVarChar).Value = request.UserIP;

                await cmd.ExecuteNonQueryAsync();

                trans.Commit();

                return true;

            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        public async Task<CurrentApprovalResponse> GetCurrentApprovalAsync(string memoNo)
        {
            using SqlConnection conn = _database.GetConnection();

            await conn.OpenAsync();

            string sql = @"

SELECT
    A.fldNo,
    A.fldApprovalLevel,
    B.fldDescription,
    A.fldStatus

FROM tbdApproval A

INNER JOIN tbdApprovalLevel B
ON A.fldApprovalLevel=B.fldApprovalLevel

WHERE A.fldNo=@MemoNo

AND A.fldCurrentApproval=1

";

            SqlCommand cmd = new(sql, conn);

            cmd.Parameters.Add("@MemoNo",
                SqlDbType.NVarChar).Value = memoNo;

            using SqlDataReader dr =
                await cmd.ExecuteReaderAsync();

            if (!await dr.ReadAsync())
            {
                return new CurrentApprovalResponse
                {
                    MemoNo = memoNo,
                    IsCompleted = true,
                    Status = "Completed"
                };
            }

            return new CurrentApprovalResponse
            {
                MemoNo = dr["fldNo"].ToString()!,
                ApprovalLevel = Convert.ToInt32(dr["fldApprovalLevel"]),
                ApprovalDescription = dr["fldDescription"].ToString()!,
                Status = dr["fldStatus"].ToString()!,
                IsCompleted = false
            };
        }

        public async Task<bool> RejectAsync(
            MemoRejectRequest request)
        {
            using SqlConnection conn =
                _database.GetConnection();

            await conn.OpenAsync();

            SqlTransaction trans =
                conn.BeginTransaction();

            try
            {
                string sql = @"
SELECT fldApprovalLevel
FROM tbdApproval
WHERE fldNo=@MemoNo
AND fldCurrentApproval=1
AND fldStatus='Waiting'
";

                SqlCommand cmd = new(sql, conn, trans);

                cmd.Parameters.Add("@MemoNo",
                SqlDbType.NVarChar).Value = request.MemoNo;

                object obj =
                await cmd.ExecuteScalarAsync();

                if (obj == null)
                    throw new Exception("No active approval.");

                int currentLevel =
                Convert.ToInt32(obj);

                sql = @"
UPDATE tbdApproval
SET
fldCurrentApproval=0,
fldApprovedBy=@User,
fldApprovedOn=GETDATE(),
fldStatus='Rejected',
fldRemarks=@Remarks
WHERE fldNo=@MemoNo
AND fldApprovalLevel=@Level;
";

                cmd = new(sql, conn, trans);

                cmd.Parameters.Add("@User",
                SqlDbType.NVarChar).Value = request.UserName;

                cmd.Parameters.Add("@Remarks",
                SqlDbType.NVarChar).Value = request.Remarks;

                cmd.Parameters.Add("@MemoNo",
                SqlDbType.NVarChar).Value = request.MemoNo;

                cmd.Parameters.Add("@Level",
                SqlDbType.Int).Value = currentLevel;

                await cmd.ExecuteNonQueryAsync();

                sql = @"

INSERT INTO tbdMemoLog
(
fldNo,
fldAction,
fldRemarks,
fldUser,
fldDateTime,
fldIPAddress
)

VALUES
(
@MemoNo,
'REJECT',
@Remarks,
@User,
GETDATE(),
@IP
)

";

                cmd = new(sql, conn, trans);

                cmd.Parameters.Add("@MemoNo",
                SqlDbType.NVarChar).Value = request.MemoNo;

                cmd.Parameters.Add("@Remarks",
                SqlDbType.NVarChar).Value = request.Remarks;

                cmd.Parameters.Add("@User",
                SqlDbType.NVarChar).Value = request.UserName;

                cmd.Parameters.Add("@IP",
                SqlDbType.NVarChar).Value = request.UserIP;

                await cmd.ExecuteNonQueryAsync();

                trans.Commit();

                return true;
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }
    }

}
