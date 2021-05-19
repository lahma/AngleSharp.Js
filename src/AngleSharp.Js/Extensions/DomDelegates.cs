using Jint;

namespace AngleSharp.Js
{
    using AngleSharp.Dom;
    using AngleSharp.Dom.Events;
    using Jint.Native.Function;
    using Jint.Runtime;
    using System;
    using System.Reflection;

    static class DomDelegates
    {
        private static readonly Type[] ToCallbackSignature = new[] { typeof(Function), typeof(EngineInstance) };

        public static Delegate ToDelegate(this Type type, Function function, EngineInstance engine)
        {
            if (type != typeof(DomEventHandler))
            {
                var method = typeof(DomDelegates).GetRuntimeMethod("ToCallback", ToCallbackSignature).MakeGenericMethod(type);
                return method.Invoke(null, new Object[] { function, engine }) as Delegate;
            }

            return function.ToListener(engine);
        }

        public static DomEventHandler ToListener(this Function function, EngineInstance engine) => (obj, ev) =>
        {
            var objAsJs = obj.ToJsValue(engine);
            var evAsJs = ev.ToJsValue(engine);

            try
            {
                function.Call(objAsJs, new[] { evAsJs });
            }
            catch (JavaScriptException jsException)
            {
                var window = (IWindow)engine.Window.Value;
                window.Fire<ErrorEvent>(e => e.Init(null, jsException.Location.Start.Line, jsException.Location.Start.Column, jsException));
            }
        };
    }
}
