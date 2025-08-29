using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace ECommerceWebsite.Models.Helping_Classes
{
    public static class TempDataExtensions
    {
        private const string SuccessKey = "Success";
        private const string ErrorKey = "Error";
        private const string InfoKey = "Info";
        private const string WarningKey = "Warning";

        public static void SetSuccess(this ITempDataDictionary tempData, string message)
        {
            tempData[SuccessKey] = message;
        }

        public static void SetError(this ITempDataDictionary tempData, string message)
        {
            tempData[ErrorKey] = message;
        }

        public static void SetInfo(this ITempDataDictionary tempData, string message)
        {
            tempData[InfoKey] = message;
        }

        public static void SetWarning(this ITempDataDictionary tempData, string message)
        {
            tempData[WarningKey] = message;
        }
    }

}
