using System.Threading.Tasks;

namespace fita.services.Repositories
{
    public interface ISeedData
    {
        Task<Result> SeedData();
    }
}