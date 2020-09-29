using AssemblyJsSerializer.Configuration;
using AssemblyJsSerializer.Error;
using AssemblyJsSerializer.Assemblies;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyJsSerializer
{
    public class ObjectMethodsSerializer : NestedErrorHandler
    {
        private Func<Type, MethodInfo, string> DefaultFieldContentGenerator => (t, m) => string.Format(this.Configuration.FieldFormat, this.GetTypeName(t), this.GetMethodName(m));

        private async Task InitAsync()
        {
            var configurationLoader = new ConfigurationLoader();
            this.InnerErrorHandlers.Add(configurationLoader);
            this.Configuration = await configurationLoader.LoadConfigurationAsync();
            this.FieldContentGenerator = DefaultFieldContentGenerator;

            var assemblyHelper = new AssemblyHelper();
            this.InnerErrorHandlers.Add(assemblyHelper);
            await assemblyHelper.GetSourceAssemblyAndLoadDependenciesAsync();
            this.SourceAssembly = assemblyHelper.GetAssembly(this.Configuration.SourceAssemblyName);
            if (this.SourceAssembly == null)
            {
                this.Errors.Add($"Aucune assembly n'a été chargée avec le nom {this.Configuration.SourceAssemblyName}");
            }
        }

        public ObjectMethodsSerializer()
        {
                InitAsync().Wait();
        }

        internal ConfigurationSettings Configuration
        {
            get; private set;
        }

        public string GetObjectName()
        {
            if (string.IsNullOrWhiteSpace(this.Configuration.TargetObjectName))
            {
                return this.SourceAssembly.GetName().Name;
            }
            return this.Configuration.TargetObjectName;
        }

        #region Serializer string builder
        private const string START_OBJECT = "{\r\n";
        private const char END_OBJECT = '}';
        private StringBuilder StartNewGlobaleObjectBuilder() => new StringBuilder($"{Configuration.CautionHeader}\r\nconst {this.GetObjectName()} = {START_OBJECT}");
        private StringBuilder StartNewObjectBuilder() => new StringBuilder(START_OBJECT);
        private string EndObjectToString(StringBuilder sb, int indentlevel) => sb.Append($"{Indent(indentlevel)}{END_OBJECT}").ToString();
        private void AppendNewField(StringBuilder sb, string fieldName, string content, int indentlevel) => sb.Append($"{Indent(indentlevel)}{SerializeFieldName(fieldName)}:{content},\r\n");
        private string SerializeFieldName(string name) => $"'{name}'";
        private string Indent(int number)
        {
            return string.Empty.PadLeft(number * 4, ' ');
        }
        #endregion

        #region Source Assembly crawling & serializing
        public virtual string GetTypeName(Type t) => t.Name;
        public virtual string GetMethodName(MethodInfo m) => m.Name;
        public Func<Type, MethodInfo, string> FieldContentGenerator
        {
            get; set;
        }
        private Assembly SourceAssembly
        {
            get; set;
        }

        private string GetCommentedError()
        {
            return $"/*{string.Join("\r\n", this.GetAllErrors())}*/";
        }

        private string SerializeAssembly()
        {
            try
            {
                StringBuilder sbGlobalObject = StartNewGlobaleObjectBuilder();

                var AssemblyCrawler = new AssemblyCrawler(this.SourceAssembly, this.Configuration.Filters);
                this.InnerErrorHandlers.Add(AssemblyCrawler);
                foreach (FilteredType type in AssemblyCrawler.FilterAssemblyContent())
                {
                    StringBuilder sbType = StartNewObjectBuilder();
                    foreach (MethodInfo method in type.Methods)
                    {
                        AppendNewField(sbType, this.GetMethodName(method), FieldContentGenerator(type.Type, method), 2);
                    }

                    AppendNewField(sbGlobalObject, this.GetTypeName(type.Type), EndObjectToString(sbType, 1), 1);
                }

                return this.EndObjectToString(sbGlobalObject, 0);
            }
            catch (Exception e)
            {
                this.AddExceptionError($"une erreur est survenue lors de la sérialisation", e);
                return string.Empty;
            }
        }
        #endregion

        private string GetContentWithError(string content)
        {
            if (this.GetAllErrors().Any())
            {
                return this.GetCommentedError() + content;
            }
            return content;
        }

        private async Task WriteToFile(string filePath, string content)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                await Task.Run(() =>
                {
                    fileInfo.Directory.Create();
                    File.WriteAllText(filePath, content);
                });
            }
            catch (Exception e)
            {
                this.AddExceptionError($"une erreur est survenue lors de l'écriture du fichier", e);
            }

        }

        public string Serialize()
        {
            return SerializeAsync().Result;
        }

        public Task<string> SerializeAsync()
        {
            return SerializeToFileAsync(this.Configuration.TargetFile);
        }

        public async Task<string> SerializeToFileAsync(string filePath)
        {
            string serializedContent = string.Empty;
            if (!this.HasErrors())
            {
                serializedContent = this.SerializeAssembly();
            }

            if (!string.IsNullOrWhiteSpace(filePath))
            {
                await this.WriteToFile(filePath, this.GetContentWithError(serializedContent));
            }

            return this.GetContentWithError(serializedContent);
        }
    }
}
