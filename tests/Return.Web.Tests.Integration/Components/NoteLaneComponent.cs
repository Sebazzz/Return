// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : NoteLaneComponent.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Components {
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Common;
    using OpenQA.Selenium;

    public sealed class NoteLaneComponent {
        private readonly IWebElement _webElement;

        public NoteLaneComponent(IWebElement webElement) {
            this._webElement = webElement;
        }

        public IWebElement AddNoteButton => this._webElement.FindElementByTestElementId("add-note-button");
        public ReadOnlyCollection<IWebElement> NoteElements => this._webElement.FindElementsByTestElementId("note");
        public IEnumerable<NoteComponent> Notes => this._webElement.FindElementsByTestElementId("note").Select(x => new NoteComponent(x));
    }
}
