global using System.Net.Sockets;
global using System.Text;
global using System.Text.Json;
global using Me.Services.EventBus;
global using Me.Services.EventBus.Abstractions;
global using Me.Services.EventBus.Events;
global using Me.Services.EventBus.Extensions;
global using Microsoft.Extensions.Logging;
global using Polly;
global using Polly.Retry;
global using RabbitMQ.Client;
global using RabbitMQ.Client.Events;
global using RabbitMQ.Client.Exceptions;
