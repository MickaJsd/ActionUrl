using System;
using System.Collections.Generic;
using System.Linq;

namespace AssemblyJsSerializer.Error
{
    public abstract class NestedErrorHandler : IErrorHandler
    {

        protected NestedErrorHandler()
        {
            this.Errors = new List<string>();
            this.InnerErrorHandlers = new List<IErrorHandler>();
        }
        public ICollection<string> Errors
        {
            get;
        }

        protected void AddExceptionError(string message, Exception e)
        {
            this.Errors.Add($"{message} : {e.Message} {e.StackTrace} ");
        }

        public ICollection<IErrorHandler> InnerErrorHandlers
        {
            get;
        }

        public IEnumerable<string> GetAllErrors()
        {
            var errors = this.Errors.ToList();
            foreach (var errorHandler in InnerErrorHandlers)
            {
                errors.AddRange(errorHandler.GetAllErrors());
            }
            return errors;
        }

        public bool HasErrors()
        {
            var hasErrors = this.Errors.Any();
            foreach (var errorHandler in InnerErrorHandlers)
            {
                hasErrors |= errorHandler.HasErrors();
            }
            return hasErrors;
        }
    }
}
