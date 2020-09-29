using System;
using System.Collections.Generic;
using System.Reflection;

namespace AssemblyJsSerializer.Assemblies
{
    internal class FilteredType
    {

        public Type Type
        {
            get; set;
        }

        public IEnumerable<MethodInfo> Methods
        {
            get; set;
        }
    }
}
