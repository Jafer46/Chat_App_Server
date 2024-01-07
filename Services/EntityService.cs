using ChatAppServer.Database;
using ChatAppServer.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ChatAppServer.Services
{
    public class EntityService<T> : IEntity<T> where T : class
    {
        private readonly DataContext _context;
        public EntityService(DataContext context)
        {
            _context = context;
        }

        public Task<T?> ReadFirst(Func<T, bool> predicate)
        {
            T? result = _context.Set<T>().FirstOrDefault(predicate);
            return Task.FromResult(result);
        }
        public Task<List<T>>? GetAll(Func<T, bool> predicate, string? childName = null)
        {
            if (childName == null)
            {
                var result = _context.Set<T>().Where(predicate).ToList();
                if (result == null)
                {
                    return null;
                }
                return Task.FromResult(result);
            }
            else
            {
                var result = _context.Set<T>().Include(childName).Where(predicate).ToList();
                if (result == null)
                {
                    return null;
                }
                return Task.FromResult(result);
            }

        }


        public async Task<bool> Create(T entity)
        {
            try
            {
                _context.Set<T>().Add(entity);
                await _context.SaveChangesAsync();

                _context.ChangeTracker.Clear();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Error: {0}.\n{1}", ex.Message, ex.InnerException));
            }

            return false;
        }
        public async Task<bool> Update(T entity)
        {
            try
            {
                _context.Set<T>().Update(entity);
                await _context.SaveChangesAsync();

                _context.ChangeTracker.Clear();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Error: {0}.\n{1}", ex.Message, ex.InnerException));
            }

            return false;
        }

        public async Task<bool> Delete(Func<T, bool> predicate)
        {
            try
            {
                IEnumerable<T> entity = _context.Set<T>().Where(predicate);
                if (entity != null)
                {
                    _context.Set<T>().RemoveRange(entity);
                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Error: {0}.\n{1}", ex.Message, ex.InnerException));
            }

            return false;
        }
    }
}