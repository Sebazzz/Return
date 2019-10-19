// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : IAppFixture.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Common {
    public interface IAppFixture {
        ReturnAppFactory App { get; set; }
        void OnInitialized() { }
    }
}
