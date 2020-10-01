using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AssemblyJsSerializer.Error
{
    internal sealed class ErrorHandler : IErrorHandler
    {
        #region Initialisation
        internal ErrorHandler()
        {
            this.ErrorCollection = new List<IError>();
            this.InnerErrorHandlers = new List<IErrorHandler>();
        }
        #endregion

        #region IErrorHandler implementation
        public void Add(string message, Exception e)
        {
            if (e == null)
            {
                this.Add(message);
            }
            else
            {
                this.Add($"{message} : {e.Message} {e.StackTrace} ");
            }
        }

        public void Add(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }
            this.ErrorCollection.Add(new Error { Message = message });
        }

        public void AddNestedErrorHandler(IErrorHandler errorHandler)
        {
            if (errorHandler != this)
            {
                InnerErrorHandlers.Add(errorHandler);
            }
        }

        public IEnumerable<IError> GetAllErrors()
        {
            var errors = this.ErrorCollection.ToList();
            foreach (var errorHandler in this.InnerErrorHandlers)
            {
                errors.AddRange(errorHandler.GetAllErrors());
            }
            return errors;
        }

        public bool HasErrors()
        {
            if (this.ErrorCollection.Any())
            {
                return true;
            }
            foreach (var errorHandler in this.InnerErrorHandlers)
            {
                if (errorHandler.HasErrors())
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region Privates
        private ICollection<IErrorHandler> InnerErrorHandlers
        {
            get;
        }
        private ICollection<IError> ErrorCollection
        {
            get;
        }
        #endregion

        #region ICollection implementation
        public int Count => this.ErrorCollection.Count;

        public bool IsReadOnly => this.ErrorCollection.IsReadOnly;

        public void Add(IError item) => this.ErrorCollection.Add(item);

        public void Clear() => this.ErrorCollection.Clear();

        public bool Contains(IError item) => this.ErrorCollection.Contains(item);

        public void CopyTo(IError[] array, int arrayIndex) => this.ErrorCollection.CopyTo(array, arrayIndex);

        public bool Remove(IError item) => this.ErrorCollection.Remove(item);

        public IEnumerator<IError> GetEnumerator() => this.ErrorCollection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.ErrorCollection.GetEnumerator();
        #endregion
    }
}
