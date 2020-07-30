namespace IrcClient.Models
{
    public class IrcMessage
    {
        public IrcMessage(string message, string sender)
        {
            Message = message;
            Sender = sender;
        }

        public string Message { get; private set; }

        public string Sender { get; private set; }
    }
}
