using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Enum;
using Dsw2025Tpi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IRepository _orderRepository;
        private readonly IEntityMapper<Order, OrderModel.OrderResponse> _entityMapper;
        public OrderService(IRepository orderRepository, IEntityMapper<Order, OrderModel.OrderResponse> entityMapper)
        {
            _orderRepository = orderRepository;
            _entityMapper = entityMapper;
        }

        public async Task<OrderModel.OrderResponse> Add(OrderModel.OrderRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "El request no puede ser nulo.");
            }

            // Validar que el cliente exista
            var customer = await _orderRepository.First<Customer>(c => c.Id == request.CustomerId);
            if (customer == null)
                throw new NotFoundException($"El cliente con ID {request.CustomerId} no fue encontrado.");

            // Crear diccionario para guardar productos
            var productIds = request.OrderItems.Select(i => i.ProductId).Distinct().ToList();
            var productDict = new Dictionary<Guid, Product>();

            // Cargar diccionario
            foreach (var productId in productIds)
            {
                var product = await _orderRepository.GetById<Product>(productId)
                    ?? throw new NotFoundException($"El producto con ID {productId} no fue encontrado.");

                if (!product.IsActive)
                {
                    throw new InvalidEntityStateException($"El producto {product.Name} no está activo y no puede ser incluido en la orden.");
                }

                productDict[productId] = product;
            }

            // Validar stock
            foreach (var item in request.OrderItems)
            {
                var product = productDict[item.ProductId];

                if (item.Quantity <= 0)
                    throw new ValidationException($"La cantidad del producto {product.Name} debe ser mayor a cero.");

                if (item.Quantity > product.StockQuantity)
                    throw new ValidationException(
                        $"Stock insuficiente para el producto {product.Name}. " +
                        $"Requerido: {item.Quantity}, Disponible: {product.StockQuantity}");
            }

            // Procesar la orden
            var orderEntity = request.ToEntity();

            foreach (var item in orderEntity.OrderItems)
            {
                var product = productDict[item.ProductId];

                product.StockQuantity -= item.Quantity;
                item.UnitPrice = product.CurrentUnitPrice;
                item.CalculateSubtotal();
            }

            orderEntity.CalculateTotalAmount();

            var savedOrderEntity = await _orderRepository.Add(orderEntity);
            return _entityMapper.ToResponse(savedOrderEntity);
        }

        public async Task<IEnumerable<OrderModel.OrderResponse>> GetAllOrders()
        {
            var orderList = await _orderRepository.GetAll<Order>("OrderItems", "OrderItems.Product")
            ?? throw new NotFoundException("La orden no existe.");

            foreach (var order in orderList)
            {
                foreach (var item in order.OrderItems)
                {
                    item.CalculateSubtotal();
                }

                order.CalculateTotalAmount();
            }

            var result = orderList.Select(o =>
            {
                return _entityMapper.ToResponse(o);
            });

            return result;

        }

        public async Task<OrderModel.OrderResponse> GetById(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("El ID de la orden no puede ser un GUID vacío.", nameof(id));
            }

            var order = await _orderRepository.First<Order>(p => p.Id == id, "OrderItems", "OrderItems.Product")
                ?? throw new NotFoundException("La orden no existe.");

            foreach (var item in order.OrderItems)
            {
               item.CalculateSubtotal();
            }

            order.CalculateTotalAmount();

            return _entityMapper.ToResponse(order);

        }

        public async Task<OrderModel.OrderResponse> UpdateOrderStatus(Guid id, OrderStatusModel r)
        {
            if (r is null) throw new ArgumentException("Request vacío.");

            if (!Enum.TryParse<OrderStatus>(r.NewOrderStatus, true, out var newStatus))
            {
                throw new ArgumentException($"Estado inválido: '{r.NewOrderStatus}'.");
            }

            // Obtener la orden con sus relaciones para devolver el DTO completo
            var order = await _orderRepository.First<Order>(p => p.Id == id, "OrderItems", "OrderItems.Product")
                ?? throw new NotFoundException("La orden no existe.");

            foreach (var item in order.OrderItems)
            {
                item.CalculateSubtotal();
            }

            order.CalculateTotalAmount();

            // Validar transición
            if (!order.CanTransitionTo(newStatus))
            {
                throw new ArgumentException($"Transición no permitida de {order.Status} a {newStatus}.");
            }

            // Aplicar cambio y persistir
            order.ChangeStatus(newStatus);
            await _orderRepository.Update<Order>(order);

            return _entityMapper.ToResponse(order);
        }
    }
}