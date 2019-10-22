// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ServiceExtensions.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Common {
    using System.Reflection;
    using Application.Common.Abstractions;
    using Services;

    public static class ServiceExtensions {
        public static void SetNoHttpContext(this ICurrentParticipantService currentParticipantService) =>
            typeof(CurrentParticipantService).GetMethod("HttpContext", BindingFlags.NonPublic | BindingFlags.Instance)?.
                Invoke(currentParticipantService, null);
    }
}
