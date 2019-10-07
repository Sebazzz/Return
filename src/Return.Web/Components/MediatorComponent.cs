// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : MediatorComponent.cs
//  Project         : Return.Web
// ******************************************************************************

namespace Return.Web.Components {
    using MediatR;
    using Microsoft.AspNetCore.Components;

    public class MediatorComponent : ComponentBase {
        [Inject]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public IMediator Mediator { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    }
}
