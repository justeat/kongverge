namespace Kongverge.DTOs
{
    public class PagedResponse<T>
    {
        public int Total { get; set; }
        public string Next { get; set; }
        public T[] Data { get; set; }
    }
}
