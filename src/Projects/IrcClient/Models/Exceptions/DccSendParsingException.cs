using System;

namespace IrcClient.Models.Exceptions
{
    public class DccSendParsingException : Exception
    {
        public DccSendParsingException(string message)
            : base($"Invalid DCC Response received: '{message}'")
        {
        }
    }
}
