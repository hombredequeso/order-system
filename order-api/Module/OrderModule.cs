using System;
using CarrierPidgin.OrderService.Dal;
using CarrierPidgin.OrderService.Domain;
using Nancy;
using Nancy.ModelBinding;
using Npgsql;

namespace CarrierPidgin.OrderService.Module
{
    public class PostOrderBody
    {
        public string Description { get; set; }
    }

    public class ApiOrder
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }

        public static string GetStatus(Order.State state)
        {
            switch (state)
            {
                case Order.State.Active: return "active";
                case Order.State.Cancelled: return "cancelled";
                default: throw new Exception("Unknown order state");
            }
        }

        public static ApiOrder Get(Order order)
        {
            return new ApiOrder
            {
                Description = order.Description,
                Id = order.OrderNumber,
                Status = GetStatus(order.Status)
            };
        }
    }


    public class OrderModule : NancyModule
    {
        private const string ConnectionString =
            "Host=localhost;Username=postgres;Password=mypassword;Database=mydb;Search Path=order";

        public OrderModule()
        {
            Post["/order"] = parameters =>
            {
                var postContent = this.Bind<PostOrderBody>();
                var orderNumber = Guid.NewGuid();
                using (var uow = new UnitOfWork(ConnectionString))
                {
                    var order = new Order(orderNumber, postContent.Description);
                    OrderRepository.Save(uow, order, DateTimeOffset.UtcNow);
                    uow.Commit();
                    return Response.AsJson(ApiOrder.Get(order), HttpStatusCode.Created);
                }
            };

            Get["/order/{id}"] = parameters =>
            {
                Guid orderNumber = Guid.Parse(parameters.id);
                using (NpgsqlConnection dbConnection = new NpgsqlConnection(ConnectionString))
                {
                    var order = OrderRepository.GetOrder(dbConnection, orderNumber);
                    return order != null
                    ? Response.AsJson(ApiOrder.Get(order), HttpStatusCode.OK)
                    : HttpStatusCode.NotFound;

                }
            };

            Post["/order/{id}/cancel"] = parameters =>
            {
                Guid orderId = Guid.Parse(parameters.id);
                using (var uow = new UnitOfWork(ConnectionString))
                {
                    var order = OrderRepository.GetOrder(uow.DbConnection, orderId);
                    if (order == null)
                        return HttpStatusCode.NotFound;

                    order.Cancel();
                    OrderRepository.Save(uow, order, DateTimeOffset.UtcNow);
                    uow.Commit();
                    return Response.AsJson(ApiOrder.Get(order), HttpStatusCode.OK);
                }
            };

            Post["/order/{id}/activate"] = parameters =>
            {
                Guid orderId = Guid.Parse(parameters.id);
                using (var uow = new UnitOfWork(ConnectionString))
                {
                    var order = OrderRepository.GetOrder(uow.DbConnection, orderId);
                    if (order == null)
                        return HttpStatusCode.NotFound;

                    order.Activate();
                    OrderRepository.Save(uow, order, DateTimeOffset.UtcNow);
                    uow.Commit();
                    return Response.AsJson(ApiOrder.Get(order), HttpStatusCode.OK);
                }
            };
        }
    }
}