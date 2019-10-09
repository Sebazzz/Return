// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : IReturnDbContextFactory.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Common.Abstractions {
    public interface IReturnDbContextFactory {
        /// <summary>
        ///     Creates a disposable copy for edit operations
        /// </summary>
        /// <returns></returns>
        IReturnDbContext CreateForEditContext();
    }
}
