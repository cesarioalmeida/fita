using System;
using System.Threading.Tasks;
using fita.data.Models;

namespace fita.services.Core
{
    public interface ISecurityService
    {
        Task<Result> Update(SecurityHistory securityHistory, DateTime? date = null);
    }
}