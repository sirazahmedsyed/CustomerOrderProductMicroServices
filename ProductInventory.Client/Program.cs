namespace ProductInventory.Client;

public class Program
{
    public static async Task Main(string[] args)
    {
        await ExampleUsage();
    }

    private static async Task ExampleUsage()
    {
        try
        {
            using var client = new InventoryClient("https://localhost:7274");

            Console.WriteLine("Creating a product...");
            var product = await client.CreateProductAsync(
                name: "Test Product",
                description: "A test product",
                price: 99.99,
                stock: 100
            );
            Console.WriteLine($"Created product with ID: {product.ProductId}");

            Console.WriteLine("\nGetting the product...");
            var retrievedProduct = await client.GetProductAsync(product.ProductId);

            Console.WriteLine($"Retrieved product details \n" +
                  $"Name: {retrievedProduct.Name}\n" +
                  $"Description: {retrievedProduct.Description}\n" +
                  $"Price: ${retrievedProduct.Price}\n" +
                  $"Stock: {retrievedProduct.Stock}");

            Console.WriteLine("\nUpdating the product...");
            var updatedProduct = await client.UpdateProductAsync(
                productId: product.ProductId,
                name: "Updated Test Product",
                description: "Updated test product",
                price: 149.99,
                stockAdjustment: 100
            );

            Console.WriteLine($"Updated product:\n" +
                    $"Name: {updatedProduct.Name}\n" +
                    $"Description: {updatedProduct.Description}\n" +
                    $"Price: ${updatedProduct.Price}\n" +
                    $"Stock: {updatedProduct.Stock}");
            
            Console.WriteLine("\nDeleting the product...");
            var deleteResponse = await client.DeleteProductAsync(product.ProductId);
            Console.WriteLine($"Delete response: {deleteResponse.Message}");

            Console.WriteLine("\nGetting the product...");
            var productExists = await client.GetProductAsync(product.ProductId);
            
            if (productExists == null)
                Console.WriteLine($"Retrieved product: {retrievedProduct.Name}, Price: ${retrievedProduct.Price}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
