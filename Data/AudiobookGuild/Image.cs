﻿namespace Anthology.Data.AudiobookGuild
{
    public class Image
    {
        public object id { get; set; }
        public DateTime created_at { get; set; }
        public int position { get; set; }
        public DateTime updated_at { get; set; }
        public object product_id { get; set; }
        public List<object> variant_ids { get; set; }
        public string src { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }
}