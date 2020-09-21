using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AssemblyJsSerializer
{
    public class ObjectMethodsSerializer
    {

        public virtual string GetTypeName(Type t) => t.Name;
        public virtual string GetMethodName(MethodInfo m) => m.Name;
        public string Serialize(Assembly assembly,
                       Func<Type, bool> controllerSelector,
                       Func<MethodInfo, bool> actionSelector,
                       Func<Type, MethodInfo, string> contentGenerator)
        {
            Func<string, string> getJsFieldName = (name) => $"'{name}'";

            Func<StringBuilder> startNewObjectBuilder = () => new StringBuilder("{");
            Func<StringBuilder, string> endObjectToString = (sb) => sb.Append("}").ToString();
            Action<StringBuilder, string, string> appendNewField =
                (sb, fieldName, content) => sb.Append($"{getJsFieldName(fieldName)}:{content},");

            StringBuilder sbType = startNewObjectBuilder();
            assembly
                .GetTypes()
                .Where(controllerSelector)
                .ToList()
                .ForEach(type =>
                {
                    StringBuilder sbMethod = startNewObjectBuilder();
                    bool hasActions = false;

                    type
                        .GetMethods()
                        .Where(actionSelector)
                        .ToList()
                        .ForEach(method =>
                        {
                            hasActions = true;
                            appendNewField(sbMethod, this.GetMethodName(method), contentGenerator(type, method));
                        });

                    if (hasActions)
                    {
                        appendNewField(sbType, this.GetTypeName(type), endObjectToString(sbMethod));
                    }

                });

            return endObjectToString(sbType);
        }

        public string SerializeGeneric<ControllerBaseType, ActionResultType>(Assembly assembly, Func<Type, MethodInfo, string> contentGenerator)
        {
            return Serialize(assembly,
                        (t) => t.BaseType == typeof(ControllerBaseType) && t.IsClass,
                        (m) => m.ReturnType == typeof(ActionResultType) || m.ReturnType.IsSubclassOf(typeof(ActionResultType)),
                        contentGenerator);
        }


        public string SerializeFormat<ControllerBaseType, ActionResultType>(Assembly assembly, string format)
        {
            return SerializeGeneric<ControllerBaseType, ActionResultType>(assembly, (t, m) => string.Format(format, this.GetTypeName(t), this.GetMethodName(m)));
        }
    }
}
