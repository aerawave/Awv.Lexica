using Lexica.Compositional.Interface;

namespace Lexica.Compositional.Lexigrams.Interface
{
    public interface ILexigram
    {
        object GetValue(ICompositionEngine engine);
    }
}
