// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : SeedBaseDataCommand.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.App.Commands.SeedBaseData {
    using Common.Abstractions;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;

    public sealed class SeedBaseDataCommand : IRequest {
    }

    public sealed class SeedBaseDataCommandHandler : IRequestHandler<SeedBaseDataCommand> {
        private readonly IReturnDbContext _returnDbContext;

        public SeedBaseDataCommandHandler(IReturnDbContext returnDbContext) {
            this._returnDbContext = returnDbContext;
        }

        public async Task<Unit> Handle(SeedBaseDataCommand request, CancellationToken cancellationToken) {
            var seeder = new BaseDataSeeder(this._returnDbContext);

            await seeder.SeedAllAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
