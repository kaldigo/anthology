namespace Anthology.Data.AudiobookGuild
{
    public class Price
    {
        public int price { get; set; }
        public int price_min { get; set; }
        public int price_max { get; set; }
        public object compare_at_price { get; set; }
        public int compare_at_price_min { get; set; }
        public int compare_at_price_max { get; set; }
        public bool compare_at_price_varies { get; set; }
    }
}