// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RetrospectiveOnlineList.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Components;

using Microsoft.Playwright;

public class RetrospectiveOnlineListComponent {
    private readonly IPage _browserPage;

    public RetrospectiveOnlineListComponent(IPage browserPage) {
        this._browserPage = browserPage;
    }

    public ILocator OnlineListItems => this._browserPage.Locator("#retrospective-online-list span[data-participant-id]");
    public ILocator GetListItem(int id) => this._browserPage.Locator($"#retrospective-online-list span[data-participant-id=\"{id}\"]");
}
