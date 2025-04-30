namespace InternalApi02.Payment.Models
{
    public class PaymentItem
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public decimal Value { get; set; }
    }
}
