using System.Collections.Generic;

namespace Microsoft.eShopWeb.PublicApi.OrderEndpoints;

public class OrderDto
{
    public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();

    public string BuyerId { get; set; }
}
