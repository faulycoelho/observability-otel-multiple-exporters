namespace InternalApi02.Payment.Models
{
    public class PaymentItem
    {
        public int Id { get; set; }
        public string TransactionCode { get; set; } = null!;
        public decimal Value { get; set; }
    }
}
