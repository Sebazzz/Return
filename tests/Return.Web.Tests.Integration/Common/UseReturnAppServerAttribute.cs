// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : UseReturnAppServerAttribute.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

[assembly: Return.Web.Tests.Integration.Common.UseReturnAppServer]

namespace Return.Web.Tests.Integration.Common {
    using System;
    using NUnit.Framework;
    using NUnit.Framework.Interfaces;

    /// <summary>
    /// Assembly-wide attribute that will set-up the server and database, and also tear it down after the test
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class UseReturnAppServerAttribute : Attribute, ITestAction {
        public void BeforeTest(ITest test) {
            TestContext.WriteLine("Setting up server and initial webdriver");

            ServerInstance = new ReturnAppFactory();
            ServerInstance.InitializeBaseData();
        }

        public void AfterTest(ITest test) {
            TestContext.WriteLine("Tearing down server and core webdriver");

            ServerInstance?.Dispose();
            ServerInstance = null;
        }

        public ActionTargets Targets => ActionTargets.Suite;

        internal static ReturnAppFactory ServerInstance;
    }
}
