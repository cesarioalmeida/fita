using fita.core.Common;
using LiteDB;

namespace fita.core.DTOs
{
    public class CategoryDTO
    {
        public string Name { get; set; }

        public CategoryGroupEnum Group { get; set; }
        
        public ObjectId ParentId { get; set; }
    }
}