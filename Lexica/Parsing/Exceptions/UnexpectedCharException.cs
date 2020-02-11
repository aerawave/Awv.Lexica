using System;

namespace Lexica.Parsing.Exceptions
{
    public class UnexpectedCharException : Exception
    {
        public char UnexpectedChar { get; private set; }
        public int LineNumber { get; private set; }
        public int LineIndex { get; private set; }

        public UnexpectedCharException(char unexpectedChar, int lineNumber, int lineIndex)
            : base($"Unexpected character at index {lineIndex} on line {lineNumber}: {unexpectedChar}")
        {
            UnexpectedChar = unexpectedChar;
            LineNumber = LineNumber;
            LineIndex = lineIndex;
        }
    }
}
