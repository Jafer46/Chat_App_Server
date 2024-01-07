using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ChatAppServer.Interfaces
{
    public interface IEntity<T>
    {
        Task<bool> Create(T entity);
        Task<bool> Update(T entity);
        Task<bool> Delete(Func<T, bool> predicate);
        Task<T?> ReadFirst(Func<T, bool> predicate);
        Task<List<T>>? GetAll(Func<T, bool> predicate, string? childName = null);
    }
}