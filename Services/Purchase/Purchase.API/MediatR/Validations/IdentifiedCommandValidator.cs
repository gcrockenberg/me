﻿namespace Me.Services.Purchase.API.MediatR.Validations;

public class IdentifiedCommandValidator : AbstractValidator<IdentifiedCommand<CreateOrderCommand, Order>>
{
    public IdentifiedCommandValidator(ILogger<IdentifiedCommandValidator> logger)
    {
        RuleFor(command => command.Id).NotEmpty();

        logger.LogTrace("INSTANCE CREATED - {ClassName}", GetType().Name);
    }
}
