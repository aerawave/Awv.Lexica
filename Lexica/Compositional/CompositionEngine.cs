using Lexica.Compositional.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using V8.Net;

namespace Lexica.Compositional
{
    public class CompositionEngine : ICompositionEngine
    {
        /// <summary>
        /// The underlying JavaScript engine to use.
        /// </summary>
        private V8Engine Engine { get; set; } = new V8Engine();
        public Dictionary<Type, string> Prescripts { get; set; } = new Dictionary<Type, string>();
        public bool ConvertTimeMarks { get; set; } = false;

        /// <summary>
        /// Registers an <see cref="ILibrary"/> to this engine, so that C# can be called from JavaScript.
        /// </summary>
        /// <typeparam name="TLibrary">A class that derives from <see cref="ILibrary"/></typeparam>
        public void RegisterLibrary<TLibrary>()
            where TLibrary : ILibrary
        {

            var type = typeof(TLibrary);
            var methods = type.GetMethods();
            var staticMethods = new List<MethodInfo>();

            foreach (var method in methods)
                if (method.IsStatic && method.IsPublic)
                    staticMethods.Add(method);

            if (staticMethods.Count > 0)
            {
                Engine.RegisterType<TLibrary>(null, true, ScriptMemberSecurity.Permanent);
                Engine.GlobalObject.SetProperty(typeof(TLibrary));
                var prescript = new List<string>();
                foreach (var method in staticMethods)
                {
                    var parameters = method.GetParameters();
                    var parameterNames = parameters.Select(par => par.Name);
                    var parameterNameList = string.Join(", ", parameterNames);
                    var function = new StringBuilder();
                    function.Append($"function {method.Name}");
                    function.Append($"({parameterNameList})");
                    function.Append("{");
                    function.Append($"return {type.Name}.{method.Name}({parameterNameList});");
                    function.Append("}");
                    prescript.Add(function.ToString());
                }
                Prescripts.Add(type, string.Join("\n", prescript));
            }

        }

        /// <summary>
        /// Executes the given JavaScript <paramref name="script"/> with the underlying <see cref="V8Engine"/>.
        /// </summary>
        /// <param name="script">JavaScript code</param>
        /// <returns>The value provided by the executed code</returns>
        public InternalHandle Execute(string script)
        {
            var prescript = string.Join("\n", Prescripts.Values);

            return Engine.Execute($"{prescript}\n\n{script}", "V8.NET+Prescripts", true, 0, true);
        }

        /// <summary>
        /// Sets a global variable with the given <paramref name="name"/> to the given <paramref name="value"/>.
        /// </summary>
        /// <param name="name">Name of the variable</param>
        /// <param name="value">Value of the variable</param>
        public void SetProperty(string name, object value)
        {
            Engine.GlobalObject.SetProperty(name, Engine.CreateValue(value));
        }

        public bool ShouldConvertTimeMarks() => ConvertTimeMarks;
    }
}
