using Grpc.Core;
using Grpc.Net.Client;
using ProductInventory.Server;
using Grpc.Reflection.V1Alpha;

namespace ProductInventory.Client;

public class InventoryClient : IDisposable
{
    private readonly InventoryService.InventoryServiceClient _client;
    private readonly GrpcChannel _channel;

    public InventoryClient(string serverUrl)
    {
        _channel = GrpcChannel.ForAddress(serverUrl);
        _client = new InventoryService.InventoryServiceClient(_channel);
    }
    public async Task<ProductResponse> CreateProductAsync(string name, string description, double price, int stock)
    {
        var request = new CreateProductRequest
        {
            Name = name,
            Description = description,
            Price = price,
            Stock = stock
        };

        return await _client.CreateProductAsync(request);
    }

    public async Task<ProductResponse> GetProductAsync(string productId)
    {
        var request = new GetProductRequest { ProductId = productId };
        return await _client.GetProductAsync(request);
    }

    public async Task<ProductResponse> UpdateProductAsync(string productId,
        string name = null, string description = null, double price = 0, int stockAdjustment = 0)
    {
        var request = new UpdateProductRequest
        {
            ProductId = productId,
            Name = name,
            Description = description,
            Price = price,
            StockAdjustment = stockAdjustment
        };

        return await _client.UpdateProductAsync(request);
    }

    public async Task<DeleteProductResponse> DeleteProductAsync(string productId)
    {
        var request = new DeleteProductRequest { ProductId = productId };
        return await _client.DeleteProductAsync(request);
    }

    public async Task WatchProductStockAsync(string productId,
        Action<StockUpdate> onStockUpdate, CancellationToken cancellationToken = default)
    {
        var request = new WatchStockRequest { ProductId = productId };
        using var call = _client.WatchProductStock(request);

        try
        {
            await foreach (var update in call.ResponseStream.ReadAllAsync(cancellationToken))
            {
                onStockUpdate(update);
            }
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
        {
            // Normal cancellation, ignore
        }
    }

    public async Task<BatchUpdateResponse> BatchUpdateProductsAsync(
        IEnumerable<(string ProductId, string? Name, string? Description, double? Price, int? StockAdjustment)> updates)
    {
        using var call = _client.BatchUpdateProducts();

        foreach (var update in updates)
        {
            await call.RequestStream.WriteAsync(new UpdateProductRequest
            {
                ProductId = update.ProductId,
                Name = update.Name,
                Description = update.Description,
                Price = update.Price ?? 0,
                StockAdjustment = update.StockAdjustment ?? 0
            });
        }

        await call.RequestStream.CompleteAsync();
        return await call;
    }

    public async Task SyncInventoryAsync(
        IAsyncEnumerable<(string ProductId, int Stock)> updates,
        Action<InventorySync> onInventoryUpdate,
        CancellationToken cancellationToken = default)
    {
        using var call = _client.SyncInventory();

        // Start receiving updates
        var readTask = Task.Run(async () =>
        {
            await foreach (var update in call.ResponseStream.ReadAllAsync(cancellationToken))
            {
                onInventoryUpdate(update);
            }
        });

        // Send updates
        await foreach (var update in updates.WithCancellation(cancellationToken))
        {
            await call.RequestStream.WriteAsync(new InventorySync
            {
                ProductId = update.ProductId,
                Stock = update.Stock,
                Timestamp = DateTime.UtcNow.ToString("o")
            });
        }

        await call.RequestStream.CompleteAsync();
        await readTask;
    }



    public void Dispose()
    {
        _channel.Dispose();
    }
}
