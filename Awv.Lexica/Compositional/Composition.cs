using Awv.Lexica.Compositional.Interface;
using Awv.Lexica.Compositional.Lexigrams.Interface;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Awv.Lexica.Compositional
{
    public class Composition : IList<ILexigram>, ILexigram
    {
        private List<ILexigram> InternalList { get; set; } = new List<ILexigram>();

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

        #region IList<ILexigram> Forwarding Implementation
        public ILexigram this[int index] { get => InternalList[index]; set => InternalList[index] = value; }
        public int Count => InternalList.Count;
        public bool IsReadOnly => false;
        public void Add(ILexigram item) => InternalList.Add(item);
        public void Clear() => InternalList.Clear();
        public bool Contains(ILexigram item) => InternalList.Contains(item);
        public void CopyTo(ILexigram[] array, int arrayIndex) => InternalList.CopyTo(array, arrayIndex);
        public IEnumerator<ILexigram> GetEnumerator() => InternalList.GetEnumerator();
        public int IndexOf(ILexigram item) => InternalList.IndexOf(item);
        public void Insert(int index, ILexigram item) => InternalList.Insert(index, item);
        public bool Remove(ILexigram item) => InternalList.Remove(item);
        public void RemoveAt(int index) => InternalList.RemoveAt(index);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion

    }
}
