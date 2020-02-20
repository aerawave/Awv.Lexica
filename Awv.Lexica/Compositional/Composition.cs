﻿using Awv.Lexica.Compositional.Interface;
using Awv.Lexica.Compositional.Lexigrams.Interface;
using System.Collections.Generic;
using System.Text;

namespace Awv.Lexica.Compositional
{
    public class Composition : List<ILexigram>, ILexigram
    {
        public string Joiner { get; set; } = "";
        /// <summary>
        /// Builds a string, joined with the <see cref="Joiner"/> from each of the child <see cref="ILexigram"/> values with the the <paramref name="engine"/>.
        /// </summary>
        /// <param name="engine">The engine to execute code against</param>
        /// <returns>The compiled string from the engine with the composition</returns>
        public virtual string Build(ICompositionEngine engine)
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

        public virtual object GetValue(ICompositionEngine engine)
            => Build(engine);

        public override string ToString() => string.Join(Joiner, this);
    }
}
