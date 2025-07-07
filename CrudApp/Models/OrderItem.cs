namespace CrudApp.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        public Product Product { get; set; }

        public decimal TotalPrice => Quantity * (Product?.Price ?? 0);
    }

}
