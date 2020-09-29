using System.Collections.Generic;

namespace AssemblyJsSerializer.Configuration
{
    internal class TypeFilter
    {
        public bool? IsClass
        {
            get; set;
        }

        public IEnumerable<string> BaseType
        {
            get; set;
        }
    }
}