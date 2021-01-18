using System.Linq;
using fita.core.Common;
using LiteDB;
using twentySix.Framework.Core.UI.Models;

namespace fita.core.Models
{
    public class Category: SynchronizableModel<Category>
    {
        public int CategoryId { get; set; }
        
        public string Name { get; set; }

        public CategoryGroupEnum Group { get; set; }
        
        [BsonRef]
        public Category Parent { get; set; }

        [BsonIgnore]
        public string FullName => string.Join(":", new[] {Name, Parent?.Name}.Where(x =>!string.IsNullOrEmpty(x)));
        
        public override bool PropertiesEqual(Category other)
        {
            if (other == null)
            {
                return false;
            }
        
            return other.Name.Equals(Name)
                   && other.Group == Group
                   && ReferenceEquals(other.Parent, Parent);
        }
        
        public override void SyncFrom(Category obj)
        {
            Name = (string) obj.Name?.Clone();
            Group = (CategoryGroupEnum)(int)obj.Group;
            Parent = obj.Parent;
        }
    }
}