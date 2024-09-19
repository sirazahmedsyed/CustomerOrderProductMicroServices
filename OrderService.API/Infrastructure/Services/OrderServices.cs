using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OrderService.API.Infrastructure.DTOs;
using OrderService.API.Infrastructure.Entities;
using OrderService.API.Infrastructure.Services;
using OrderService.API.Infrastructure.UnitOfWork;
namespace OrderService.API.Infrastructure.Services
{
public class OrderServices : IOrderService
{
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderServices(IUnitOfWork unitOfWork, IMapper mapper, IProductService productService, ICustomerService customerService, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _productService = productService;
            _customerService = customerService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<OrderDTO>> GetAllOrdersAsync()
        {
            var orders = await _unitOfWork.Repository<Order>().GetAllAsync(
                include: q => q.Include(o => o.OrderItems));
            return _mapper.Map<IEnumerable<OrderDTO>>(orders);
        }

        public async Task<OrderDTO> GetOrderByIdAsync(Guid id)
        {
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(id,
                include: q => q.Include(o => o.OrderItems));
            return order == null ? null : _mapper.Map<OrderDTO>(order);
        }


        //    public async Task<OrderDTO> AddOrderAsync(OrderDTO orderDto)
        //    {
        //        var bearerToken = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        //        // Validate customer
        //        if (!await _customerService.CustomerExistsAsync(orderDto.CustomerId, bearerToken))
        //        {
        //            throw new CustomerNotFoundException(orderDto.CustomerId);
        //        }

        //        //// Check if there is an existing order for the customer
        //        //var existingOrder = await _unitOfWork.Repository<Order>()
        //        //    .GetAsync(o => o.CustomerId == orderDto.CustomerId && o.OrderItems.Any(oi => orderDto.OrderItems.Select(i => i.ProductId).Contains(oi.ProductId)),
        //        //              include: o => o.Include(o => o.OrderItems));

        //        var existingOrders = await _unitOfWork.Repository<Order>()
        //.GetAllAsync(
        //    o => o.CustomerId == orderDto.CustomerId && o.OrderItems.Any(oi => orderDto.OrderItems.Select(i => i.ProductId).Contains(oi.ProductId)),
        //    include: o => o.Include(o => o.OrderItems));

        //        if (existingOrders != null)
        //        {
        //            // Update quantities for existing products
        //            foreach (var itemDto in orderDto.OrderItems)
        //            {
        //                var existingItem = existingOrders.OrderItems.FirstOrDefault(oi => oi.ProductId == itemDto.ProductId);

        //                if (existingItem != null)
        //                {
        //                    existingItem.Quantity += itemDto.Quantity; // Update the quantity
        //                }
        //                else
        //                {
        //                    // Add new product to the existing order
        //                    var (price, taxPercentage) = await _productService.GetProductDetailsAsync(itemDto.ProductId, bearerToken);
        //                    existingOrder.OrderItems.Add(new OrderItem
        //                    {
        //                        ProductId = itemDto.ProductId,
        //                        Quantity = itemDto.Quantity,
        //                        UnitPrice = price
        //                    });
        //                }
        //            }

        //            await CalculateOrderTotals(existingOrder, bearerToken);
        //            _unitOfWork.Repository<Order>().Update(existingOrder);
        //        }
        //        else
        //        {
        //            // Group and sum quantities for duplicate product IDs
        //            var groupedOrderItems = orderDto.OrderItems
        //                .GroupBy(item => item.ProductId)
        //                .Select(g => new OrderItemDTO
        //                {
        //                    ProductId = g.Key,
        //                    Quantity = g.Sum(item => item.Quantity)
        //                })
        //                .ToList();

        //            // Validate products and create new order
        //            var orderItems = new List<OrderItem>();
        //            foreach (var item in groupedOrderItems)
        //            {
        //                if (!await _productService.ProductExistsAsync(item.ProductId, bearerToken))
        //                {
        //                    throw new ProductNotFoundException(item.ProductId);
        //                }

        //                var (price, taxPercentage) = await _productService.GetProductDetailsAsync(item.ProductId, bearerToken);
        //                orderItems.Add(new OrderItem
        //                {
        //                    ProductId = item.ProductId,
        //                    Quantity = item.Quantity,
        //                    UnitPrice = price
        //                });
        //            }

        //            var order = new Order
        //            {
        //                OrderId = Guid.NewGuid(),
        //                CustomerId = orderDto.CustomerId,
        //                OrderDate = DateTime.UtcNow,
        //                DiscountPercentage = orderDto.DiscountPercentage,
        //                OrderItems = orderItems
        //            };

        //            await CalculateOrderTotals(order, bearerToken);
        //            await _unitOfWork.Repository<Order>().AddAsync(order);
        //            existingOrder = order; // Set this to return the newly created order
        //        }

        //        await _unitOfWork.CompleteAsync();
        //        return _mapper.Map<OrderDTO>(existingOrder);
        //    }

        //public async Task<OrderDTO> AddOrderAsync(OrderDTO orderDto)
        //{
        //    var bearerToken = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        //    // Validate customer
        //    if (!await _customerService.CustomerExistsAsync(orderDto.CustomerId, bearerToken))
        //    {
        //        throw new CustomerNotFoundException(orderDto.CustomerId);
        //    }

        //    // Group and sum quantities for duplicate product IDs
        //    var groupedOrderItems = orderDto.OrderItems
        //        .GroupBy(item => item.ProductId)
        //        .Select(g => new OrderItemDTO
        //        {
        //            ProductId = g.Key,
        //            Quantity = g.Sum(item => item.Quantity)
        //        })
        //        .ToList();

        //    // Validate products and create order items
        //    var orderItems = new List<OrderItem>();
        //    foreach (var item in groupedOrderItems)
        //    {
        //        if (!await _productService.ProductExistsAsync(item.ProductId, bearerToken))
        //        {
        //            throw new ProductNotFoundException(item.ProductId);
        //        }

        //        var (price, taxPercentage) = await _productService.GetProductDetailsAsync(item.ProductId, bearerToken);
        //        orderItems.Add(new OrderItem
        //        {
        //            ProductId = item.ProductId,
        //            Quantity = item.Quantity,
        //            UnitPrice = price
        //        });
        //    }

        //    var order = new Order
        //    {
        //        OrderId = Guid.NewGuid(),
        //        CustomerId = orderDto.CustomerId,
        //        OrderDate = DateTime.UtcNow,
        //        DiscountPercentage = orderDto.DiscountPercentage,
        //        OrderItems = orderItems
        //    };

        //    await CalculateOrderTotals(order, bearerToken);
        //    await _unitOfWork.Repository<Order>().AddAsync(order);
        //    await _unitOfWork.CompleteAsync();
        //    var createdOrderDto = _mapper.Map<OrderDTO>(order);
        //    return createdOrderDto;
        //}

        //the bellow one is working fine
            public async Task<OrderDTO> AddOrderAsync(OrderDTO orderDto)
        {
            var bearerToken = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            //Validate customer
            if (!await _customerService.CustomerExistsAsync(orderDto.CustomerId, bearerToken))
            {
                throw new CustomerNotFoundException(orderDto.CustomerId);
                //return NotFound($"Customer with ID {orderDto.CustomerId} does not exist.");
            }


            var existingOrder = (await _unitOfWork.Repository<Order>()
    .GetAllAsync(
        o => o.CustomerId == orderDto.CustomerId &&
             o.OrderItems.Any(oi => orderDto.OrderItems.Select(i => i.ProductId).Contains(oi.ProductId)),
        include: q => q.Include(o => o.OrderItems))) // Use the include parameter
    .FirstOrDefault();

            if (existingOrder != null)
            {
                // Update quantities for existing products
                foreach (var itemDto in orderDto.OrderItems)
                {
                    var existingItem = existingOrder.OrderItems.FirstOrDefault(oi => oi.ProductId == itemDto.ProductId);

                    if (existingItem != null)
                    {
                        existingItem.Quantity += itemDto.Quantity; // Update the quantity
                    }
                    else
                    {
                        // Add new product to the existing order
                        var (price, taxPercentage) = await _productService.GetProductDetailsAsync(itemDto.ProductId, bearerToken);
                        existingOrder.OrderItems.Add(new OrderItem
                        {
                            ProductId = itemDto.ProductId,
                            Quantity = itemDto.Quantity,
                            UnitPrice = price
                        });
                    }
                }

                await CalculateOrderTotals(existingOrder, bearerToken);
                _unitOfWork.Repository<Order>().Update(existingOrder);
            }
            else
            {
                // Group and sum quantities for duplicate product IDs
                var groupedOrderItems = orderDto.OrderItems
                    .GroupBy(item => item.ProductId)
                    .Select(g => new OrderItemDTO
                    {
                        ProductId = g.Key,
                        Quantity = g.Sum(item => item.Quantity)
                    })
                    .ToList();

                // Validate products and create new order
                var orderItems = new List<OrderItem>();
                foreach (var item in groupedOrderItems)
                {
                    //if (!await _productService.ProductExistsAsync(item.ProductId, bearerToken))
                    //{
                    //    throw new ProductNotFoundException(item.ProductId);
                    //}
                    try { 
                    var (price, taxPercentage) = await _productService.GetProductDetailsAsync(item.ProductId, bearerToken);
                    orderItems.Add(new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = price
                    });

                }
    catch (ProductNotFoundException ex)
    {
                    // Handle the case where the product doesn't exist
                    //_logger.LogWarning(ex.Message);
                    throw new ProductNotFoundException(item.ProductId);
                }
            }

                var order = new Order
                {
                    OrderId = Guid.NewGuid(),
                    CustomerId = orderDto.CustomerId,
                    OrderDate = DateTime.UtcNow,
                    DiscountPercentage = orderDto.DiscountPercentage,
                    OrderItems = orderItems
                };

                await CalculateOrderTotals(order, bearerToken);
                await _unitOfWork.Repository<Order>().AddAsync(order);
                existingOrder = order; // Set this to return the newly created order
            }

            await _unitOfWork.CompleteAsync();
            return _mapper.Map<OrderDTO>(existingOrder);
        }

