using System.Linq.Expressions;

namespace ClinicManagementSystem.Repositories
{
    public interface IRepository<T> where T : class
    {
        // Read
        Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T, bool>>? filter = null,
            bool tracked = true,
            params Expression<Func<T, object>>[] includes);

        Task<T?> GetOneAsync(
            Expression<Func<T, bool>> filter,
            bool tracked = true,
            params Expression<Func<T, object>>[] includes);

        // Write
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task SaveAsync();
    }
}
