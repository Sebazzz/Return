// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : CleanupObjects.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Common {
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;
    using NUnit.Framework.Interfaces;
    using NUnit.Framework.Internal;

    public static class Disposables {
        private static List<IDisposable> PerTestDisposables { get; } = new List<IDisposable>();
        private static List<IDisposable> PerTestFixtureDisposables { get; } = new List<IDisposable>();

        public static void RegisterForTest(IDisposable disposable) => PerTestDisposables.Add(disposable);
        public static void RegisterForTestFixture(IDisposable disposable) => PerTestFixtureDisposables.Add(disposable);

        public static void CleanupTestDisposables() => DisposeList(PerTestDisposables);
        public static void CleanupTestFixtureDisposables() => DisposeList(PerTestFixtureDisposables);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "We shouldn't fail the test if something cannot be disposed")]
        private static void DisposeList(List<IDisposable> list) {
            foreach (IDisposable disposable in list) {
                TestContext.WriteLine($"Disposing: {disposable}");
                try {
                    disposable?.Dispose();
                }
                catch (Exception ex) {
                    TestContext.WriteLine($"Disposing failed {disposable}: {ex}");
                }
            }

            list.Clear();
        }

        public static T RegisterAsTestDisposable<T>(this T disposable) where T : IDisposable {
            RegisterForTest(disposable);
            return disposable;
        }

        public static T RegisterAsTestFixtureDisposable<T>(this T disposable) where T : IDisposable {
            RegisterForTestFixture(disposable);
            return disposable;
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class CleanupDisposablesAttribute : Attribute, ITestAction {
        public void BeforeTest(ITest test) {

        }

        public void AfterTest(ITest test) {
            Disposables.CleanupTestDisposables();

            if (test.IsSuite) {
                Disposables.CleanupTestFixtureDisposables();
            }
        }

        public ActionTargets Targets { get; } = ActionTargets.Suite | ActionTargets.Test;
    }
}