        public async Task<OrderDTO> UpdateOrderAsync(OrderDTO orderDto)
            {
                var existingOrder = await _unitOfWork.Repository<Order>().GetByIdAsync(orderDto.OrderId,
                    include: q => q.Include(o => o.OrderItems));

                if (existingOrder == null)
                {
                    throw new Exception($"Order with ID {orderDto.OrderId} not found.");
                }
        
            var bearerToken = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            // Validate customer
            if (!await _customerService.CustomerExistsAsync(orderDto.CustomerId,bearerToken))
                {
                    throw new Exception($"Customer with ID {orderDto.CustomerId} does not exist.");
                }
                // Group and sum quantities for duplicate product IDs
                var groupedOrderItems = orderDto.OrderItems
                    .GroupBy(item => item.ProductId)
                    .Select(g => new OrderItemDTO
                    {
                        ProductId = g.Key,
                        Quantity = g.Sum(item => item.Quantity)
                    })
                    .ToList();
                // Validate products and update order items
                var updatedOrderItems = new List<OrderItem>();
                foreach (var item in groupedOrderItems)
                {
                    if (!await _productService.ProductExistsAsync(item.ProductId, bearerToken))
                    {
                        throw new Exception($"Product with ID {item.ProductId} does not exist.");
                    }

                    var (price, taxPercentage) = await _productService.GetProductDetailsAsync(item.ProductId, bearerToken);
                    var existingItem = existingOrder.OrderItems.FirstOrDefault(oi => oi.ProductId == item.ProductId);

                    if (existingItem != null)
                    {
                        existingItem.Quantity = item.Quantity;
                        existingItem.UnitPrice = price;
                        updatedOrderItems.Add(existingItem);
                    }
                    else
                    {
                        updatedOrderItems.Add(new OrderItem
                        {
                            OrderId = existingOrder.OrderId,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            UnitPrice = price
                        });
                    }
                }

                existingOrder.CustomerId = orderDto.CustomerId;
                existingOrder.OrderDate = orderDto.OrderDate;
                existingOrder.DiscountPercentage = orderDto.DiscountPercentage;
                existingOrder.OrderItems = updatedOrderItems;

                await CalculateOrderTotals(existingOrder,bearerToken);
                _unitOfWork.Repository<Order>().Update(existingOrder);
                await _unitOfWork.CompleteAsync();
            var createdOrderDto = _mapper.Map<OrderDTO>(existingOrder);
            return createdOrderDto;
        }

            private async Task CalculateOrderTotals(Order order, string bearerToken)
            {
                decimal totalAmount = 0;
                foreach (var item in order.OrderItems)
                {
                    var (price, taxPercentage) = await _productService.GetProductDetailsAsync(item.ProductId, bearerToken);

                    decimal itemTotal = item.UnitPrice * item.Quantity;
                    decimal tax = itemTotal * (taxPercentage / 100);
                    totalAmount += itemTotal + tax;
                }

                order.TotalAmount = totalAmount;
                order.DiscountedTotal = totalAmount * (1 - order.DiscountPercentage / 100);
            }
        public async Task DeleteOrderAsync(Guid id)
        {
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(id);
            if (order != null)
            {
                _unitOfWork.Repository<Order>().Remove(order);
                await _unitOfWork.CompleteAsync();
            }
            else
            {
                //throw new Exception($"Order with ID {id} not found.");
                throw new OrderNotFoundException(id);
            }
        }
    }
   }
