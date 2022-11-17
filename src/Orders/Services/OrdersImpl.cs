using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Ingredients.Protos;
using Microsoft.AspNetCore.Authorization;
using Orders.Protos;
using Orders.PubSub;

namespace Orders.Services;

public class OrdersImpl : OrderService.OrderServiceBase
{
    private readonly IngredientsService.IngredientsServiceClient _ingredients;
    private readonly IOrderPublisher _orderPublisher;
    private readonly IOrderMessages _orderMessages;
    private readonly ILogger<OrdersImpl> _logger;

    public OrdersImpl(IngredientsService.IngredientsServiceClient ingredients,
        IOrderPublisher orderPublisher,
        IOrderMessages orderMessages,
        ILogger<OrdersImpl> logger)
    {
        _ingredients = ingredients;
        _orderPublisher = orderPublisher;
        _orderMessages = orderMessages;
        _logger = logger;
    }

    [Authorize]
    public override async Task<PlaceOrderResponse> PlaceOrder(PlaceOrderRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Order placed from {Client}",
            context.GetHttpContext().User.Identity.Name);
        
        if (request.CrustId.Length == 0)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "crust_id is required"));
        }
        
        var decrementToppingsRequest = new DecrementToppingsRequest
        {
            ToppingIds = { request.ToppingIds }
        };
        await _ingredients.DecrementToppingsAsync(decrementToppingsRequest);

        var decrementCrustsRequest = new DecrementCrustsRequest
        {
            CrustIds = { request.CrustId }
        };
        await _ingredients.DecrementCrustsAsync(decrementCrustsRequest);

        var dueBy = DateTimeOffset.UtcNow.AddMinutes(45);

        await _orderPublisher.PublishOrder(request.CrustId, request.ToppingIds, dueBy, Guid.NewGuid().ToString());
        
        return new PlaceOrderResponse
        {
            DueBy = dueBy.ToTimestamp()
        };
    }

    public override async Task Subscribe(SubscribeRequest request, IServerStreamWriter<OrderNotification> responseStream, ServerCallContext context)
    {
        var cancellationToken = context.CancellationToken;

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var message = await _orderMessages.ReadAsync(cancellationToken);

                var notification = new OrderNotification
                {
                    CrustId = message.CrustId,
                    ToppingIds = { message.ToppingIds },
                    DueBy = message.Time.ToTimestamp(),
                    NotificationId = message.NotificationId,
                };

                try
                {
                    await responseStream.WriteAsync(notification, cancellationToken);
                }
                catch
                {
                    await _orderPublisher.PublishOrder(message.CrustId, message.ToppingIds, message.Time, message.NotificationId);
                    throw;
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}