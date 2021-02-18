using fita.data.Enums;
using LiteDB;

namespace fita.data.Models
{
    public class Security
    {
        public ObjectId SecurityId { get; set; } = ObjectId.NewObjectId();

        public string Name { get; set; }

        public string Symbol { get; set; }

        public SecurityTypeEnum Type { get; set; }

        public string Suffix { get; set; }
    }
}