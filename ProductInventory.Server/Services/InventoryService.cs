using Grpc.Core;
using ProductInventory.Shared.Entities;
using ProductInventory.Shared.Interfaces;

namespace ProductInventory.Server.Services;

public class InventoryService : Server.InventoryService.InventoryServiceBase
{
    private readonly IProductRepository _repository;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(IProductRepository repository, ILogger<InventoryService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public override async Task<ProductResponse> CreateProduct(CreateProductRequest request, ServerCallContext context)
    {
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock
        };

        var created = await _repository.CreateProductAsync(product);
        return MapToProductResponse(created);
    }

    public override async Task<ProductResponse> GetProduct(GetProductRequest request, ServerCallContext context)
    {
        var product = await _repository.GetProductAsync(request.ProductId);
        if (product == null)
            throw new RpcException(new Status(StatusCode.NotFound, "Product not found"));

        return MapToProductResponse(product);
    }

    public override async Task<ProductResponse> UpdateProduct(UpdateProductRequest request, ServerCallContext context)
    {
        var existing = await _repository.GetProductAsync(request.ProductId);
        if (existing == null)
            throw new RpcException(new Status(StatusCode.NotFound, "Product not found"));

        if (request.Name != null)
            existing.Name = request.Name;
        if (request.Description != null)
            existing.Description = request.Description;
        if (request.Price != null)
            existing.Price = request.Price;
        if (request.StockAdjustment != null)
            existing.Stock += request.StockAdjustment;

        var updated = await _repository.UpdateProductAsync(existing);
        return MapToProductResponse(updated);
    }

    public override async Task<DeleteProductResponse> DeleteProduct(DeleteProductRequest request, ServerCallContext context)
    {
        var success = await _repository.DeleteProductAsync(request.ProductId);
        return new DeleteProductResponse
        {
            Success = success,
            Message = success ? "Product deleted successfully" : "Product not found"
        };
    }

    public override async Task<BatchUpdateResponse> BatchUpdateProducts(IAsyncStreamReader<UpdateProductRequest> requestStream, ServerCallContext context)
    {
        var updatedIds = new List<string>();
        
        await foreach (var request in requestStream.ReadAllAsync())
        {
            try
            {
                var product = await _repository.GetProductAsync(request.ProductId);
                if (product != null)
                {
                    if (request.Name != null)
                        product.Name = request.Name;
                    if (request.Description != null)
                        product.Description = request.Description;
                    if (request.Price != null)
                        product.Price = request.Price;
                    if (request.StockAdjustment != null)
                        product.Stock += request.StockAdjustment;

                    await _repository.UpdateProductAsync(product);
                    updatedIds.Add(product.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {ProductId}", request.ProductId);
            }
        }

        return new BatchUpdateResponse
        {
            ProductsUpdated = updatedIds.Count,
            UpdatedProductIds = { updatedIds }
        };
    }

    public override async Task WatchProductStock(WatchStockRequest request,
        IServerStreamWriter<StockUpdate> responseStream, ServerCallContext context)
    {
        while (!context.CancellationToken.IsCancellationRequested)
        {
            var stock = await _repository.GetStockLevelAsync(request.ProductId);
            await responseStream.WriteAsync(new StockUpdate
            {
                ProductId = request.ProductId,
                CurrentStock = stock,
                Timestamp = DateTime.UtcNow.ToString("o")
            });

            await Task.Delay(1000, context.CancellationToken); 
        }
    }

    public override async Task SyncInventory(IAsyncStreamReader<InventorySync> requestStream,
        IServerStreamWriter<InventorySync> responseStream, ServerCallContext context)
    {
        var processingTask = ProcessClientUpdatesAsync(requestStream, context.CancellationToken);
        var broadcastTask = BroadcastInventoryUpdatesAsync(responseStream, context.CancellationToken);

        await Task.WhenAll(processingTask, broadcastTask);
    }

    private async Task ProcessClientUpdatesAsync(IAsyncStreamReader<InventorySync> requestStream,
        CancellationToken cancellationToken)
    {
        await foreach (var update in requestStream.ReadAllAsync(cancellationToken))
        {
            var product = await _repository.GetProductAsync(update.ProductId);
            if (product != null)
            {
                product.Stock = update.Stock;
                await _repository.UpdateProductAsync(product);
            }
        }
    }

    private async Task BroadcastInventoryUpdatesAsync(IServerStreamWriter<InventorySync> responseStream,
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var products = await _repository.GetAllProductsAsync();
            foreach (var product in products)
            {
                await responseStream.WriteAsync(new InventorySync
                {
                    ProductId = product.Id,
                    Stock = product.Stock,
                    Timestamp = DateTime.UtcNow.ToString("o")
                });
            }
            await Task.Delay(1000, cancellationToken);
        }
    }

    private static ProductResponse MapToProductResponse(Product product)
    {
        return new ProductResponse
        {
            ProductId = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = (double)product.Price,
            Stock = product.Stock,
            LastUpdated = product.LastUpdated.ToString("o")
        };
    }
}