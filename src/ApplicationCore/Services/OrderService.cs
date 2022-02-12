using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using Microsoft.eShopWeb.Web.Features.MessageSender;
using Newtonsoft.Json;

namespace Microsoft.eShopWeb.ApplicationCore.Services;

public class OrderService : IOrderService
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IUriComposer _uriComposer;
    private readonly IRepository<Basket> _basketRepository;
    private readonly IRepository<CatalogItem> _itemRepository;

    public OrderService(IRepository<Basket> basketRepository,
        IRepository<CatalogItem> itemRepository,
        IRepository<Order> orderRepository,
        IUriComposer uriComposer)
    {
        _orderRepository = orderRepository;
        _uriComposer = uriComposer;
        _basketRepository = basketRepository;
        _itemRepository = itemRepository;
    }

    public async Task<Order> CreateOrderAsync(
        int basketId,
        Address shippingAddress,
        string orderData)
    {
        var basketSpec = new BasketWithItemsSpecification(basketId);
        var basket = await _basketRepository.GetBySpecAsync(basketSpec);
        var messageSender = new QueueMessageSender();

        Guard.Against.NullBasket(basketId, basket);
        Guard.Against.EmptyBasketOnCheckout(basket.Items);

        var catalogItemsSpecification = new CatalogItemsSpecification(basket.Items.Select(item => item.CatalogItemId).ToArray());
        var catalogItems = await _itemRepository.ListAsync(catalogItemsSpecification);

        var items = basket.Items.Select(basketItem =>
        {
            var catalogItem = catalogItems.First(c => c.Id == basketItem.CatalogItemId);
            var itemOrdered = new CatalogItemOrdered(catalogItem.Id, catalogItem.Name, _uriComposer.ComposePicUri(catalogItem.PictureUri));
            var orderItem = new OrderItem(itemOrdered, basketItem.UnitPrice, basketItem.Quantity);
            return orderItem;
        }).ToList();

        var order = new Order(basket.BuyerId, shippingAddress, items);

        var orderDetails = await _orderRepository.AddAsync(order);

        var orderCosmosDbData = new OrderDetailsModel
        {
            BasketId = basketId,
            Street = shippingAddress.Street,
            City = shippingAddress.City,
            State = shippingAddress.State,
            Country = shippingAddress.Country,
            ZipCode = shippingAddress?.ZipCode,
            OrderDetails = orderData
        };

        await SendOrderToCosmosDb(orderCosmosDbData);
        await messageSender.SendMessageAsync(orderData);

        return order;
    }


    private async Task SendOrderToCosmosDb(OrderDetailsModel order)
    {
        var client = new HttpClient(); //You should extract this and reuse the same instance multiple times.
        var request = new HttpRequestMessage(HttpMethod.Post, "https://deliveryordersfuncapp.azurewebsites.net/api/OrderCosmosDBProcessing?");
        
        string serializedData = JsonConvert.SerializeObject(order);

        using (var content = new StringContent(serializedData, Encoding.UTF8, "application/json"))
        {
            request.Content = content;
            var response =  await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
    }
}
