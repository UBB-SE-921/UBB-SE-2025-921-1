namespace SharedClassLibrary.DataTransferObjects
{
    /// <summary>
    /// Data transfer object for dummy product requests.
    /// </summary>
    public class DummyProductRequest
    {
        public required string Name { get; set; }
        public float Price { get; set; }
        public int SellerID { get; set; }
        public required string ProductType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}