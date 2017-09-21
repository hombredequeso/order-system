using System;
using System.Linq;
using CarrierPidgin.OrderService.ApiEntity;
using CarrierPidgin.OrderService.Dal;
using CarrierPidgin.OrderService.Domain;
using Nancy;
using Nancy.ModelBinding;
using Npgsql;
using OrderLine = CarrierPidgin.OrderService.ApiEntity.OrderLine;

namespace CarrierPidgin.OrderService.Module
{
    public class OrderModule : NancyModule
    {
        private const string ConnectionString =
            "Host=localhost;Username=postgres;Password=mypassword;Database=carrierpidgin;Search Path=order";

        public OrderModule()
        {
            Post["/order"] = parameters =>
            {
                var postContent = this.Bind<PostOrderBody>();
                var orderNumber = Guid.NewGuid();

                using (var uow = new UnitOfWork(ConnectionString))
                {
                    var lines = postContent.Lines.Select(ToDomainLine).ToList();
                    var order = new Order(
                        orderNumber, 
                        postContent.Description, 
                        lines);
                    OrderRepository.Save(uow, order, DateTimeOffset.UtcNow);
                    uow.Commit();
                    return Response.AsJson(SirenOrderFactory.Get(order, Context.Request.Url.SiteBase), HttpStatusCode.Created);
                }
            };

            Get["/order/{id}"] = parameters =>
            {
                Guid orderNumber = Guid.Parse(parameters.id);
                using (NpgsqlConnection dbConnection = new NpgsqlConnection(ConnectionString))
                {
                    var order = OrderRepository.GetOrder(dbConnection, orderNumber);
                    return order != null
                    ? Response.AsJson(SirenOrderFactory.Get(order, Context.Request.Url.SiteBase), HttpStatusCode.OK)
                    : HttpStatusCode.NotFound;

                }
            };


            Post["/order/{id}/cancellation"] = parameters =>
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
                    return Response.AsJson(SirenOrderFactory.Get(order, Context.Request.Url.SiteBase), HttpStatusCode.OK);
                }
            };

            Post["/order/{id}/activation"] = parameters =>
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
                    return Response.AsJson(SirenOrderFactory.Get(order, Context.Request.Url.SiteBase), HttpStatusCode.OK);
                }
            };
        }

        private static Domain.OrderLine ToDomainLine(OrderLine l)
        {
            return new Domain.OrderLine(l.ItemId, l.Quantity, l.PricePerItem);
        }
    }
}