using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace ECommerceWebsite.Models.Helping_Classes
{
    public static class TempDataExtensions
    {
        public static void SetSuccess(this ITempDataDictionary tempData, string message)
        {
            tempData["Success"] = message;
        }

        public static void SetError(this ITempDataDictionary tempData, string message)
        {
            tempData["Error"] = message;
        }

        public static void SetInfo(this ITempDataDictionary tempData, string message)
        {
            tempData["Info"] = message;
        }

        public static void SetWarning(this ITempDataDictionary tempData, string message)
        {
            tempData["Warning"] = message;
        }
    }
}