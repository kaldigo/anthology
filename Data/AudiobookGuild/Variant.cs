namespace Anthology.Data.AudiobookGuild
{
    public class Variant
    {
        public object id { get; set; }
        public string title { get; set; }
        public string option1 { get; set; }
        public object option2 { get; set; }
        public object option3 { get; set; }
        public string sku { get; set; }
        public bool requires_shipping { get; set; }
        public bool taxable { get; set; }
        public FeaturedImage featured_image { get; set; }
        public bool available { get; set; }
        public string price { get; set; }
        public int grams { get; set; }
        public object compare_at_price { get; set; }
        public int position { get; set; }
        public object product_id { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }
}