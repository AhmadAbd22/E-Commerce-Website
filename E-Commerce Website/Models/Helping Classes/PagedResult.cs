namespace ECommerceWebsite.Models.Helping_Classes
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
        public int StartItem => TotalCount > 0 ? ((CurrentPage - 1) * PageSize) + 1 : 0;
        public int EndItem => TotalCount > 0 ? StartItem + Items.Count - 1 : 0;
    }
}
