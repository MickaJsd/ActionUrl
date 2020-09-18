using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AssemblyJsSerializer
{
    public  class MvcActionSerializer : ObjectMethodsSerializer
    {
        const string CONTROLLER_SUFFIX = "Controller";
        public override string GetTypeName(Type t) => t.Name.Replace(CONTROLLER_SUFFIX, "");
    }
}
