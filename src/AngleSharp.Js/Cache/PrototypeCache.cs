namespace AngleSharp.Js
{
    using Jint;
    using Jint.Native.Object;
    using System;
    using System.Collections.Concurrent;

    sealed class PrototypeCache
    {
        private readonly Engine _engine;
        private readonly ConcurrentDictionary<Type, ObjectInstance> _prototypes;

        public PrototypeCache(Engine engine)
        {
            _engine = engine;
            _prototypes = new ConcurrentDictionary<Type, ObjectInstance>();
        }

        public ObjectInstance GetOrCreate(Type type, Func<Type, ObjectInstance> creator)
        {
            if (type == typeof(object))
            {
                return _engine.Intrinsics.Object.PrototypeObject;
            }
            return _prototypes.GetOrAdd(type, creator.Invoke);
        }
    }
}
