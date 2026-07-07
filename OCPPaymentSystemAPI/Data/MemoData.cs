using Microsoft.Data.SqlClient;
using OCPPaymentSystemAPI.Helpers;
using OCPPaymentSystemAPI.Models;
using OCPPaymentSystemAPI.Models.Workflow;
using System.Data;

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

        public async Task<List<string>> GetAttachmentListAsync(
            string memoNo)
        {
            List<string> list = new();


            using SqlConnection conn =
                _database.GetConnection();


            await conn.OpenAsync();


            string sql =
            @"
    SELECT fldDocumentType
    FROM tbdMemoAttachment
    WHERE fldNo=@MemoNo
    ORDER BY fldDocumentType
    ";


            SqlCommand cmd =
                new(sql, conn);


            cmd.Parameters.Add(
                "@MemoNo",
                SqlDbType.NVarChar)
                .Value = memoNo;


            SqlDataReader dr =
                await cmd.ExecuteReaderAsync();


            while (await dr.ReadAsync())
            {
                list.Add(
                    dr["fldDocumentType"]
                    .ToString() ?? "");
            }


            return list;
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

            string sql = @"SELECT * FROM vw_SearchMemo WHERE 1=1 ";

            if (request.CompanyAccess != null && request.CompanyAccess.Count > 0)
            {
                List<string> parameters = new();
                for (int i = 0; i < request.CompanyAccess.Count; i++)
                {
                    parameters.Add("@CompanyCode" + i);
                }
                sql += $" AND fldCompanyCode IN ({string.Join(",", parameters)}) ";
            }

            if (request.ApprovalLevel >= 0) { sql += " AND ApprovalLevel=@ApprovalLevel "; }
            if (!string.IsNullOrWhiteSpace(request.millCode)) {sql += " AND fldMillCode=@millCode ";}
            if (!string.IsNullOrWhiteSpace(request.MemoNo)) {sql += " AND fldNo LIKE @MemoNo ";}
            if (!string.IsNullOrWhiteSpace(request.SupplierCode)) {sql += " AND fldSupplierCode=@SupplierCode ";}
            if (request.DateFrom != null) {sql += " AND fldDate>=@DateFrom ";}
            if (request.DateTo != null) {sql += " AND fldDate<=@DateTo ";}

            if (!string.IsNullOrWhiteSpace(request.CompanyCode)) {sql += " AND fldCompanyCode=@CompanyCode ";}
            if (request.AmountFrom != null) {sql += " AND fldAmount>=@AmountFrom ";}
            if (request.AmountTo != null) {sql += " AND fldAmount<=@AmountTo ";}
            if (!string.IsNullOrWhiteSpace(request.Remarks)) {sql += " AND fldRemarks LIKE @Remarks ";}

            sql += " ORDER BY fldDate DESC ";

            SqlCommand cmd = new(sql, conn);

            if (request.CompanyAccess != null && request.CompanyAccess.Count > 0)
            {
                for (int i = 0; i < request.CompanyAccess.Count; i++)
                {
                    cmd.Parameters.Add("@CompanyCode" + i,SqlDbType.NVarChar).Value = request.CompanyAccess[i];
                }
            }
            if (request.ApprovalLevel >= 0) cmd.Parameters.Add("@ApprovalLevel", SqlDbType.NVarChar).Value = request.ApprovalLevel;
            if (!string.IsNullOrWhiteSpace(request.millCode)) cmd.Parameters.Add("@millCode", SqlDbType.NVarChar).Value = request.millCode;
            if (!string.IsNullOrWhiteSpace(request.MemoNo)) cmd.Parameters.Add("@MemoNo", SqlDbType.NVarChar).Value = "%" + request.MemoNo + "%";
            if (!string.IsNullOrWhiteSpace(request.SupplierCode)) cmd.Parameters.Add("@SupplierCode", SqlDbType.NVarChar).Value = request.SupplierCode;
            if (request.DateFrom != null) cmd.Parameters.Add("@DateFrom", SqlDbType.Date).Value = request.DateFrom.Value;
            if (request.DateTo != null) cmd.Parameters.Add("@DateTo", SqlDbType.Date).Value = request.DateTo.Value;

            if (!string.IsNullOrWhiteSpace(request.CompanyCode)) cmd.Parameters.Add("@CompanyCode",SqlDbType.VarChar).Value = request.CompanyCode;
            if (request.AmountFrom != null) cmd.Parameters.Add("@AmountFrom",SqlDbType.Decimal).Value = request.AmountFrom;
            if (request.AmountTo != null) cmd.Parameters.Add("@AmountTo",SqlDbType.Decimal).Value = request.AmountTo;
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
                    MillCode = dr["fldMillCode"].ToString() ?? ""
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
                string memoNo = await _runningNumber.GenerateMemoNumberAsync();
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
