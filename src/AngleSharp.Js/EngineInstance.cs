using AngleSharp.Attributes;
using Jint.Runtime.Interop;

namespace AngleSharp.Js
{
    using AngleSharp.Dom;
    using Jint;
    using Jint.Native;
    using Jint.Native.Object;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    sealed class EngineInstance
    {
        #region Fields

        private Engine _engine;
        private AngleSharpHost _host;

        private static readonly TypeResolver _typeResolver = new()
        {
            MemberNameCreator = MemberNameCreator
        };

        internal static IEnumerable<string> MemberNameCreator(MemberInfo member)
        {
            var found = false;
            var attributes = member.GetCustomAttributes(typeof(DomNameAttribute), inherit: true);
            foreach (DomNameAttribute attribute in attributes)
            {
                found = true;
                yield return attribute.OfficialName;
            }

            // check interfaces
            foreach (var interfaceType in member.DeclaringType.GetInterfaces())
            {
                var memberInfos = interfaceType.GetMember(member.Name);
                foreach (var interfaceMember in memberInfos)
                {
                    attributes = interfaceMember.GetCustomAttributes(typeof(DomNameAttribute), inherit: true);
                    foreach (DomNameAttribute attribute in attributes)
                    {
                        found = true;
                        yield return attribute.OfficialName;
                    }
                }
            }

            if (!found)
            {
                yield return member.Name;
            }
        }

        #endregion

        #region ctor

        public EngineInstance(IWindow window, IDictionary<String, Object> assignments, IEnumerable<Assembly> libs)
        {
            var context = window.Document.Context;
            var logger = context.GetService<IConsoleLogger>();

            _engine = new Engine((engine, options) =>
            {
                _engine = engine;
                options.UseHostFactory(_ =>
                {
                    _host = new AngleSharpHost(
                        this,
                        window,
                        assignments,
                        libs,
                        logger);

                    return _host;
                });
                options.SetTypeResolver(_typeResolver);
                options.Interop.ValueCoercion = ValueCoercionType.All;
                options.SetWrapObjectHandler((e, target, type) => _host.GetDomNode(target));
            });

        }

        #endregion

        #region Properties

        public IEnumerable<Assembly> Libs => _host.Libs;

        public DomNodeInstance Window => _host.Window;

        public Engine Jint => _engine;

        #endregion

        #region Methods

        public DomNodeInstance GetDomNode(Object obj) => _host.GetDomNode(obj);

        public ObjectInstance GetDomPrototype(Type type) => _host.GetDomPrototype(type);

        public JsValue RunScript(String source, JsValue context)
        {
            lock (_engine)
            {
                var result = _engine.Evaluate(source);
                return result;
            }
        }

        #endregion
    }
}
