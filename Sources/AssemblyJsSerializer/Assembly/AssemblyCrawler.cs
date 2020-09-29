using AssemblyJsSerializer.Configuration;
using AssemblyJsSerializer.Error;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using TypeFilterSetting = AssemblyJsSerializer.Configuration.TypeFilter;

namespace AssemblyJsSerializer.Assemblies
{
    internal class AssemblyCrawler : NestedErrorHandler
    {
        public Assembly SourceAssembly
        {
            get;
        }

        public AssemblyCrawler(Assembly sourceAssembly, Filters filters)
        {
            this.SourceAssembly = sourceAssembly;
            this.InitFilters(filters);
        }

        private void InitFilters(Filters filters)
        {
            this.Filters = filters;
            this.SetTypeFilter();
            this.SetMethodFilter();

        }

        public Filters Filters
        {
            get;
            private set;
        }

        private IEnumerable<Type> GetTypesByName(IEnumerable<string> typeNames)
        {
            var baseTypesByName = typeNames.ToDictionary(typeName => typeName, typeName=> Type.GetType(typeName, false, true));
            var nullTypes = baseTypesByName.Where(kv => kv.Value == null);
            if (nullTypes.Any())
            {
                this.Errors.Add($"Des types configurés n'ont pas pu être résolus : [{string.Join(";", nullTypes.Select(kv => kv.Key))}]");
            }
            return baseTypesByName.Where(kv => kv.Value != null).Select(kv => kv.Value);
        }


        private IEnumerable<Type> ReadTypes()
        {
            return this.SourceAssembly.GetTypes().Where(DoFilterType);
        }

        private IEnumerable<MethodInfo> ReadMethods(Type type)
        {
            return type.GetMethods().Where(DoMethodFilter);
        }

        #region Method Filter        

        private IEnumerable<Type> ReturnTypes
        {
            get; set;
        }

        private MethodFilter MethodFilter
        {
            get
            {
                return this.Filters.MethodFilter;
            }
        }

        private void SetMethodFilter()
        {
            if (this.MethodFilter != null)
            {
                if (this.MethodFilter.ReturnType.Any())
                {
                    this.ReturnTypes = GetTypesByName(this.MethodFilter.ReturnType);
                }
                this.DoMethodFilter = ConfiguredMethodFilter;
            }
            else
            {
                this.DoMethodFilter = this.DefaultMethodFilter;
            }
        }

        private bool DefaultMethodFilter(MethodInfo method) => true;

        private bool ConfiguredMethodFilter(MethodInfo method)
        {
            bool result = true;

            result &= this.ReturnTypes == null || !this.ReturnTypes.Any() || this.ReturnTypes.Any(returnType => method.ReturnType.IsSubclassOf(returnType) || method.ReturnType == returnType);

            return result;
        }

        private Func<MethodInfo, bool> DoMethodFilter
        {
            get; set;
        }

        #endregion

        #region Type Filter

        private void SetTypeFilter()
        {
            if (this.TypeFilter != null)
            {
                if (this.TypeFilter.BaseType.Any())
                {
                    this.BaseTypes = GetTypesByName(this.TypeFilter.BaseType);
                }
                this.DoFilterType = ConfiguredTypeFilter;
            }
            else
            {
                this.DoFilterType = this.DefautlTypeFilter;
            }
        }

        private IEnumerable<Type> BaseTypes
        {
            get; set;
        }

        private TypeFilterSetting TypeFilter
        {
            get
            {
                return this.Filters.TypeFilter;
            }
        }

        private bool DefautlTypeFilter(Type type) => true;

        private bool ConfiguredTypeFilter(Type type)
        {
            bool result = true;

            result &= this.TypeFilter.IsClass == null || this.TypeFilter.IsClass == type.IsClass;

            result &= this.BaseTypes == null || !this.BaseTypes.Any() || this.BaseTypes.Any(baseType => type.IsSubclassOf(baseType) || type == baseType);

            return result;
        }

        private Func<Type, bool> DoFilterType
        {
            get; set;
        }

        #endregion

        internal IEnumerable<FilteredType> FilterAssemblyContent()
        {
            List<FilteredType> result = new List<FilteredType>();
            try
            {
                foreach (Type type in this.ReadTypes())
                {
                    var filteredType = new FilteredType
                    {
                        Type = type,
                        Methods = this.ReadMethods(type)
                    };

                    if (filteredType.Methods.Any())
                    {
                        result.Add(filteredType);
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                this.AddExceptionError($"une erreur est survenue lors du parcours de l'assembly {SourceAssembly.GetName()}", e);
                return new FilteredType[] { };
            }
        }

    }
}
