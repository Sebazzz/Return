// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : NoteComponent.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Components;

using System.Threading.Tasks;
using Common;
using Microsoft.Playwright;

public sealed class NoteComponent {
    private NoteComponent(ILocator locator) {
        this.Locator = locator;
    }

    public ILocator Locator { get; }

    public static async Task<NoteComponent> Create(ILocator locator) =>
        new(locator)
        {
            Id = await locator.GetAttributeAsync<int>("data-id")
        };

    public int Id { get; init; }

    public ILocator Input => this.Locator.Locator(".textarea");
    public ILocator Content => this.Locator.Locator(".note__content");
    public ILocator DeleteButton => this.Locator.Locator(".note__delete-icon");
}
