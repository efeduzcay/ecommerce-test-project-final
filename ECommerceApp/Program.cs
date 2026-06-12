using ECommerceApp.Core;

Console.WriteLine("=== E-Commerce Demo ===\n");

// 1. Create products (note the deliberately zero stock for the Mouse)
var laptop   = new Product(1, "Laptop",    15000m, 10);
var mouse    = new Product(2, "Mouse",       250m,  0);   // stock = 0 → should block ordering
var keyboard = new Product(3, "Keyboard",    500m, 30);

Console.WriteLine("Available Products:");
Console.WriteLine(laptop);
Console.WriteLine(mouse);
Console.WriteLine(keyboard);

// 2. Build cart
var cart = new Cart();
cart.AddItem(laptop, 1);
cart.AddItem(keyboard, 2);
cart.AddItem(laptop, 1);   // BUG B1: will add duplicate instead of qty=2

Console.WriteLine($"\n--- Cart (ItemCount = {cart.ItemCount}) ---");
Console.WriteLine(cart);

// 3. Discount code resolution
var discountService = new DiscountService();
var validCode   = "SAVE10";
var invalidCode = "FAKE123";

Console.WriteLine($"\nDiscount for '{validCode}'   : {discountService.GetDiscountPercent(validCode)}%");
Console.WriteLine($"Discount for '{invalidCode}' : {discountService.GetDiscountPercent(invalidCode)}%  (BUG B8: should be 0%)");

// 4. Show totals (with and without discount)
Console.WriteLine($"\nTotal (no discount)   : {cart.GetTotal():C2}");
Console.WriteLine($"Total (10% discount)  : {cart.GetTotal(10):C2}   (BUG B2: same as above)");

// 5. Place order (minimum order amount = 100₺)
var orderService = new OrderService(minimumOrderAmount: 100m);
Console.WriteLine("\n--- Placing Order ---");
try
{
    var orderMsg = orderService.PlaceOrder(cart);
    Console.WriteLine(orderMsg);
}
catch (OutOfStockException ex)         { Console.WriteLine($"OUT OF STOCK: {ex.Message}"); }
catch (MinimumOrderNotMetException ex) { Console.WriteLine($"MIN ORDER NOT MET: {ex.Message}"); }

// 6. Process payment
if (orderService.Status == OrderStatus.Placed)
{
    Console.WriteLine("\n--- Processing Payment ---");
    var paymentMsg = orderService.ProcessPayment(cart.GetTotal());
    Console.WriteLine(paymentMsg);
}

Console.WriteLine($"\nFinal Status: {orderService}");
