using System.Data;

namespace OCPPaymentSystemAPI.Helpers
{
    public static class DataReaderExtension
    {
        public static string GetString(this IDataRecord dr, string fieldName)
        {
            return dr[fieldName] == DBNull.Value
                ? string.Empty
                : dr[fieldName].ToString()!;
        }

        public static int GetInt(this IDataRecord dr, string fieldName)
        {
            return dr[fieldName] == DBNull.Value
                ? 0
                : Convert.ToInt32(dr[fieldName]);
        }

        public static decimal GetDecimal(this IDataRecord dr, string fieldName)
        {
            return dr[fieldName] == DBNull.Value
                ? 0
                : Convert.ToDecimal(dr[fieldName]);
        }

        public static DateTime? GetDate(this IDataRecord dr, string fieldName)
        {
            return dr[fieldName] == DBNull.Value
                ? null
                : Convert.ToDateTime(dr[fieldName]);
        }

        public static bool GetBool(this IDataRecord dr, string fieldName)
        {
            return dr[fieldName] != DBNull.Value &&
                   Convert.ToBoolean(dr[fieldName]);
        }
    }
}