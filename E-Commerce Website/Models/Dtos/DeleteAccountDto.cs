namespace ECommerceWebsite.Models.Dtos
{
    public class DeleteAccountDto
    {
        public string ConfirmationPassword { get; set; } = string.Empty;
        public bool ConfirmDeletion { get; set; }
    }
}