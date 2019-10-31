// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : SecurityValidator.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Common.Security {
    using System;
    using System.Threading.Tasks;
    using Abstractions;
    using Domain.Abstractions;
    using Domain.Entities;
    using Microsoft.Extensions.Logging;
    using Models;
    using TypeHandling;

    public interface ISecurityValidator {
        ValueTask EnsureOperation(Retrospective retrospective, SecurityOperation operation, object entity);
    }

    public sealed class SecurityValidator : ISecurityValidator {
        private readonly ICurrentParticipantService _currentParticipantService;
        private readonly ILogger<SecurityValidator> _logger;

        public SecurityValidator(ICurrentParticipantService currentParticipantService, ILogger<SecurityValidator> logger) {
            this._currentParticipantService = currentParticipantService;
            this._logger = logger;
        }

        public async ValueTask EnsureOperation(Retrospective retrospective, SecurityOperation operation, object entity) {
            if (retrospective == null) throw new ArgumentNullException(nameof(retrospective));
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            CurrentParticipantModel participant = await this.GetAuthenticatedParticipant(operation, entity.GetType());

            if (operation == SecurityOperation.AddOrUpdate || operation == SecurityOperation.Delete) {
                this.EnsureOperationSecurity(operation, entity, participant);
            }

            this.InvokeTypeSecurityChecks(operation, retrospective, participant, entity);
        }

        private async ValueTask<CurrentParticipantModel> GetAuthenticatedParticipant(SecurityOperation operation, Type entityType) {
            CurrentParticipantModel participant = await this._currentParticipantService.GetParticipant();

            static void ThrowSecurityException(string message) {
                throw new OperationSecurityException(message);
            }

            if (participant.IsAuthenticated == false) {
                string message = $"Operation {operation} on type {entityType} not allowed: user is not authenticated.";
                this._logger.LogError(message);

                ThrowSecurityException(message);
            }

            return participant;
        }

        private void EnsureOperationSecurity(
            SecurityOperation operation,
            object entity,
            in CurrentParticipantModel participant
        ) {
            if (entity is IOwnedByParticipant ownedEntity) {
                if (participant.Id != ownedEntity.ParticipantId && ownedEntity.ParticipantId != 0) {
                    string message =
                        $"Operation '{operation}': Not allowed - entity is owned by participant {ownedEntity.ParticipantId}. Operation is performed by {participant.Id} ({participant.Name})";
                    this._logger.LogError(message);

                    throw new OperationSecurityException(message);
                }
            }
        }

        private void InvokeTypeSecurityChecks(SecurityOperation operation, Retrospective retrospective, in CurrentParticipantModel participant, object entity) {
            try {
                SecurityTypeHandlers.HandleOperation(operation, retrospective, entity, participant);

                if (this._logger.IsEnabled(LogLevel.Trace)) {
                    this._logger.LogTrace($"Operation {operation} granted for entity {entity.GetType()} for participant #{participant.Id}");
                }
            }
            catch (OperationSecurityException ex) {
                string message =
                    $"Failure asserting operation '{operation}' for entity {entity.GetType()} for participant #{participant.Id} ({participant.Name})";
                this._logger.LogError(ex, message);

                throw new OperationSecurityException(message, ex);
            }
        }
    }
}
