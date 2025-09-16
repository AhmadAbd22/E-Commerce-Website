using DnsClient;
using System.Net.Mail;

namespace ECommerceWebsite.Models.Helping_Classes
{
    public static class EmailValidationHelper
    {
        public static bool IsEmailFormatValid(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> IsDomainValidAsync(string email)
        {
            try
            {
                var domain = email.Split('@')[1];
                var lookup = new LookupClient();
                var result = await lookup.QueryAsync(domain, QueryType.MX);
                return result.Answers.MxRecords().Any();
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> IsEmailValidAsync(string email)
        {
            return IsEmailFormatValid(email) && await IsDomainValidAsync(email);
        }
    }
}
