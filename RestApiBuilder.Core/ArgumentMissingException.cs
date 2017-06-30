using System;

namespace RestApiClientBuilder.Core
{
    public class ArgumentMissingException : Exception
    {
        public ArgumentMissingException(string message)
            : base(message)
        {

        }
    }
}
