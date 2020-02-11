using V8.Net;

namespace Awv.Lexica.Compositional.Interface
{
    public interface ICompositionEngine
    {
        InternalHandle Execute(string script);
        void SetProperty(string name, object value);
        bool ShouldConvertTimeMarks();
    }
}
