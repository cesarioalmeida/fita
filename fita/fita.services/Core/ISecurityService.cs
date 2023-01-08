using System;
using System.Threading.Tasks;
using fita.data.Models;
using twentySix.Framework.Core.Common;

namespace fita.services.Core;

public interface ISecurityService
{
    Task<Result> Update(SecurityHistory securityHistory, DateTime? date = null);
}