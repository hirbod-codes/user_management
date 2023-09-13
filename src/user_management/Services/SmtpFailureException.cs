using System.Runtime.Serialization;

namespace user_management.Services
{
    [Serializable]
    public class SmtpFailureException : Exception
    {
        public SmtpFailureException()
        {
        }

        public SmtpFailureException(string? message) : base(message)
        {
        }

        public SmtpFailureException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected SmtpFailureException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}