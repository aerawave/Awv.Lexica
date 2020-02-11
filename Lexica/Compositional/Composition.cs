using Lexica.Compositional.Interface;
using Lexica.Compositional.Lexigrams.Interface;
using System.Collections.Generic;
using System.Text;

namespace Lexica.Compositional
{
    public class Composition : List<ILexigram>
    {
        public string Joiner { get; set; } = "";
        /// <summary>
        /// Builds a string, joined with the <see cref="Joiner"/> from each of the child <see cref="ILexigram"/> values with the the <paramref name="engine"/>.
        /// </summary>
        /// <param name="engine">The engine to execute code against</param>
        /// <returns>The compiled string from the engine with the composition</returns>
        public string Build(ICompositionEngine engine)
        {
            var built = new StringBuilder();
            foreach (var token in this)
            {
                var value = token.GetValue(engine);
                built.Append(value.ToString());
                if (token is IIdLexigram)
                {
                    var id = (token as IIdLexigram).Id;
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        engine.SetProperty(id, value);
                    }
                }
            }
            return string.Join(Joiner, built);
        }

        public override string ToString() => string.Join(Joiner, this);
    }
}
