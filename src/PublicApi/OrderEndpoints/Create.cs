using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Web.ViewModels.Order;
using Swashbuckle.AspNetCore.Annotations;

namespace Microsoft.eShopWeb.PublicApi.OrderEndpoints;

public class Create : EndpointBaseAsync
    .WithRequest<CreateOrderRequest>
    .WithActionResult<CreateOrderResponse>
{
    private readonly IOrderService _orderService;
    public Create(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost("api/order")]
    [SwaggerOperation(
        Summary = "Creates a new order",
        Description = "Creates a new order",
        OperationId = "order.create",
        Tags = new[] { "OrderEndpoints" })
    ]
    public override async Task<ActionResult<CreateOrderResponse>> HandleAsync([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var response = new CreateOrderResponse(request.CorrelationId());

        var orderDetails = await _orderService.CreateOrderAsync(
            request.BasketId,
            new Address(
                request.Street,
                request.City,
                request.State,
                request.Country,
                request.ZipCode),
            request.OrderDetails );

        response.Order.Items = orderDetails.OrderItems.Select(
            item => new OrderItemDto() { ItemId = item.Id, Quantity = item.Units })
            .ToList();
        response.Order.BuyerId = orderDetails.BuyerId;
        return response;
    }
}
