using AssemblyJsSerializer.Configuration;
using AssemblyJsSerializer.Error;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AssemblyJsSerializer
{
    internal class AssemblyHelper : IErrorHandledObject
    {
        public IErrorHandler ErrorHandler
        {
            get;
        }

        public AssemblyHelper(IErrorHandler errorHandler)
        {
            this.ErrorHandler = errorHandler;
        }

        public async Task GetSourceAssemblyAndLoadDependenciesAsync()
        {
            try
            {
                await this.LoadAssembliesFromDirectoryToCurrentDomain(Directory.GetCurrentDirectory());
            }
            catch (Exception e)
            {
                this.ErrorHandler.Add($"Une erreur est survenue lors du chargement des assemblies", e);
            }
        }

        public Assembly GetAssembly(string assemblyName)
        {
            if (string.IsNullOrWhiteSpace(assemblyName))
            {
                this.ErrorHandler.Add($"Le paramètre de configuration {nameof(ConfigurationSettings.SourceAssemblyName)} n'a pas été renseigné");
                return null;
            }
            return AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(a => a.GetName().Name == assemblyName);
        }

        private async Task<IEnumerable<Assembly>> LoadAssembliesFromDirectoryToCurrentDomain(string directoryPath)
        {
            List<Task<Assembly>> tasks = new List<Task<Assembly>>();

            foreach (string assemblyName in Directory.GetFiles(directoryPath, "*.dll"))
            {
                tasks.Add(Task.Run(() =>
                {
                    return AppDomain.CurrentDomain.Load(Assembly.LoadFrom(assemblyName).GetName());
                }
                ));
            }
            await Task.WhenAll(tasks);

            return tasks
                    .Where(t => !t.IsFaulted)
                    .Select(t => t.Result);
        }
    }

}
