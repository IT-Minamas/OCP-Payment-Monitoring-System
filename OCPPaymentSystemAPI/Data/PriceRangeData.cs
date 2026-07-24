using Microsoft.Data.SqlClient;
using OCPPaymentSystemAPI.Models;
using System.Data;

namespace OCPPaymentSystemAPI.Data
{
    public class PriceRangeData
    {
        private readonly Database _database;

        public PriceRangeData(Database database)
        {
            _database = database;
        }

        private async Task<bool> IsDateOverlapAsync(
            SqlConnection conn,
            SqlTransaction? trans,
            int id,
            string millCode,
            DateTime dateFrom,
            DateTime dateTo)
        {
            string sql = @"
SELECT COUNT(*)
FROM tbdPriceRange
WHERE fldMillCode=@MillCode
AND fldID<>@ID
AND @DateFrom<=fldDateTo
AND @DateTo>=fldDateFrom";

            SqlCommand cmd = new(sql, conn);

            if (trans != null)
                cmd.Transaction = trans;

            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
            cmd.Parameters.Add("@MillCode", SqlDbType.VarChar).Value = millCode;
            cmd.Parameters.Add("@DateFrom", SqlDbType.Date).Value = dateFrom;
            cmd.Parameters.Add("@DateTo", SqlDbType.Date).Value = dateTo;

            int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());

            return count > 0;
        }

        public async Task<PriceRangeModel?> GetByIDAsync(int id)
        {
            using SqlConnection conn = _database.GetConnection();

            await conn.OpenAsync();

            string sql = @"
SELECT
fldID,
fldMillCode,
fldDateFrom,
fldDateTo,
fldPriceFrom,
fldPriceTo,
fldAttachmentFileName,
fldAttachmentContentType,
fldAttachmentFileSize,
fldCreatedBy,
fldCreatedOn,
fldCreatedIP,
fldUpdatedBy,
fldUpdatedOn,
fldUpdatedIP
FROM tbdPriceRange
WHERE fldID=@ID";

            SqlCommand cmd = new(sql, conn);

            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;

            SqlDataReader dr = await cmd.ExecuteReaderAsync();

            if (!await dr.ReadAsync())
                return null;

            return new PriceRangeModel
            {
                ID = Convert.ToInt32(dr["fldID"]),
                MillCode = dr["fldMillCode"].ToString() ?? "",
                DateFrom = Convert.ToDateTime(dr["fldDateFrom"]),
                DateTo = Convert.ToDateTime(dr["fldDateTo"]),
                PriceFrom = Convert.ToDecimal(dr["fldPriceFrom"]),
                PriceTo = Convert.ToDecimal(dr["fldPriceTo"]),
                AttachmentFileName = dr["fldAttachmentFileName"].ToString() ?? "",
                AttachmentContentType = dr["fldAttachmentContentType"].ToString() ?? "",
                AttachmentFileSize = dr["fldAttachmentFileSize"] == DBNull.Value ? 0 : Convert.ToInt64(dr["fldAttachmentFileSize"]),
                CreatedBy = dr["fldCreatedBy"].ToString() ?? "",
                CreatedOn = Convert.ToDateTime(dr["fldCreatedOn"]),
                CreatedIP = dr["fldCreatedIP"].ToString() ?? "",
                UpdatedBy = dr["fldUpdatedBy"].ToString() ?? "",
                UpdatedOn = dr["fldUpdatedOn"] == DBNull.Value ? null : Convert.ToDateTime(dr["fldUpdatedOn"]),
                UpdatedIP = dr["fldUpdatedIP"].ToString() ?? ""
            };
        }

