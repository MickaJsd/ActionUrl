using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AssemblyJsSerializer
{
    public class ObjectMethodsSerializerGeneric<ClassBaseType, BaseMethodType> : ObjectMethodsSerializer
    {
        public ObjectMethodsSerializerGeneric(Assembly sourceAssembly) : base(sourceAssembly) { }

        public override Func<Type, bool> TypeFilter => (t) => t.BaseType == typeof(ClassBaseType) && t.IsClass;

        public override Func<MethodInfo, bool> MethodFilter => (m) => m.ReturnType == typeof(BaseMethodType) || m.ReturnType.IsSubclassOf(typeof(BaseMethodType));
    }
}
