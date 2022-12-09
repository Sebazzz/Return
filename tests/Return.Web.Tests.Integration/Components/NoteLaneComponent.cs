// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : NoteLaneComponent.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Components;

using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Microsoft.Playwright;

public sealed class NoteLaneComponent {
    private readonly ILocator _locator;

    public NoteLaneComponent(ILocator locator) {
        this._locator = locator;
    }

    public ILocator AddNoteButton => this._locator.FindElementByTestElementId("add-note-button");
    public ILocator AddNoteGroupButton => this._locator.FindElementByTestElementId("add-note-group-button");
    public ILocator NoteElements => this._locator.FindElementByTestElementId("note");
    public ILocator NoteGroupElements => this._locator.FindElementByTestElementId("note-group");
    public Task<List<NoteComponent>> Notes() => this._locator.GenerateSubElementsByTestElementId("note", NoteComponent.Create);
    public Task<List<VoteListComponent>> VoteLists() => this._locator.GenerateSubElementsByTestElementId("vote-list", VoteListComponent.Create);
    public Task<List<NoteGroupComponent>> NoteGroups() => this._locator.GenerateSubElementsByTestElementId("note-group", NoteGroupComponent.Create);
}
