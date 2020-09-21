using System;
using System.Reflection;

namespace AssemblyJsSerializer
{
    public class MvcActionSerializer<TypeController, TypeActionresult> : ObjectMethodsSerializerGeneric<TypeController, TypeActionresult>
    {
        const string CONTROLLER_SUFFIX = "Controller";

        public MvcActionSerializer(Assembly sourceAssembly) : base(sourceAssembly) { }

        public override string GetTypeName(Type t) => t.Name.Replace(CONTROLLER_SUFFIX, "");
    }
}
