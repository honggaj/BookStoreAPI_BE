namespace BookStoreAPI.Models.Response
{
    public class ResultCustomModel<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }
}
