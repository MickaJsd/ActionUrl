using System.Collections.Generic;

namespace AssemblyJsSerializer.Error
{
    public interface IErrorHandler
    {
        ICollection<string> Errors
        {
            get;
        }

        ICollection<IErrorHandler> InnerErrorHandlers
        {
            get;
        }

        IEnumerable<string> GetAllErrors();

        bool HasErrors();
    }
}
