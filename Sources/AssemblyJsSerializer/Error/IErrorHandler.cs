using System;
using System.Collections.Generic;

namespace AssemblyJsSerializer.Error
{
    public interface IErrorHandler : ICollection<IError>
    {
        IEnumerable<IError> GetAllErrors();
        bool HasErrors();
        void Add(string message);
        void Add(string message, Exception e);
        void AddNestedErrorHandler(IErrorHandler errorHandler);

    }
}
