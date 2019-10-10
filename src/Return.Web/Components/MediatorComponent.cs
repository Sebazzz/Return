// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : MediatorComponent.cs
//  Project         : Return.Web
// ******************************************************************************

namespace Return.Web.Components {
    using MediatR;
    using Microsoft.AspNetCore.Components;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

    public abstract class MediatorComponent : ComponentBase {
        [Inject]
        public IMediator Mediator { get; set; }
    }
}
