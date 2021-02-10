﻿using LiteDB;

namespace fita.data.Models
{
    public class Currency
    {
        public ObjectId CurrencyId { get; set; }

        public string Name { get; set; }

        public string Symbol { get; set; }

        public string Prefix { get; set; }

        public string Suffix { get; set; }
    }
}