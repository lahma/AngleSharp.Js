using Jint.Runtime.Descriptors;

namespace AngleSharp.Js
{
    using Jint.Native.Function;
    using Jint.Native.Object;
    using System.Reflection;

    partial class DomConstructors
    {
        private readonly EngineInstance _engine;

        public DomConstructors(EngineInstance engine)
        {
            Object = engine.Jint.Intrinsics.Object;
            _engine = engine;
        }

        public ObjectConstructor Object
        {
            get;
            private set;
        }

        public void Configure() => Setup(_engine);

        public void AttachConstructors(ObjectInstance obj)
        {
            var properties = GetType().GetTypeInfo().DeclaredProperties;

            foreach (var property in properties)
            {
                var func = property.GetValue(this) as Function;
                obj.FastSetProperty(property.Name, new PropertyDescriptor(func, writable: true, enumerable: false, configurable: true));
            }
        }

        partial void Setup(EngineInstance engine);
    }
}
