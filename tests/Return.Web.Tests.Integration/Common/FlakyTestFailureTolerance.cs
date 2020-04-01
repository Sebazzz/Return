// ******************************************************************************
//  © 2020 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : FlakyTestFailureTolerance.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Common {
    using System;
    using NUnit.Framework;
    using NUnit.Framework.Interfaces;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class FlakyTestFailureToleranceAttribute : Attribute, ITestAction {
        public void BeforeTest(ITest test) { }

        public void AfterTest(ITest test) {
            TestContext.ResultAdapter result = TestContext.CurrentContext.Result;

            TestContext.WriteLine($"{nameof(FlakyTestFailureToleranceAttribute)}: Result is {result.Outcome}");
            if (result.Outcome != ResultState.Success &&
                String.Equals(Environment.GetEnvironmentVariable("SCREENSHOT_TEST_FAILURE_TOLERANCE"), Boolean.TrueString, StringComparison.OrdinalIgnoreCase)) {

                Assert.Ignore("This environment is too slow to run this test fixture - this test is skipped - this test is quite heavy and does not respond well in this environment");
            }
        }

        public ActionTargets Targets { get; } = ActionTargets.Test;
    }
}
