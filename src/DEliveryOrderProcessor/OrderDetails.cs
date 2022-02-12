using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DeliveryOrderProcessor;

public class OrderDetails
{
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; }

    public string Order { get; set; }

    public override string ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}

public class CreateOrderRequest
{
    public int BasketId { get; set; }

    public string Street { get; set; }

    public string City { get; set; }

    public string State { get; set; }

    public string Country { get; set; }

    public string ZipCode { get; set; }

    public string OrderDetails { get; set; }
}
