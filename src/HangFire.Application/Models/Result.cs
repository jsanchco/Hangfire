namespace HangFire.Application.Models
{
    public class Result
    {
        public DateTime Start {  get; set; } = DateTime.Now;
        public DateTime? End { get; set; }
        public bool Success { get; set; }
        public List<string> Summary { get; set; }
    }
}
