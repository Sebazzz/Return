// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : NoteGroupComponent.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Components;

using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Microsoft.Playwright;

public sealed class NoteGroupComponent {
    private NoteGroupComponent(ILocator locator) {
        this.Locator = locator;
    }

    public ILocator Locator { get; }

    public static async Task<NoteGroupComponent> Create(ILocator locator) =>
        new(locator)
        {
            Id = await locator.GetAttributeAsync<int>("data-id")
        };
    public int Id { get; init; }

    public ILocator Input => this.Locator.Locator(".note-group__title-input");
    public ILocator Title => this.Locator.Locator(".note-group__title");
    public ILocator Content => this.Locator.Locator(".note-group__item-list");

    public ILocator NoteElements => this.Locator.FindElementByTestElementId("note");
    public Task<List<NoteComponent>> Notes() => this.Locator.GenerateSubElementsByTestElementId("note", NoteComponent.Create);
}
