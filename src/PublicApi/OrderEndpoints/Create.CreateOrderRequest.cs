using System.Collections.Generic;
using Microsoft.eShopWeb.Web.ViewModels.Order;

namespace Microsoft.eShopWeb.PublicApi.OrderEndpoints;

public class CreateOrderRequest : BaseRequest
{
    public int BasketId { get; set; }

    public string Street { get; set; }

    public string City { get; set; }

    public string State { get; set; }

    public string Country { get; set; }

    public string ZipCode { get; set; }

    public string OrderDetails { get; set; }
}
