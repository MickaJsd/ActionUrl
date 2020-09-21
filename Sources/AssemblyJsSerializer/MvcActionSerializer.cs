using System;

namespace AssemblyJsSerializer
{
    public class MvcActionSerializer : ObjectMethodsSerializer
    {
        const string CONTROLLER_SUFFIX = "Controller";
        public override string GetTypeName(Type t) => t.Name.Replace(CONTROLLER_SUFFIX, "");
    }
}
