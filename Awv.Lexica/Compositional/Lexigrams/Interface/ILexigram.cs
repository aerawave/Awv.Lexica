using Awv.Lexica.Compositional.Interface;

namespace Awv.Lexica.Compositional.Lexigrams.Interface
{
    public interface ILexigram
    {
        object GetValue(ICompositionEngine engine);
    }
}
