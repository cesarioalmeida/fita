using fita.core.Common;
using LiteDB;
using twentySix.Framework.Core.DTOs;

namespace fita.core.DTOs
{
    public class CategoryDTO : BaseDTO
    {
        public string Name { get; set; }

        public CategoryGroupEnum Group { get; set; }
        
        public ObjectId ParentId { get; set; }
    }
}