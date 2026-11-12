using System;

namespace Rijndael.Core.Padding
{
    public class InvalidPaddingException : Exception
    {
        public InvalidPaddingException(string message) : base(message)
        {
            
        }
        public InvalidPaddingException(string message, Exception innerException) : base(message, innerException)
        {
            
        }
    }
}