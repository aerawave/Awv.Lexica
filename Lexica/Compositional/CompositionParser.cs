using Lexica.Compositional.Lexigrams;
using Lexica.Compositional.Lexigrams.Interface;
using Lexica.Parsing;
using System.Text;

namespace Lexica.Compositional
{
    public class CompositionParser : Parser
    {
        private const char CodeOpener = '`';
        private const char CodeCloser = '`';
        private const char EscapeChar = '\\';
        private const char IdOpener = '(';
        private const char IdCloser = ')';

        public CompositionParser(string source) : base(source)
        {
        }

        /// <summary>
        /// Transpiles a <see cref="Composition"/> consisting of every <see cref="ILexigram"/> found, one after the other.
        /// </summary>
        /// <returns>The transpiled composition</returns>
        public Composition Transpile()
        {
            var tokens = new Composition();

            while (!EndOfString) tokens.Add(ReadNext());

            return tokens;
        }

        /// <summary>
        /// Reads the next <see cref="ILexigram"/>. This could be a <see cref="Lexigram"/> or a <see cref="CodeLexigram"/>.
        /// </summary>
        /// <returns>The next <see cref="ILexigram"/></returns>
        public ILexigram ReadNext()
        {
            ILexigram output;
            if (Expect(CodeOpener, true).HasValue)
            {
                output = ReadCode();
            } else
            {
                output = ReadString();
            }
            return output;
        }

        /// <summary>
        /// Reads until either at end of string, or until a <see cref="CodeOpener"/> is found. If an <see cref="EscapeChar"/> is found, the next character is read regardless.
        /// </summary>
        /// <returns>A <see cref="Lexigram"/> of the provided string</returns>
        public ILexigram ReadString()
        {
            var parsing = true;
            var parsed = new StringBuilder();
            while (parsing)
            {
                var ch = ReadChar();
                parsing = !EndOfString;
                switch (ch)
                {
                    case EscapeChar:
                        ch = ReadChar();
                        parsed.Append(ch);
                        break;
                    default:
                        parsed.Append(ch);
                        break;
                    case CodeOpener:
                        parsing = false;
                        break;
                }
            }
            if (!EndOfString) Back();

            return (Lexigram)parsed.ToString();
        }

        /// <summary>
        /// Reads a string until a <see cref="CodeCloser"/> is found. Then checks for an <see cref="IdOpener"/>. If one is found, a string is read until an <see cref="IdCloser"/> is found to provide an ID.
        /// </summary>
        /// <returns>A <see cref="CodeLexigram"/> of the provided code</returns>
        public ILexigram ReadCode()
        {
            var code = ReadUntilAny(CodeCloser);
            Expect(CodeCloser);
            var id = (string)null;

            if (Expect(IdOpener, true).HasValue)
            {
                id = ReadUntilAny(IdCloser);
                Expect(IdCloser);
            }

            return new CodeLexigram(id, code);
        }
    }
}
