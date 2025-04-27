public class UpdateOrderRequestDto
{
    public int ProductType { get; set; }
    public required string PaymentMethod { get; set; }
    public DateTime OrderDate { get; set; }
}
