// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : UseRunningAppAttribute.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Common {
    using System;
    using NUnit.Framework;
    using NUnit.Framework.Interfaces;

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class UseRunningAppAttribute : Attribute, ITestAction {
        public void BeforeTest(ITest test) {
            if (test == null) throw new ArgumentNullException(nameof(test));
            if (!(test.Fixture is IAppFixture appFixture)) {
                TestContext.WriteLine($"{nameof(UseRunningAppAttribute)}.{nameof(this.BeforeTest)}: {test.ClassName} is not {typeof(IAppFixture)}");

                return;
            }

            try {
                appFixture.App = UseReturnAppServerAttribute.ServerInstance;

                appFixture.OnInitialized();
            }
            catch (Exception ex) {
                TestContext.WriteLine($"{nameof(UseRunningAppAttribute)}.{nameof(this.BeforeTest)}: {ex}");

                throw;
            }
        }

        public void AfterTest(ITest test) {
            if (test == null) throw new ArgumentNullException(nameof(test));

            try {
                if (test.Fixture is IAppFixture appFixture) {
                    appFixture.App = null;
                }
            }
            catch (Exception ex) {
                TestContext.WriteLine($"{nameof(UseRunningAppAttribute)}.{nameof(this.AfterTest)}: {ex}");

                throw;
            }
        }

        public ActionTargets Targets { get; } = ActionTargets.Default;
    }
}
