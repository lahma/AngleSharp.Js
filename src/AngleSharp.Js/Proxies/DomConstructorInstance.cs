namespace AngleSharp.Js
{
    using Jint.Native;
    using Jint.Native.Object;
    using Jint.Runtime;
    using Jint.Runtime.Descriptors;
    using System;
    using System.Reflection;

    sealed class DomConstructorInstance : Constructor
    {
        private readonly ConstructorInfo _constructor;
        private readonly EngineInstance _instance;

        public DomConstructorInstance(EngineInstance engine, Type type)
            : base(engine.Jint, "DOM")
        {
            var objectPrototype = engine.GetDomPrototype(type);
            _instance = engine;
            FastSetProperty("prototype", new PropertyDescriptor(objectPrototype, writable: false, enumerable: false, configurable: false));
            objectPrototype.FastSetProperty("constructor", new PropertyDescriptor(this, writable: true, enumerable: false, configurable: true));
        }

        public DomConstructorInstance(EngineInstance engine, ConstructorInfo constructor)
            : this(engine, constructor.DeclaringType)
        {
            _constructor = constructor;
        }

        public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
        {
            if (_constructor == null)
            {
                throw new JavaScriptException("Illegal constructor.");
            }

            try
            {
                var parameters = _instance.BuildArgs(_constructor, arguments);
                var obj = _constructor.Invoke(parameters);
                return _instance.GetDomNode(obj);
            }
            catch
            {
                throw new JavaScriptException(_instance.Jint.Intrinsics.Error);
            }
        }
    }
}
