// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : VoteListComponent.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Components;

using System;
using System.Threading.Tasks;
using Common;
using Microsoft.Playwright;

public sealed class VoteListComponent {
    private readonly ILocator _locator;

    private VoteListComponent(ILocator locator) {
        this._locator = locator;
    }

    public static async Task<VoteListComponent> Create(ILocator locator) =>
        new(locator)
        {
            Id = await locator.GetAttributeAsync<int>("data-id")
        };

    public int Id { get; init; }

    public ILocator VoteButton => this._locator.Locator(".vote-list__vote-button");
    public ILocator Votes => this._locator.Locator(".vote-indicator");

    public async Task IsVoteButtonEnabled(bool enabled)
    {
        if (!enabled) await this.VoteButton.Expected().ToHaveAttributeAsync("class", "vote-list__vote-button vote-list__vote-button--disabled");
        if (enabled) await this.VoteButton.Expected().ToHaveAttributeAsync("class", "vote-list__vote-button");
    }

    public Task ClickVoteButton() => this.VoteButton.ClickAsync();
}
