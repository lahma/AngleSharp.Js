using Jint.Native;

namespace AngleSharp.Js
{
    using Jint.Native.Object;
    using Jint.Runtime.Descriptors;
    using System;

    sealed class DomNodeInstance : ObjectInstance
    {
        private readonly Object _value;

        public DomNodeInstance(EngineInstance engine, Object value)
            : base(engine.Jint)
        {
            _value = value;

            Prototype = engine.GetDomPrototype(value.GetType());
        }

        public Object Value => _value;

        public override object ToObject() => _value;

        public override PropertyDescriptor GetOwnProperty(JsValue propertyName)
        {
            if (Prototype is DomPrototypeInstance prototype
                && prototype.TryGetFromIndex(_value, propertyName.ToString(), out var descriptor))
            {
                return descriptor;
            }

            return base.GetOwnProperty(propertyName);
        }
    }
}
