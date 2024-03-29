﻿using System.IdentityModel.Tokens.Jwt;
using Azure.Identity;
using HealthChecks.UI.Client;
using Me.Services.EventBus;
using Me.Services.EventBus.Abstractions;
using Me.Services.EventBusRabbitMQ;
using Me.Services.EventBusServiceBus;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;

namespace Me.Services.Common;

public static class CommonExtensions
{
    public static WebApplicationBuilder AddServiceDefaults(this WebApplicationBuilder builder)
    {
        // Shared configuration via key vault
        //        builder.Configuration.AddKeyVault();

        // Shared app insights configuration
        //        builder.Services.AddApplicationInsights(builder.Configuration);

        // Default health checks assume the event bus and self health checks
        builder.Services.AddDefaultHealthChecks(builder.Configuration);

        // Add the event bus
        builder.Services.AddEventBus(builder.Configuration);


        builder.Services.AddDefaultAuthentication(builder.Configuration);

        builder.Services.AddDefaultOpenApi(builder.Configuration);

        // Add the accessor
        builder.Services.AddHttpContextAccessor();

        return builder;
    }


    /// <summary>
    /// Resolve ExceptionHandling, Routing, Authorization, OpenApi and HealthChecks
    /// </summary>
    /// <param name="app"></param>
    /// <returns>The app Microsoft.AspNetCore.Builder.ControllerActionEndpointConventionBuilder for
    /// endpoints associated with controller actions.</returns>
    public static WebApplication UseServiceDefaults(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
        }

        var pathBase = app.Configuration["PATH_BASE"];

        if (!string.IsNullOrEmpty(pathBase))
        {
            app.UsePathBase(pathBase);
            app.UseRouting();

            var identitySection = app.Configuration.GetSection("AzureAdB2C");

            if (identitySection.Exists())
            {
                // We have to add the auth middleware to the pipeline here
                app.UseAuthentication();
                app.UseAuthorization();
            }
        }

        app.UseDefaultOpenApi(app.Configuration);

        app.MapDefaultHealthChecks();

