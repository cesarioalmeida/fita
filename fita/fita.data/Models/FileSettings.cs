﻿using LiteDB;

namespace fita.data.Models;

public class FileSettings
{
    public ObjectId FileSettingsId { get; set; } = ObjectId.NewObjectId();

    public string Name { get; set; }

    [BsonRef("currency")]
    public Currency BaseCurrency { get; set; }
}