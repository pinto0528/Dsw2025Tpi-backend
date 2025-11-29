using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dsw2025Tpi.Domain.Enum;

namespace Dsw2025Tpi.Domain.Entities
{
    public class Order : EntityBase
    {
        public DateTime Date { get; set; }
        public string ShippingAddress { get; set; }
        public string BillingAddress { get; set; }
        public string? Notes { get; set; }
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItem> OrderItems { get; set; }
        public Guid CustomerId { get; set; }
        public Customer Customer { get; set; }
        public bool IsFinalState => Status == OrderStatus.DELIVERED || Status == OrderStatus.CANCELLED;

        // Calculo de TotalAmount
        public void CalculateTotalAmount()
        {
            TotalAmount = OrderItems.Sum(item => item.Subtotal);
        }
        public bool CanTransitionTo(OrderStatus newStatus)
        {
            if (Status == newStatus) return false;

            return Status switch
            {
                OrderStatus.PENDING => newStatus == OrderStatus.PROCESSING || newStatus == OrderStatus.CANCELLED,
                OrderStatus.PROCESSING => newStatus == OrderStatus.SHIPPED || newStatus == OrderStatus.CANCELLED,
                OrderStatus.SHIPPED => newStatus == OrderStatus.DELIVERED,
                _ => false,
            };
        }

        public void ChangeStatus(OrderStatus newStatus)
        {
            if (!CanTransitionTo(newStatus))
                throw new InvalidOperationException($"Transición a {newStatus} no valida.");

            Status = newStatus;
        }
    }
}