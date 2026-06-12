namespace ECommerceApp.Core
{
    /// <summary>
    /// Represents a product in the e-commerce system.
    /// </summary>
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }

        public Product(int id, string name, decimal price, int stock = 100)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Product name cannot be empty.", nameof(name));

            Id = id;
            Name = name;
            Price = price;
            Stock = stock;
        }

        public override string ToString() => $"[{Id}] {Name} - {Price:C2} (Stock: {Stock})";
    }
}
