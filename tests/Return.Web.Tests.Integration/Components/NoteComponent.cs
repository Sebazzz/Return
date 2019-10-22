// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : NoteComponent.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Components {
    using OpenQA.Selenium;

    public sealed class NoteComponent {
        private readonly IWebElement _webElement;

        public NoteComponent(IWebElement webElement) {
            this._webElement = webElement;
        }

        public IWebElement Input => this._webElement.FindElement(By.ClassName("textarea"));
        public IWebElement Content => this._webElement.FindElement(By.ClassName("note__content"));
    }
}
