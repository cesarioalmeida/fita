using fita.data.Enums;
using LiteDB;

namespace fita.data.Models
{
    public class Category
    {
        public ObjectId CategoryId { get; set; } = ObjectId.NewObjectId();

        public string Name { get; set; }

        public CategoryGroupEnum Group { get; set; }
    }
}