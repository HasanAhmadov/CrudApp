namespace CrudApp.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }

        public User User { get; set; }
        public List<OrderItem> Items { get; set; } = new();
    }

}
