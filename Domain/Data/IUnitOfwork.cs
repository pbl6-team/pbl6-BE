namespace PBL6.Domain.Data
{
    public interface IUnitOfwork
    {
        IExampleRepository Examples { get; }

        Task CompleteAsync();
    }
}