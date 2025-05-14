namespace CineTicket.Repositories
{
    public interface IGmailSender
    {
        Task SendEmail(string to, string subject, string body);
        Task SendEmailAsync(string email, string subject, string htmlMessage);
        Task SendEmailWithAttachmentAsync(string email, string subject, string htmlMessage, byte[] attachment, string filename);
    }

}
