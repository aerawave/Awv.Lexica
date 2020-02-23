using Awv.Lexica.Parsing.Exceptions;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Awv.Lexica.Parsing
{
    public class Parser
    {
        private const char NewLine = '\n';

        private char? cachedCurrentChar;
        private string cachedSourceUntilNow;
        private int? cachedLineNumber;
        private int? cachedLineIndex;

        /// <summary>
        /// A private string used to cache so that <see cref="LineNumber"/> and <see cref="LineIndex"/> are a little mroe efficient.
        /// </summary>
        private string SourceUntilNow => cachedSourceUntilNow = cachedSourceUntilNow ?? Source.Substring(0, CurrentIndex);
        /// <summary>
        /// The source provided at instantiation.
        /// </summary>
        public string Source { get; private set; }
        /// <summary>
        /// The current index in the <see cref="Source"/>
        /// </summary>
        public int CurrentIndex { get; private set; }
        /// <summary>
        /// The current character in the <see cref="Source"/>.
        /// </summary>
        public char CurrentChar => (cachedCurrentChar = cachedCurrentChar.HasValue ? cachedCurrentChar.Value : Source[CurrentIndex]).Value;
        /// <summary>
        /// Length of the <see cref="Source"/>.
        /// </summary>
        public int Length => Source.Length;
        /// <summary>
        /// The index of the current line. *Note: Uses <see cref="NewLine"/>. Some improvement may need to be made.
        /// </summary>
        public int LineNumber => (cachedLineNumber = cachedLineNumber.HasValue ? cachedLineNumber.Value : SourceUntilNow.Split(NewLine).Length).Value;
        /// <summary>
        /// The index of the current character in the current line. *Note: Uses <see cref="NewLine"/>. Some improvement may need to be made.
        /// </summary>
        public int LineIndex => (cachedLineIndex = cachedLineIndex.HasValue ? cachedLineIndex.Value : CurrentIndex - (SourceUntilNow.LastIndexOf(NewLine) + 1)).Value;
        /// <summary>
        /// Whether or not the <see cref="CurrentIndex"/> is equal to or greater than the length of <see cref="Source"/>
        /// </summary>
        public bool EndOfString => CurrentIndex >= Source.Length;

        public Parser(string source) {
            if (string.IsNullOrEmpty(source)) throw new ArgumentNullException(nameof(source));
            Source = source;
            Seek(0, SeekOrigin.Begin);
        }
        
        /// <summary>
        /// Returns the current character in the string and consumes it.
        /// </summary>
        /// <returns>Current character in the string</returns>
        public char ReadChar()
        {
            var ch = PeekChar();
            ConsumeChar();
            return ch;
        }
        /// <summary>
        /// Returns the current character in the string.
        /// </summary>
        /// <returns>Current character in the string</returns>
        public char PeekChar() => CurrentChar;

        /// <summary>
        /// Shorthand for setting the position within the current string forward one from the current position.
        /// </summary>
        public void ConsumeChar()
        {
            try {
                Seek(1, SeekOrigin.Current);
            } catch (IndexOutOfRangeException) {
                throw new UnexpectedEndOfStringException(Source.Length);
            }
        }

        /// <summary>
        /// Sets the position within the current string.
        /// </summary>
        /// <param name="value">A byte offset relative to the <paramref name="origin"/> parameter</param>
        /// <param name="origin">A value of type <see cref="SeekOrigin"/> indicating the reference point used to obtain the new position</param>
        public void Seek(int value, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    break;
                case SeekOrigin.Current:
                    value += CurrentIndex;
                    break;
                case SeekOrigin.End:
                    value = Source.Length - value;
                    break;
            }
            if (value > Source.Length || value < 0)
                throw new IndexOutOfRangeException($"Index {value} is out of range for string of length {Source.Length}");
            CurrentIndex = value;
            ClearCache();
        }

        /// <summary>
        /// Shorthand for setting the position within the current string negatively from the current position.
        /// </summary>
        /// <param name="amount">Amount to move the position by</param>
        public void Back(int amount = 1)
        {
            Seek(-amount, SeekOrigin.Current);
        }

        private void ClearCache()
        {
            cachedCurrentChar = null;
            cachedSourceUntilNow = null;
            cachedLineNumber = null;
            cachedLineIndex = null;
        }
        /// <summary>
        /// Reads characters into a string so long as the consumed character is whitespace, and the <see cref="Parser"/> is not at its <see cref="EndOfString"/>
        /// </summary>
        /// <returns>The read string</returns>
        public string SkipWhitespace() => ReadWhile((ch) => char.IsWhiteSpace(ch));
        /// <summary>
        /// Reads characters into a string so long as the consumed character is not whitespace, and the <see cref="Parser"/> is not at its <see cref="EndOfString"/>
        /// </summary>
        /// <returns>The read string</returns>
        public string ReadUntilWhitespace() => ReadWhile((ch) => !char.IsWhiteSpace(ch));
        /// <summary>
        /// Reads characters into a string so long as the consumed character is not contained within the <paramref name="expectedChars"/>, is not whitespace, and the <see cref="Parser"/> is not at its <see cref="EndOfString"/>
        /// </summary>
        /// <param name="expectedChars">Characters to expect if whitespace is not encountered</param>
        /// <returns>The read string</returns>
        public string ReadUntilWhitespaceOrAny(params char[] expectedChars) => ReadWhile((ch) => !(char.IsWhiteSpace(ch) || expectedChars.Contains(ch)));
        /// <summary>
        /// Reads characters into a string so long as the consumed character is not contained within the <paramref name="expectedChars"/>, and the <see cref="Parser"/> is not at its <see cref="EndOfString"/>
        /// </summary>
        /// <param name="expectedChars">Characters to expect</param>
        /// <returns>The read string</returns>
        public string ReadUntilAny(params char[] expectedChars) => ReadWhile((ch) => !expectedChars.Contains(ch));
        /// <summary>
        /// Reads characters into a string so long as the <paramref name="predicate"/> serves true, and the <see cref="Parser"/> is not at its <see cref="EndOfString"/>
        /// </summary>
        /// <param name="predicate">The predicate against which to check a given string and character for continuation.</param>
        /// <returns>The read string</returns>
        public string ReadWhile(Func<char, bool> predicate) => ReadWhile((str, ch) => predicate(ch));

        /// <summary>
        /// Reads characters into a string so long as the <paramref name="predicate"/> serves true, and the <see cref="Parser"/> is not at its <see cref="EndOfString"/>
        /// </summary>
        /// <param name="predicate">The predicate against which to check a given string and character for continuation.</param>
        /// <returns>The read string</returns>
        public string ReadWhile(Func<string, char, bool> predicate)
        {
            var parsing = true;
            var parsed = new StringBuilder();
            while (parsing && !EndOfString) {
                var ch = PeekChar();
                parsing = predicate(parsed.ToString(), ch);
                if (parsing) {
                    ConsumeChar();
                    parsed.Append(ch);
                }
            }

            return parsed.ToString();
        }
        
        /// <summary>
        /// Reads the current character, then the next stream of characters to the length of <paramref name="expectedString"/>. If any character is not the next expected one, and the expectation is <paramref name="optional"/>, then the <see cref="CurrentIndex"/> is reset. However if the expectation is not <paramref name="optional"/>, then the an <see cref="UnexpectedCharException"/> is thrown.
        /// </summary>
        /// <param name="expectedString">String of characters to expect.</param>
        /// <param name="optional">Whether or not one of the expected characters is requried.</param>
        /// <returns>Whether or not the expected string was encountered.</returns>
        public bool Expect(string expectedString, bool optional = false)
        {
            var startIndex = CurrentIndex;
            var chars = expectedString.ToCharArray();
            var found = true;

            foreach (var ch in chars)
                found = found && Expect(ch, optional).HasValue;

            if (!found)
                CurrentIndex = startIndex;

            return found;
        }

        /// <summary>
        /// Reads the current character and checks if it is the <paramref name="expectedChar"/>. If it is, that character is consumed and returned. If it's not, null will be returned an <see cref="UnexpectedCharException"/> will be thrown depending on if the value is <paramref name="optional"/>.
        /// </summary>
        /// <param name="expectedChar">The character that is acceptable.</param>
        /// <param name="optional">Whether or not one of the expected characters is required.</param>
        /// <returns>The encountered character or null</returns>
        public char? Expect(char expectedChar, bool optional = false) => ExpectAny(optional, expectedChar);
        /// <summary>
        /// Reads the current character and checks if it is contained within the <paramref name="expectedChars"/>. If it is, that character is consumed and returned. If it's not, an <see cref="UnexpectedCharException"/> is thrown.
        /// </summary>
        /// <param name="expectedChars">The array of characters that are acceptable.</param>
        /// <returns>The encountered character or null</returns>
        public char? ExpectAny(params char[] expectedChars) => ExpectAny(false, expectedChars);

        /// <summary>
        /// Reads the current character and checks if it is contained within the <paramref name="expectedChars"/>. If it is, that character is consumed and returned. If it's not, null will be returned an <see cref="UnexpectedCharException"/> will be thrown depending on if the value is <paramref name="optional"/>.
        /// </summary>
        /// <param name="optional">Whether or not one of the expected characters is required.</param>
        /// <param name="expectedChars">The array of characters that are acceptable.</param>
        /// <returns>The encountered character or null</returns>
        public char? ExpectAny(bool optional, params char[] expectedChars)
        {
            if (EndOfString && optional) return null;

            var found = expectedChars.Contains(CurrentChar);

            if (!found && !optional)
                throw new UnexpectedCharException(CurrentChar, LineNumber, LineIndex);

            if (found) return ReadChar();

            return null;
        }
    }
}