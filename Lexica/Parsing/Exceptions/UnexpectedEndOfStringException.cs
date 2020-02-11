using System;

namespace Lexica.Parsing.Exceptions
{
    public class UnexpectedEndOfStringException : Exception
    {
        public int SourceLength { get; private set; }

        public UnexpectedEndOfStringException(int sourceLength)
            : base($"Unexpected end of string")
        {
            SourceLength = sourceLength;
        }
    }
}
