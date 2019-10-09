// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : AbstractStageCommandHandler.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.RetrospectiveWorkflows.Commands {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Common;
    using Application.Common.Abstractions;
    using Common;
    using Domain.Entities;
    using MediatR;
    using Services;

    public abstract class AbstractStageCommandHandler<TRequest> : IRequestHandler<TRequest> where TRequest : AbstractStageCommand, IRequest {
        private readonly IRetrospectiveStatusUpdateDispatcher _retrospectiveStatusUpdateDispatcher;
        private readonly IReturnDbContextFactory _dbContextFactory;

#nullable disable
        protected IReturnDbContext DbContext { get; private set; }
#nullable enable

        protected AbstractStageCommandHandler(IReturnDbContextFactory returnDbContext, IRetrospectiveStatusUpdateDispatcher retrospectiveStatusUpdateDispatcher) {
            this._dbContextFactory = returnDbContext;
            this._retrospectiveStatusUpdateDispatcher = retrospectiveStatusUpdateDispatcher;
        }

        public async Task<Unit> Handle(TRequest request, CancellationToken cancellationToken) {
            if (request == null) throw new ArgumentNullException(nameof(request));

            try {
                this.DbContext = this._dbContextFactory.CreateForEditContext();
                Retrospective? retrospective = await this.DbContext.Retrospectives.FindByRetroId(request.RetroId, cancellationToken);

                if (retrospective == null) {
                    throw new NotFoundException();
                }

                return await this.HandleCore(request, retrospective, cancellationToken);
            }
            finally {
                this.DbContext?.Dispose();
            }
        }

        protected abstract Task<Unit> HandleCore(TRequest request, Retrospective retrospective, CancellationToken cancellationToken);

        protected Task DispatchUpdate(Retrospective retrospective, CancellationToken cancellationToken) => this._retrospectiveStatusUpdateDispatcher.DispatchUpdate(retrospective, cancellationToken);
    }
}
