using System.Linq;
using fita.core.Common;
using fita.core.DTOs;
using LiteDB;
using twentySix.Framework.Core.UI.Models;

namespace fita.core.Models
{
    public class Category //: SynchronizableModelWithDTO<Category, CategoryDTO>
    {
        public Category()
        {
            //Id = ObjectId.NewObjectId();
        }

        public string Name { get; set; }

        public CategoryGroupEnum Group { get; set; }
        
        public Category Parent { get; set; }

        public string FullName => string.Join(":", new[] {Name, Parent?.Name}.Where(x =>!string.IsNullOrEmpty(x)));
        
        // public override CategoryDTO GetDTO()
        // {
        //     return new()
        //     {
        //         Id = Id,
        //         IsDeleted = IsDeleted,
        //         LastUpdated = LastUpdated,
        //         Name = Name,
        //         Group = Group,
        //         ParentId = Parent?.Id
        //     };
        // }
        //
        // public override bool PropertiesEqual(Category other)
        // {
        //     if (other == null)
        //     {
        //         return false;
        //     }
        //
        //     return other.Name.Equals(Name)
        //            && other.Group == Group
        //            && other.Parent?.Id == Parent?.Id;
        // }
        //
        // public override void SyncFrom(Category obj)
        // {
        //     IsDeleted = obj.IsDeleted;
        //     LastUpdated = obj.LastUpdated;
        //     Name = (string) obj.Name?.Clone();
        //     Group = (CategoryGroupEnum)(int)obj.Group;
        //     Parent = obj.Parent;
        // }
    }
}