namespace AssemblyJsSerializer.Configuration
{
    internal class ConfigurationSettings
    {

        internal ConfigurationSettings()
        {
        }

        public string FieldFormat
        {
            get; set;
        }

        public string TargetFile
        {
            get; set;
        }

        public string CautionHeader
        {
            get; set;
        }

        public string SourceAssemblyName
        {
            get; set;
        }
        public string TargetObjectName
        {
            get; set;
        }

        public Filters Filters
        {
            get; set;
        }
    }
}
