// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : QueryTestBase.cs
//  Project         : Return.Application.Tests.Unit
// ******************************************************************************

namespace Return.Application.Tests.Unit.Support {
    using System;
    using Application.Common.Mapping;
    using AutoMapper;
    using Persistence;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "Not necessary for test")]
    public class QueryTestBase : IDisposable {
        public QueryTestBase() {
            this.Context = ReturnDbContextFactory.Create();

            var configurationProvider = new MapperConfiguration(configure: cfg => {
                cfg.AddProfile<MappingProfile>();
            });

            this.Mapper = configurationProvider.CreateMapper();
        }

        public ReturnDbContext Context { get; }
        public IMapper Mapper { get; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = "Not necessary for test")]
        public void Dispose() => ReturnDbContextFactory.Destroy(context: this.Context);
    }
}
