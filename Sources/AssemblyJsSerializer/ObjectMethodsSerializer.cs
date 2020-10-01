using AssemblyJsSerializer.Assemblies;
using AssemblyJsSerializer.Configuration;
using AssemblyJsSerializer.Error;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyJsSerializer
{
    public class ObjectMethodsSerializer : IErrorHandledObject
    {
        private Func<Type, MethodInfo, string> DefaultFieldContentGenerator => (t, m) => string.Format(this.Configuration.FieldFormat, this.GetTypeName(t), this.GetMethodName(m));

        private async Task InitAsync()
        {
            var configurationLoader = new ConfigurationLoader(this.ErrorHandler);
            this.ErrorHandler.AddNestedErrorHandler(configurationLoader.ErrorHandler);
            this.Configuration = await configurationLoader.LoadConfigurationAsync();
            this.FieldContentGenerator = this.DefaultFieldContentGenerator;

            var assemblyHelper = new AssemblyHelper(this.ErrorHandler);
            this.ErrorHandler.AddNestedErrorHandler(assemblyHelper.ErrorHandler);
            await assemblyHelper.GetSourceAssemblyAndLoadDependenciesAsync();
            this.SourceAssembly = assemblyHelper.GetAssembly(this.Configuration.SourceAssemblyName);
            if (this.SourceAssembly == null)
            {
                this.ErrorHandler.Add($"Aucune assembly n'a été chargée avec le nom {this.Configuration.SourceAssemblyName}");
            }
        }

        public IErrorHandler ErrorHandler
        {
            get;
        }

        public ObjectMethodsSerializer() : this(new ErrorHandler()) { }

        public ObjectMethodsSerializer(IErrorHandler errorHandler)
        {
            this.ErrorHandler = errorHandler;
            this.InitAsync().Wait();
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
        private StringBuilder StartNewGlobaleObjectBuilder() => new StringBuilder($"{this.Configuration.CautionHeader}\r\nconst {this.GetObjectName()} = {START_OBJECT}");
        private StringBuilder StartNewObjectBuilder() => new StringBuilder(START_OBJECT);
        private string EndObjectToString(StringBuilder sb, int indentlevel) => sb.Append($"{this.Indent(indentlevel)}{END_OBJECT}").ToString();
        private void AppendNewField(StringBuilder sb, string fieldName, string content, int indentlevel) => sb.Append($"{this.Indent(indentlevel)}{this.SerializeFieldName(fieldName)}:{content},\r\n");
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
            return $"/*{string.Join("\r\n", this.ErrorHandler.GetAllErrors().Select(e=>e.Message))}*/";
        }

        private string SerializeAssembly()
        {
            try
            {
                StringBuilder sbGlobalObject = this.StartNewGlobaleObjectBuilder();

                var AssemblyCrawler = new AssemblyCrawler(this.SourceAssembly, this.Configuration.Filters, this.ErrorHandler);
                this.ErrorHandler.AddNestedErrorHandler(AssemblyCrawler.ErrorHandler);
                foreach (FilteredType type in AssemblyCrawler.FilterAssemblyContent())
                {
                    StringBuilder sbType = this.StartNewObjectBuilder();
                    foreach (MethodInfo method in type.Methods)
                    {
                        this.AppendNewField(sbType, this.GetMethodName(method), this.FieldContentGenerator(type.Type, method), 2);
                    }

                    this.AppendNewField(sbGlobalObject, this.GetTypeName(type.Type), this.EndObjectToString(sbType, 1), 1);
                }

                return this.EndObjectToString(sbGlobalObject, 0);
            }
            catch (Exception e)
            {
                this.ErrorHandler.Add($"une erreur est survenue lors de la sérialisation", e);
                return string.Empty;
            }
        }
        #endregion

        private string GetContentWithError(string content)
        {
            if (this.ErrorHandler.HasErrors())
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
                this.ErrorHandler.Add($"une erreur est survenue lors de l'écriture du fichier", e);
            }

        }

        public string Serialize()
        {
            return this.SerializeAsync().Result;
        }

        public Task<string> SerializeAsync()
        {
            return this.SerializeToFileAsync(this.Configuration.TargetFile);
        }

        public async Task<string> SerializeToFileAsync(string filePath)
        {
            string serializedContent = string.Empty;
            if (!this.ErrorHandler.HasErrors())
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
