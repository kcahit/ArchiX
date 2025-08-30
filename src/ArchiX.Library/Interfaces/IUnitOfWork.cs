namespace ArchiX.Library.Interfaces
{ 
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync();
    }
}
