// ******************************************************************************
//  © 2020 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : IAppFixture.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Common;

using System.Threading.Tasks;

public interface IAppFixture
{
    ReturnAppFactory App { get; set; }

    Task OnInitialized();
}
