global using System.Data;
global using Me.Services.Purchase.Domain.AggregatesModel.BuyerAggregate;
global using Me.Services.Purchase.Domain.AggregatesModel.OrderAggregate;
global using Me.Services.Purchase.Domain.Exceptions;
global using Me.Services.Purchase.Domain.SeedWork;
global using Me.Services.Purchase.Infrastructure.EntityConfigurations;
global using Me.Services.Purchase.Infrastructure.Idempotency;
global using MediatR;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Design;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.EntityFrameworkCore.Storage;