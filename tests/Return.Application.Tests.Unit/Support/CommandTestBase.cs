// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : CommandTestBase.cs
//  Project         : Return.Application.Tests.Unit
// ******************************************************************************

namespace Return.Application.Tests.Unit.Support {
    using System;
    using Persistence;

    public class CommandTestBase : IDisposable {
        protected readonly ReturnDbContext _context;

        public CommandTestBase() {
            this._context = ReturnDbContextFactory.Create();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "Not necessary for test")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = "Not necessary for test")]
        public void Dispose() => ReturnDbContextFactory.Destroy(this._context);
    }
}
