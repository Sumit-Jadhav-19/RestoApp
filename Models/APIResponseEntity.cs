namespace RestoApp.Models
{
    public class APIResponseEntity
    {
        public int statusCode { get; set; }
        public string status { get; set; }
        public string message { get; set; }
        public dynamic data { get; set; }
    }
}
