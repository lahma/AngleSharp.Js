using System;
using System.Collections.Generic;
using System.Reflection;
using AngleSharp.Dom;
using Jint.Native.Object;
using Jint.Runtime;
using Jint.Runtime.Descriptors;

namespace AngleSharp.Js
{
    internal sealed class AngleSharpHost : Host
    {
        private readonly EngineInstance _engineInstance;
        private readonly PrototypeCache _prototypes;
        private readonly ReferenceCache _references;
        private readonly IWindow _window;
        private readonly IDictionary<string, object> _assignments;
        private readonly IConsoleLogger _logger;

        public AngleSharpHost(
            EngineInstance engineInstance,
            IWindow window,
            IDictionary<string, object> assignments,
            IEnumerable<Assembly> libs,
            IConsoleLogger logger)
        {
            _engineInstance = engineInstance;
            _window = window;
            _assignments = assignments;
            Libs = libs;
            _logger = logger;
            _references = new ReferenceCache();
            _prototypes = new PrototypeCache(engineInstance.Jint);
        }

        public DomNodeInstance Window { get; private set; }
        public IEnumerable<Assembly> Libs { get; }

        protected override ObjectInstance CreateGlobalObject(Realm realm)
        {
            var global = base.CreateGlobalObject(realm);
            Window = GetDomNode(_window);
            Window.Prototype = realm.Intrinsics.Object;
            global.FastSetProperty("window", new PropertyDescriptor(Window, true, false, true));
            return global;
        }

        protected override void PostInitialize()
        {
            _engineInstance.Jint.SetValue("console", new ConsoleInstance(Engine, _logger));

            foreach (var property in _window.GetType().GetProperties())
            {
                foreach (var name in EngineInstance.MemberNameCreator(property))
                {
                    _engineInstance.Jint.SetValue(name, property.GetValue(_window));
                }
            }

            foreach (var assignment in _assignments)
            {
                _engineInstance.Jint.SetValue(assignment.Key, assignment.Value);
            }

            foreach (var lib in Libs)
            {
                _engineInstance.AddConstructors(Window, lib);
                _engineInstance.AddInstances(Window, lib);
            }

        }

        public DomNodeInstance GetDomNode(Object obj) => _references.GetOrCreate(obj, CreateInstance);
        public ObjectInstance GetDomPrototype(Type type) => _prototypes.GetOrCreate(type, CreatePrototype);

        private DomNodeInstance CreateInstance(Object obj) => new DomNodeInstance(_engineInstance, obj);
        private ObjectInstance CreatePrototype(Type type) => new DomPrototypeInstance(_engineInstance, type);
    }
}