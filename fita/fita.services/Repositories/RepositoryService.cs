using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LiteDB;
using twentySix.Framework.Core.Services.Interfaces;

namespace fita.services.Repositories
{
    public class RepositoryService<T> : IRepositoryService<T> where T : class 
    {
        public RepositoryService(IDBHelperService dbHelperService, ILoggingService loggingService)
        {
            DBHelperService = dbHelperService;
            LoggingService = loggingService;
        }
        
        public IDBHelperService DBHelperService { get; }

        public ILoggingService LoggingService { get; }

        public ILiteCollection<T> Collection => DBHelperService.DB.GetCollection<T>();

        public virtual void IndexData()
        {
        }

        public virtual Task<Result> SaveAsync(T model)
        {
            return Task.Run(
                () =>
                {
                    if (model == null)
                    {
                        return Result.Fail;
                    }

                    try
                    {
                        return Collection.Upsert(model) ? Result.Success : Result.Fail;
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Warn($"{nameof(SaveAsync)}: {ex}");
                        return Result.Fail;
                    }
                });
        }

        public virtual Task<Result> DeleteAsync(ObjectId id)
        {
            return Task.Run(
                () =>
                {
                    try
                    {
                        return Collection.Delete(id) ? Result.Success : Result.Fail;
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Warn($"{nameof(DeleteAsync)}: {ex}");
                        return Result.Fail;
                    }
                });
        }

        public virtual Task<T> DetailsAsync(ObjectId id)
        {
            return Task.Run(
                () =>
                {
                    try
                    {
                        return Collection.FindById(id);
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Warn($"{nameof(DetailsAsync)}: {ex}");
                        return null;
                    }
                });
        }

        public virtual Task<T> DetailsEnrichedAsync(ObjectId id)
        {
            return DetailsAsync(id);
        }

        public virtual Task<IEnumerable<T>> AllAsync()
        {
            return Task.Run(
                () =>
                {
                    try
                    {
                        return Collection.FindAll();
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Warn($"{nameof(AllAsync)}: {ex}");
                        return null;
                    }
                });
        }

        public virtual Task<IEnumerable<T>> AllEnrichedAsync()
        {
            return AllAsync();
        }
    }
}