// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RequestValidationBehavior.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Common.Behaviours
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentValidation;
    using FluentValidation.Results;
    using MediatR;

    public sealed class RequestValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public RequestValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            this._validators = validators;
        }

        public Task<TResponse> Handle(
            TRequest request,
            CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next
        )
        {
            if (next == null) throw new ArgumentNullException(nameof(next));
            var context = new ValidationContext(instanceToValidate: request);

            List<ValidationFailure> failures = this._validators.Select(selector: v => v.Validate(context: context)).
                SelectMany(selector: result => result.Errors).
                Where(predicate: f => f != null).
                ToList();

            if (failures.Count != 0)
            {
                throw new ValidationException(errors: failures);
            }

            return next();
        }
    }
}