        public async Task<int> CreateAsync(
            PriceRangeCreateRequest request)
        {
            using SqlConnection conn = _database.GetConnection();

            await conn.OpenAsync();

            SqlTransaction trans = conn.BeginTransaction();

            try
            {
                if (request.DateFrom > request.DateTo)
                    throw new Exception("Date From must be earlier than Date To.");

                if (request.PriceFrom > request.PriceTo)
                    throw new Exception("Price Range From must be less than Price Range To.");

                bool overlap =
                    await IsDateOverlapAsync(
                        conn,
                        trans,
                        0,
                        request.MillCode,
                        request.DateFrom,
                        request.DateTo);

                if (overlap)
                    throw new Exception("Price range period overlaps with existing configuration.");

                string sql = @"
INSERT INTO tbdPriceRange
(
fldMillCode,
fldDateFrom,
fldDateTo,
fldPriceFrom,
fldPriceTo,
fldCreatedBy,
fldCreatedOn,
fldCreatedIP
)
VALUES
(
@MillCode,
@DateFrom,
@DateTo,
@PriceFrom,
@PriceTo,
@UserName,
GETDATE(),
@UserIP
)

SELECT CAST(SCOPE_IDENTITY() AS INT)";

                SqlCommand cmd = new(sql, conn, trans);

                cmd.Parameters.Add("@MillCode", SqlDbType.VarChar).Value = request.MillCode;
                cmd.Parameters.Add("@DateFrom", SqlDbType.Date).Value = request.DateFrom;
                cmd.Parameters.Add("@DateTo", SqlDbType.Date).Value = request.DateTo;

                cmd.Parameters.Add("@PriceFrom", SqlDbType.Decimal).Value = request.PriceFrom;
                cmd.Parameters["@PriceFrom"].Precision = 18;
                cmd.Parameters["@PriceFrom"].Scale = 2;

                cmd.Parameters.Add("@PriceTo", SqlDbType.Decimal).Value = request.PriceTo;
                cmd.Parameters["@PriceTo"].Precision = 18;
                cmd.Parameters["@PriceTo"].Scale = 2;

                cmd.Parameters.Add("@UserName", SqlDbType.NVarChar).Value = request.UserName;
                cmd.Parameters.Add("@UserIP", SqlDbType.NVarChar).Value = request.UserIP;

                int id = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                trans.Commit();

                return id;
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }


        public async Task<bool> UpdateAsync(
            PriceRangeUpdateRequest request)
        {
            using SqlConnection conn = _database.GetConnection();

            await conn.OpenAsync();

            SqlTransaction trans = conn.BeginTransaction();

            try
            {
                if (request.DateFrom > request.DateTo)
                    throw new Exception("Date From must be earlier than Date To.");

                if (request.PriceFrom > request.PriceTo)
                    throw new Exception("Price Range From must be less than Price Range To.");

                bool overlap =
                    await IsDateOverlapAsync(
                        conn,
                        trans,
                        request.ID,
                        request.MillCode,
                        request.DateFrom,
                        request.DateTo);

                if (overlap)
                    throw new Exception("Price range period overlaps with existing configuration.");

                string sql = @"
UPDATE tbdPriceRange
SET
fldMillCode=@MillCode,
fldDateFrom=@DateFrom,
fldDateTo=@DateTo,
fldPriceFrom=@PriceFrom,
fldPriceTo=@PriceTo,
fldUpdatedBy=@UserName,
fldUpdatedOn=GETDATE(),
fldUpdatedIP=@UserIP
WHERE fldID=@ID";

                SqlCommand cmd = new(sql, conn, trans);

                cmd.Parameters.Add("@ID", SqlDbType.Int).Value = request.ID;
                cmd.Parameters.Add("@MillCode", SqlDbType.VarChar).Value = request.MillCode;
                cmd.Parameters.Add("@DateFrom", SqlDbType.Date).Value = request.DateFrom;
                cmd.Parameters.Add("@DateTo", SqlDbType.Date).Value = request.DateTo;

                cmd.Parameters.Add("@PriceFrom", SqlDbType.Decimal).Value = request.PriceFrom;
                cmd.Parameters["@PriceFrom"].Precision = 18;
                cmd.Parameters["@PriceFrom"].Scale = 2;

                cmd.Parameters.Add("@PriceTo", SqlDbType.Decimal).Value = request.PriceTo;
                cmd.Parameters["@PriceTo"].Precision = 18;
                cmd.Parameters["@PriceTo"].Scale = 2;

                cmd.Parameters.Add("@UserName", SqlDbType.NVarChar).Value = request.UserName;
                cmd.Parameters.Add("@UserIP", SqlDbType.NVarChar).Value = request.UserIP;

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

        public async Task<bool> DeleteAsync(int id)
        {
            using SqlConnection conn = _database.GetConnection();

            await conn.OpenAsync();

            string sql = @"
DELETE FROM tbdPriceRange
WHERE fldID=@ID";

            SqlCommand cmd = new(sql, conn);

            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;

            int row = await cmd.ExecuteNonQueryAsync();

            return row > 0;
        }

        public async Task<List<PriceRangeModel>> SearchAsync(
            PriceRangeSearchRequest request)
        {
            List<PriceRangeModel> list = new();

            using SqlConnection conn = _database.GetConnection();

            await conn.OpenAsync();

            string sql = @"
SELECT
a.fldID,
a.fldMillCode,
a.fldDateFrom,
a.fldDateTo,
a.fldPriceFrom,
a.fldPriceTo,
a.fldAttachmentFileName,
a.fldAttachmentContentType,
a.fldAttachmentFileSize,
a.fldCreatedBy,
a.fldCreatedOn,
a.fldCreatedIP,
a.fldUpdatedBy,
a.fldUpdatedOn,
a.fldUpdatedIP,
b.fldName AS fldMillName
FROM tbdPriceRange AS a
JOIN [172.16.192.10].[SAP_Replicate_New].[dbo].[vw_UnitSetup2] AS b ON a.fldMillCode COLLATE SQL_Latin1_General_CP1_CI_AS = b.fldSAPVirtualCode COLLATE SQL_Latin1_General_CP1_CI_AS
WHERE 1=1 
";

            if (!string.IsNullOrWhiteSpace(request.CompanyCode))
            {
                sql += @"
AND b.fldCompanyCode IN
(
    SELECT LTRIM(RTRIM(value))
    FROM STRING_SPLIT(@CompanyCode, ',')
)";
            }

            if (!string.IsNullOrWhiteSpace(request.MillCode))
                sql += " AND a.fldMillCode=@MillCode ";

            if (request.DateFrom != null)
                sql += " AND a.fldDateTo>=@DateFrom ";

            if (request.DateTo != null)
                sql += " AND a.fldDateFrom<=@DateTo ";

            sql += " ORDER BY a.fldMillCode,a.fldDateFrom DESC";

            SqlCommand cmd = new(sql, conn);

            if (!string.IsNullOrWhiteSpace(request.CompanyCode))
                cmd.Parameters.Add("@CompanyCode", SqlDbType.VarChar).Value = request.CompanyCode;

            if (!string.IsNullOrWhiteSpace(request.MillCode))
                cmd.Parameters.Add("@MillCode", SqlDbType.VarChar).Value = request.MillCode;

            if (request.DateFrom != null)
                cmd.Parameters.Add("@DateFrom", SqlDbType.Date).Value = request.DateFrom.Value;

            if (request.DateTo != null)
                cmd.Parameters.Add("@DateTo", SqlDbType.Date).Value = request.DateTo.Value;

            SqlDataReader dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                list.Add(new PriceRangeModel
                {
                    ID = Convert.ToInt32(dr["fldID"]),
                    MillCode = dr["fldMillCode"].ToString() ?? "",
                    MillName = dr["fldMillName"].ToString() ?? "",
                    DateFrom = Convert.ToDateTime(dr["fldDateFrom"]),
                    DateTo = Convert.ToDateTime(dr["fldDateTo"]),
                    PriceFrom = Convert.ToDecimal(dr["fldPriceFrom"]),
                    PriceTo = Convert.ToDecimal(dr["fldPriceTo"]),

                    AttachmentFileName = dr["fldAttachmentFileName"] == DBNull.Value ? "" : dr["fldAttachmentFileName"].ToString() ?? "",
                    AttachmentContentType = dr["fldAttachmentContentType"] == DBNull.Value ? "" : dr["fldAttachmentContentType"].ToString() ?? "",
                    AttachmentFileSize = dr["fldAttachmentFileSize"] == DBNull.Value ? 0 : Convert.ToInt64(dr["fldAttachmentFileSize"]),

                    CreatedBy = dr["fldCreatedBy"] == DBNull.Value ? "" : dr["fldCreatedBy"].ToString() ?? "",
                    CreatedOn = dr["fldCreatedOn"] == DBNull.Value ? Convert.ToDateTime("2000/01/01") : Convert.ToDateTime(dr["fldCreatedOn"]),
                    CreatedIP = dr["fldCreatedIP"] == DBNull.Value ? "" : dr["fldCreatedIP"].ToString() ?? "",

                    UpdatedBy = dr["fldUpdatedBy"] == DBNull.Value ? "" : dr["fldUpdatedBy"].ToString() ?? "",
                    UpdatedOn = dr["fldUpdatedOn"] == DBNull.Value ? null : Convert.ToDateTime(dr["fldUpdatedOn"]),
                    UpdatedIP = dr["fldUpdatedIP"] == DBNull.Value ? "" : dr["fldUpdatedIP"].ToString() ?? ""
                });
            }

            return list;
        }

        public async Task<bool> UploadAttachmentAsync(
            PriceRangeAttachmentRequest request)
        {
            if (request.file == null)
                throw new Exception("File is empty.");

            byte[] fileBytes;

            using (var ms = new MemoryStream())
            {
                await request.file.CopyToAsync(ms);
                fileBytes = ms.ToArray();
            }

            using SqlConnection conn = _database.GetConnection();

            await conn.OpenAsync();

            string sql = @"
UPDATE tbdPriceRange
SET
fldAttachmentFileName=@FileName,
fldAttachmentContentType=@ContentType,
fldAttachmentFileSize=@FileSize,
fldAttachment=@File
WHERE fldID=@ID";

            SqlCommand cmd = new(sql, conn);

            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = request.ID;
            cmd.Parameters.Add("@FileName", SqlDbType.NVarChar).Value = request.file.FileName;
            cmd.Parameters.Add("@ContentType", SqlDbType.NVarChar).Value = request.file.ContentType;
            cmd.Parameters.Add("@FileSize", SqlDbType.BigInt).Value = request.file.Length;
            cmd.Parameters.Add("@File", SqlDbType.VarBinary).Value = fileBytes;

            int row = await cmd.ExecuteNonQueryAsync();

            return row > 0;
        }

        public async Task<PriceRangeAttachmentDownload?> DownloadAttachmentAsync(
            int id)
        {
            using SqlConnection conn = _database.GetConnection();

            await conn.OpenAsync();

            string sql = @"
SELECT
fldAttachmentFileName,
fldAttachmentContentType,
fldAttachment
FROM tbdPriceRange
WHERE fldID=@ID";

            SqlCommand cmd = new(sql, conn);

            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;

            SqlDataReader dr = await cmd.ExecuteReaderAsync();

            if (!await dr.ReadAsync())
                return null;

            if (dr["fldAttachment"] == DBNull.Value)
                return null;

            return new PriceRangeAttachmentDownload
            {
                FileName = dr["fldAttachmentFileName"].ToString() ?? "",
                ContentType = dr["fldAttachmentContentType"].ToString() ?? "",
                FileData = (byte[])dr["fldAttachment"]
            };
        }

    }
}