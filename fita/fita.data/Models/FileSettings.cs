using LiteDB;

namespace fita.data.Models
{
    public class FileSettings
    {
        public ObjectId FileSettingsId { get; set; }

        [BsonRef("currency")]
        public Currency BaseCurrency { get; set; }
    }
}