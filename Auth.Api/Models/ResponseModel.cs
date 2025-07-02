namespace Auth.Api.Models
{
    public class ResponseModel<T>
    {
        public T? Data { get; set; }
        public string? Message { get; set; }
       
    }
}
