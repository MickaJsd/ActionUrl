namespace AssemblyJsSerializer.Error
{
    public interface IErrorHandledObject
    {
        IErrorHandler ErrorHandler
        {
            get;
        }
    }
}
