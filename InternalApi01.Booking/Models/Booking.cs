namespace InternalApi01.Booking.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string TraceId { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
