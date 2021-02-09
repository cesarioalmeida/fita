using System.Collections.Generic;
using System.Threading.Tasks;
using LiteDB;

namespace fita.services.Repositories
{
    public interface IRepositoryService<T>
    {
        void IndexData();

        Task<Result> SaveAsync(T model);

        Task<Result> DeleteAsync(ObjectId id);

        Task<T> DetailsAsync(ObjectId id);

        Task<T> DetailsEnrichedAsync(ObjectId id);
        
        Task<IEnumerable<T>> AllAsync();

        Task<IEnumerable<T>> AllEnrichedAsync();
    }
}