using System;

namespace VideoConverter.Common
{
    public static class ExceptionExtensions
    {
        public static string GetExtendedMessage(this Exception e, string description)
        {
            string message = e.Message;

            if (!string.IsNullOrEmpty(description))
                message = $"{description}: {message}";

            if (!(e.InnerException is null))
                message = $"{message} ({e.InnerException.Message})";

            return message;          
        }
    }
}
