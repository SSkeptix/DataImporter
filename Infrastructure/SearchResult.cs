namespace Infrastructure
{
    public class SearchResult<T>
        where T: class
    {
        public T[] Items { get; set; }
        public int Count { get; set; }

        public SearchResult() { }

        public SearchResult(T[] items, int count)
        {
            this.Items = items;
            this.Count = count;
        }
    }
}
