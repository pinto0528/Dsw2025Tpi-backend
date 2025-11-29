using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Dsw2025Tpi.Api.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        
        {
            _orderService = orderService;
        }

        [HttpPost]
        [Authorize(Roles = "CLIENT")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderModel.OrderRequest request)
        {
            var response = await _orderService.Add(request);
            return CreatedAtAction(
                 nameof(GetById),
                 new { id = response.Id },
                 response
             );
        }

        [HttpGet("{id:guid}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var response = await _orderService.GetById(id);
            return Ok(response);
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetAllOrders()
        {
            var order = await _orderService.GetAllOrders();
            if (order == null) return NotFound();
            return Ok(order);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]

        public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] OrderStatusModel nw)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var order = await _orderService.UpdateOrderStatus(id, nw);
                return Ok(order);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }

        }

    }
}