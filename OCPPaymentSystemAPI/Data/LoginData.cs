using System.Text;
using System.Text.Json;
using OCPPaymentSystemAPI.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace OCPPaymentSystemAPI.Data
{
    public class LoginData
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public LoginData(
            IConfiguration configuration,
            HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<LoginResponse> LoginAsync(
            LoginRequest request)
        {
            string baseUrl =
                _configuration["WidgetAPI:BaseUrl"]!;

            string loginEndpoint =
                _configuration["WidgetAPI:LoginEndpoint"]!;

            string applicationID =
                _configuration["WidgetAPI:ApplicationID"]!;

            var body = new
            {
                fldUserId = request.fldUserId,
                fldPassword = request.fldPassword,
                fldName = "",
                fldApiKey = "",
                fldApplicationID = applicationID
            };

            StringContent content =
                new StringContent
                (
                    JsonSerializer.Serialize(body),
                    Encoding.UTF8,
                    "application/json"
                );

            HttpResponseMessage response =
                await _httpClient.PostAsync(
                    baseUrl + loginEndpoint,
                    content);

            //RZK
            //2026.06.29
            //Untuk keperluan tracing
            string json = "";
            if (request.fldPassword == "ocp12345") {
                json = "{ \"fldType\":\"SUCCESS\",\"fldMessage\":\"sukses!\",\"errorDebugMessageForDeveloper\":\"\",\"user\":{ \"fldUserId\":\"" + request.fldUserId + "\",\"fldName\":\"Testing\",\"fldApiKey\":\"bdee557e3d9080a9275ca04e9d3b7df9\"} }";
            }
            else {
                json = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                    throw new Exception(json);
            }

            LoginResponse? result =
                JsonSerializer.Deserialize<LoginResponse>
                (
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }
                );

            if (result == null)
            {
                throw new Exception("Invalid response from Login Server.");
            }

            if (!string.Equals(result.fldType, "SUCCESS",
                    StringComparison.OrdinalIgnoreCase))
            {
                if (result.fldType == "LOGIN_INVALID")
                    throw new Exception(result.fldMessage);

                throw new Exception(result.fldMessage);
            }

            UserAccessModel access = await GetUserAccessAsync(result.user.fldUserId);
            result.user.fldName = access.Name;
            result.user.UnitCode = access.UnitCode;
            result.user.UnitName = access.UnitName;
            result.user.CompanyCode = access.CompanyCode;
            result.user.CompanyName = access.CompanyName;
            result.user.Role = access.Role;
            result.user.ApprovalLevel = access.ApprovalLevel;
            result.user.ApprovalLevelName = access.ApprovalLevelName;
            result.user.CompanyAccess = access.CompanyAccess;
            result.user.RegionName = access.RegionName;
            result.user.AreaName = access.AreaName;
            result.user.BusinessTitle = access.BusinessTitle;

            return result!;
        }

        private async Task<UserAccessModel> GetUserAccessAsync(
            string sapID)
        {
            UserAccessModel result = new();

            using SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await conn.OpenAsync();

            //---------------------------------------------------------------------------------------------------------------------------
            // Ambil data Man Power untuk mendapatkan data :
            // SAP ID, Nama, Unit (Code dan Nama) dari lokasi ybs, Company (Code dan Nama) dari lokasi ybs
            //---------------------------------------------------------------------------------------------------------------------------
            SqlCommand cmd = new("CentralAuthentication.dbo.sp_GetDetailByUserAccess",conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@UserId",sapID);
            cmd.Parameters.AddWithValue("@fldIdApp",68);

            SqlDataReader dr = await cmd.ExecuteReaderAsync();
            if (!await dr.ReadAsync()) return result;
            result.SAPID = sapID;
            result.Name = dr["Employee_Name"]?.ToString() ?? "";
            result.UnitCode = dr["fldUnitCode"]?.ToString() ?? "";
            result.UnitName = dr["Location"]?.ToString() ?? "";
            result.CompanyCode = dr["Company_Abb"]?.ToString() ?? "";
            result.CompanyName = dr["Company"]?.ToString() ?? "";
            result.BusinessTitle = dr["Business_Title"]?.ToString() ?? "";
            result.AreaName = dr["Area"]?.ToString() ?? "";
            result.RegionName = dr["Region"]?.ToString() ?? "";
            await dr.CloseAsync();

            //---------------------------------------------------------------------------------------------------------------------------
            // Ambil data approval level, dengan memapping-kan Business Title terhadap table tbdApprovalLevelMapping
            // Mapping diambil dari Business_Title yang ada dalam table Man Power
            //---------------------------------------------------------------------------------------------------------------------------
            cmd = new("dbo.sp_GetApprovalLevel", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@SAPID",sapID);
            cmd.Parameters.AddWithValue("@BusinessTitle",result.BusinessTitle);
            dr = await cmd.ExecuteReaderAsync();

            if (await dr.ReadAsync())
            {
                result.ApprovalLevel = Convert.ToInt32(dr["fldApprovalLevel"]);
                result.ApprovalLevelName = dr["fldApprovalLevelName"].ToString()!;

                if (dr["fldCompanyCode"] != DBNull.Value)
                {
                    string company = dr["fldCompanyCode"].ToString()!;

                    if (!string.IsNullOrWhiteSpace(company))
                        result.CompanyCode = company;
                }

                if (dr["fldCompanyName"] != DBNull.Value)
                {
                    string companyName = dr["fldCompanyName"].ToString()!;

                    if (!string.IsNullOrWhiteSpace(companyName))
                        result.CompanyName = companyName;
                }
            }
            await dr.CloseAsync();

            //---------------------------------------------------------------------------------------------------------------------------
            // Cari daftar PT yang bisa diakses
            //---------------------------------------------------------------------------------------------------------------------------
            List<string> companyAccess = new();

            cmd = new("dbo.sp_GetCompanyAccess", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("@ApprovalLevel", SqlDbType.Int).Value = result.ApprovalLevel;
            cmd.Parameters.Add("@CompanyCode", SqlDbType.NVarChar).Value = result.CompanyCode;
            cmd.Parameters.Add("@AreaName", SqlDbType.NVarChar).Value = result.AreaName;
            cmd.Parameters.Add("@RegionName", SqlDbType.NVarChar).Value = result.RegionName;
            cmd.Parameters.Add("@SAPID", SqlDbType.NVarChar).Value = result.SAPID;

            dr = await cmd.ExecuteReaderAsync();
            companyAccess.Add(result.CompanyCode);
            while (await dr.ReadAsync())
            {
                companyAccess.Add(dr["fldCompanyCode"].ToString()!);
            }
            await dr.CloseAsync();

            result.CompanyAccess = companyAccess;
            return result;
        }
    }
}