        return app;
    }


    // public static async Task<bool> CheckHealthAsync(this WebApplication app)
    // {
    //     app.Logger.LogInformation("Running health checks...");

    //     // Do a health check on startup, this will throw an exception if any of the checks fail
    //     var report = await app.Services.GetRequiredService<HealthCheckService>().CheckHealthAsync();

    //     if (report.Status == HealthStatus.Unhealthy)
    //     {
    //         app.Logger.LogCritical("Health checks failed!");
    //         foreach (var entry in report.Entries)
    //         {
    //             if (entry.Value.Status == HealthStatus.Unhealthy)
    //             {
    //                 app.Logger.LogCritical("{Check}: {Status}", entry.Key, entry.Value.Status);
    //             }
    //         }

    //         return false;
    //     }

    //     return true;
    // }


    public static IApplicationBuilder UseDefaultOpenApi(this WebApplication app, IConfiguration configuration)
    {
        var openApiSection = configuration.GetSection("OpenApi");

        if (!openApiSection.Exists())
        {
            return app;
        }

        app.UseSwagger();
        app.UseSwaggerUI(setup =>
        {
            /// {
            ///   "OpenApi": {
            ///     "Endpoint: {
            ///         "Name": 
            ///     },
            ///     "Auth": {
            ///         "ClientId": ..,
            ///         "AppName": ..
            ///     }
            ///   }
            /// }

            var pathBase = configuration["PATH_BASE"];
            var authSection = openApiSection.GetSection("Auth");
            var endpointSection = openApiSection.GetRequiredSection("Endpoint");
            var swaggerUrl = endpointSection["Url"] ?? $"{(!string.IsNullOrEmpty(pathBase) ? pathBase : string.Empty)}/swagger/v1/swagger.json";

            setup.SwaggerEndpoint(swaggerUrl, endpointSection.GetRequiredValue("Name"));

            if (authSection.Exists())
            {
                setup.OAuthClientId(authSection.GetRequiredValue("ClientId"));
                setup.OAuthAppName(authSection.GetRequiredValue("AppName"));
            }
        });

        // Add a redirect from the root of the app to the swagger endpoint
        app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

        return app;
    }


    public static IServiceCollection AddDefaultOpenApi(this IServiceCollection services, IConfiguration configuration)
    {
        var openApi = configuration.GetSection("OpenApi");

        var sp = services.BuildServiceProvider();
        var logger = sp.GetRequiredService<ILogger<OpenApiInfo>>();

        if (!openApi.Exists())
        {
            return services;
        }

        services.AddEndpointsApiExplorer();

        return services.AddSwaggerGen(options =>
        {
            /// {
            ///   "OpenApi": {
            ///     "Document": {
            ///         "Title": ..
            ///         "Version": ..
            ///         "Description": ..
            ///     }
            ///   }
            /// }
            var document = openApi.GetRequiredSection("Document");

            var version = document.GetRequiredValue("Version") ?? "v1";

            options.SwaggerDoc(version, new OpenApiInfo
            {
                Title = document.GetRequiredValue("Title"),
                Version = version,
                Description = document.GetRequiredValue("Description")
            });

            // var identitySection = configuration.GetSection("Identity");

            // if (!identitySection.Exists())
            // {
            //     // No identity section, so no authentication open api definition
            //     return;
            // }

            // {
            //   "Identity": {
            //     "ExternalUrl": "http://identity",
            //     "Scopes": {
            //         "cart": "Cart API"
            //      }
            //    }
            // }

            // var identityUrlExternal = identitySection["ExternalUrl"] ?? identitySection.GetRequiredValue("Url");
            // var scopes = identitySection.GetRequiredSection("Scopes").GetChildren().ToDictionary(p => p.Key, p => p.Value);

            // options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            // {
            //     Type = SecuritySchemeType.OAuth2,
            //     Flows = new OpenApiOAuthFlows()
            //     {
            //         Implicit = new OpenApiOAuthFlow()
            //         {
            //             AuthorizationUrl = new Uri($"{identityUrlExternal}/connect/authorize"),
            //             TokenUrl = new Uri($"{identityUrlExternal}/connect/token"),
            //             Scopes = scopes,
            //         }
            //     }
            // });

            // options.OperationFilter<AuthorizeCheckOperationFilter>();
        });
    }


    public static IServiceCollection AddDefaultAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // {
        //   "Identity": {
        //     "Url": "http://identity",
        //     "Audience": "cart"
        //    }
        // }

        // var identitySection = configuration.GetSection("Identity");

        // if (!identitySection.Exists())
        // {
        //     // No identity section, so no authentication
        //     return services;
        // }

        // // prevent from mapping "sub" claim to nameidentifier.
        // JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");

        // services.AddAuthentication().AddJwtBearer(options =>
        // {
        //     var identityUrl = identitySection.GetRequiredValue("Url");
        //     var audience = identitySection.GetRequiredValue("Audience");

        //     options.Authority = identityUrl;
        //     options.RequireHttpsMetadata = false;
        //     options.Audience = audience;
        //     options.TokenValidationParameters.ValidateAudience = false;
        // });

        // Configure AAD B2C Authentication instead of custom Identity provider
        // Adds Microsoft Identity platform (Azure AD B2C) support to protect this Api

        // This configuration is required BEFORE OpenIdConnectOptions.
        // By default, the claims mapping will map claim names in the old format to accommodate older SAML applications.
        // For instance, 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role' instead of 'roles' claim.
        // This flag ensures that the ClaimsIdentity claims collection will be built from the claims in the token
        const string JWT_CONFIGURATION_SECTION_NAME = "AzureAdB2C";
        var logger = services.BuildServiceProvider().GetRequiredService<ILogger<MicrosoftIdentityOptions>>();

        // Using environment variables (e.g. AzureAdB2C__Instance) instead of appsettings.json.
        var azureAdB2C = configuration.GetSection(JWT_CONFIGURATION_SECTION_NAME);
        if (!azureAdB2C.Exists())
        {
            logger.LogError("--> {section} not found. Skipping authorization initialization.", JWT_CONFIGURATION_SECTION_NAME);
            return services;
        }

        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(options =>
        {
            logger.LogInformation("--> Initializing authorization for: {section}", JWT_CONFIGURATION_SECTION_NAME);
            configuration.Bind(JWT_CONFIGURATION_SECTION_NAME, options);

            // Can map claim types here
            options.TokenValidationParameters.NameClaimType = "name";
        },
        options => { configuration.Bind(JWT_CONFIGURATION_SECTION_NAME, options); }
        );
        // End of the Microsoft Identity platform block  

        return services;
    }


    public static ConfigurationManager AddKeyVault(this ConfigurationManager configuration)
    {
        // {
        //   "Vault": {
        //     "Name": "myvault",
        //     "TenantId": "mytenantid",
        //     "ClientId": "myclientid",
        //    }
        // }

        var vaultSection = configuration.GetSection("Vault");

        if (!vaultSection.Exists())
        {
            return configuration;
        }

        var credential = new ClientSecretCredential(
            vaultSection.GetRequiredValue("TenantId"),
            vaultSection.GetRequiredValue("ClientId"),
            vaultSection.GetRequiredValue("ClientSecret"));

        var name = vaultSection.GetRequiredValue("Name");

        configuration.AddAzureKeyVault(new Uri($"https://{name}.vault.azure.net/"), credential);

        return configuration;
    }


    public static IServiceCollection AddApplicationInsights(this IServiceCollection services, IConfiguration configuration)
    {
        var appInsightsSection = configuration.GetSection("ApplicationInsights");

        // No instrumentation key, so no application insights
        if (string.IsNullOrEmpty(appInsightsSection["InstrumentationKey"]))
        {
            return services;
        }

        services.AddApplicationInsightsTelemetry(configuration);
        //services.AddApplicationInsightsKubernetesEnricher();
        return services;
    }


    public static IHealthChecksBuilder AddDefaultHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var hcBuilder = services.AddHealthChecks();

        // Health check for the application itself
        hcBuilder.AddCheck("self", () => HealthCheckResult.Healthy());

        // {
        //   "EventBus": {
        //     "ProviderName": "ServiceBus | RabbitMQ",
        //   }
        // }

        var eventBusSection = configuration.GetSection("EventBus");

        //GMC        if (!eventBusSection.Exists())
        //GMC        {
        return hcBuilder;
        //GMC        }

        //GMC        return eventBusSection["ProviderName"]?.ToLowerInvariant() switch
        //GMC        {
        //GMC            "servicebus" => hcBuilder.AddAzureServiceBusTopic(
        //GMC                    _ => configuration.GetRequiredConnectionString("EventBus"),
        //GMC                    _ => "eshop_event_bus",
        //GMC                    name: "servicebus",
        //GMC                    tags: new string[] { "ready" }),
        //GMC
        //GMC            _ => hcBuilder.AddRabbitMQ(
        //GMC                    _ => $"amqp://{configuration.GetRequiredConnectionString("EventBus")}",
        //GMC                    name: "rabbitmq",
        //GMC                    tags: new string[] { "ready" })
        //GMC        };
    }


    public static IServiceCollection AddEventBus(this IServiceCollection services, IConfiguration configuration)
    {
        //  {
        //    "ConnectionStrings": {
        //      "EventBus": "..."
        //    },

        // {
        //   "EventBus": {
        //     "ProviderName": "ServiceBus | RabbitMQ",
        //     ...
        //   }
        // }

        // {
        //   "EventBus": {
        //     "ProviderName": "ServiceBus",
        //     "SubscriptionClientName": "eshop_event_bus"
        //   }
        // }

        // {
        //   "EventBus": {
        //     "ProviderName": "RabbitMQ",
        //     "SubscriptionClientName": "...",
        //     "UserName": "...",
        //     "Password": "...",
        //     "RetryCount": 1
        //   }
        // }

        const string EVENT_BUS_CONFIGURATION_SECTION_NAME = "EventBus";
        var logger = services.BuildServiceProvider().GetRequiredService<ILogger<EventBusServiceBusImpl>>();
        var eventBusSection = configuration.GetSection(EVENT_BUS_CONFIGURATION_SECTION_NAME);
        if (!eventBusSection.Exists())
        {
            logger.LogError("--> {section} not found. Skipping EventBus initialization.", EVENT_BUS_CONFIGURATION_SECTION_NAME);
            return services;
        }

        // ProviderName means we are configuring Azure Service Bus
        if (string.Equals(eventBusSection["ProviderName"], EVENT_BUS_CONFIGURATION_SECTION_NAME, StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IServiceBusPersisterConnection>(sp =>
            {
                var serviceBusConnectionString = configuration.GetRequiredConnectionString(EVENT_BUS_CONFIGURATION_SECTION_NAME);

                return new DefaultServiceBusPersisterConnection(serviceBusConnectionString);
            });

            services.AddSingleton<IEventBus, EventBusServiceBusImpl>(sp =>
            {
                var serviceBusPersisterConnection = sp.GetRequiredService<IServiceBusPersisterConnection>();
                var logger = sp.GetRequiredService<ILogger<EventBusServiceBusImpl>>();
                var eventBusSubscriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();
                string subscriptionName = eventBusSection.GetRequiredValue("SubscriptionClientName");

                return new EventBusServiceBusImpl(serviceBusPersisterConnection, logger,
                    eventBusSubscriptionsManager, sp, subscriptionName);
            });
        }
        else    // We are configuring RabbitMQ
        {
            logger.LogInformation("--> Initializing RabbitMQ for section: {section}", EVENT_BUS_CONFIGURATION_SECTION_NAME);
            services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();

                var factory = new ConnectionFactory()
                {
                    HostName = configuration.GetRequiredConnectionString(EVENT_BUS_CONFIGURATION_SECTION_NAME),
                    DispatchConsumersAsync = true
                };

                if (!string.IsNullOrEmpty(eventBusSection["UserName"]))
                {
                    factory.UserName = eventBusSection["UserName"];
                }

                if (!string.IsNullOrEmpty(eventBusSection["Password"]))
                {
                    factory.Password = eventBusSection["Password"];
                }

                var retryCount = eventBusSection.GetValue("RetryCount", 5);

                return new DefaultRabbitMQPersistentConnection(factory, logger, retryCount);
            });

            services.AddSingleton<IEventBus, EventBusRabbitMQImpl>(sp =>
            {
                var subscriptionClientName = eventBusSection.GetRequiredValue("SubscriptionClientName");
                var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
                var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQImpl>>();
                var eventBusSubscriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();
                var retryCount = eventBusSection.GetValue("RetryCount", 5);

                return new EventBusRabbitMQImpl(rabbitMQPersistentConnection, logger, sp, eventBusSubscriptionsManager, subscriptionClientName, retryCount);
            });
        }

        services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();
        return services;
    }


    public static void MapDefaultHealthChecks(this IEndpointRouteBuilder routes)
    {
        routes.MapHealthChecks("/hc", new HealthCheckOptions()
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        routes.MapHealthChecks("/liveness", new HealthCheckOptions
        {
            Predicate = r => r.Name.Contains("self")
        });
    }
}
