﻿namespace Me.Services.Purchase.Domain.AggregatesModel.OrderAggregate;

//This is just the RepositoryContracts or Interface defined at the Domain Layer
//as requisite for the Order Aggregate

public interface IOrderRepository : IRepository<Order>
{
    Task<Order> Add(Order order);

    void Update(Order order);

    Task<Order> GetAsync(int orderId, Boolean withBuyer = false, Boolean withStatus = false,
                         Boolean withItems = false, Boolean withAddress = false);

    Task<OrderStatus> GetOrderStatusAsync(int orderStatusId);
}
