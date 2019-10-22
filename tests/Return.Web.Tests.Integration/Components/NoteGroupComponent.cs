// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : NoteGroupComponent.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Components {
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using OpenQA.Selenium;

    public sealed class NoteGroupComponent {
        public NoteGroupComponent(IWebElement webElement) {
            this.WebElement = webElement;
        }

        public IWebElement WebElement { get; }
        public int Id => this.WebElement.GetAttribute<int>("data-id");
        public IWebElement Input => this.WebElement.FindElement(By.ClassName("note-group__title-input"));
        public IWebElement Title => this.WebElement.FindElement(By.ClassName("note-group__title"));
        public IWebElement Content => this.WebElement.FindElement(By.ClassName("note-group__item-list"));

        public IEnumerable<NoteComponent> Notes => this.WebElement.FindElementsByTestElementId("note").Select(x => new NoteComponent(x));

    }
}
