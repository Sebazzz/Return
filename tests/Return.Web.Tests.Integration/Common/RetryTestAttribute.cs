// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RetryTestAttribute.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Common {
    using System;
    using NUnit.Framework;
    using NUnit.Framework.Interfaces;
    using NUnit.Framework.Internal;
    using NUnit.Framework.Internal.Commands;

    /// <summary>
    ///     Specifies that a test method should be rerun on failure up to the specified
    ///     maximum number of times.
    /// </summary>
    [AttributeUsage(validOn: AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
    public sealed class RetryTestAttribute : NUnitAttribute, IRepeatTest {
        private readonly int _tryCount;

        /// <summary>
        ///     Construct a <see cref="RetryAttribute" />
        /// </summary>
        /// <param name="tryCount">The maximum number of times the test should be run if it fails</param>
        public RetryTestAttribute(int tryCount) {
            this._tryCount = tryCount;
        }

        #region IRepeatTest Members

        /// <summary>
        ///     Wrap a command and return the result.
        /// </summary>
        /// <param name="command">The command to be wrapped</param>
        /// <returns>The wrapped command</returns>
        public TestCommand Wrap(TestCommand command) =>
            new RetryCommand(innerCommand: command, tryCount: this._tryCount);

        #endregion

        #region Nested RetryCommand Class

        /// <summary>
        ///     The test command for the <see cref="RetryAttribute" />
        /// </summary>
        private sealed class RetryCommand : DelegatingTestCommand {
            private readonly int _tryCount;

            /// <summary>
            ///     Initializes a new instance of the <see cref="RetryCommand" /> class.
            /// </summary>
            /// <param name="innerCommand">The inner command.</param>
            /// <param name="tryCount">The maximum number of repetitions</param>
            public RetryCommand(TestCommand innerCommand, int tryCount)
                : base(innerCommand: innerCommand) {
                this._tryCount = tryCount;
            }


            /// <summary>
            ///     Runs the test, saving a TestResult in the supplied TestExecutionContext.
            /// </summary>
            /// <param name="context">The context in which the test should run.</param>
            /// <returns>A TestResult</returns>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Needed for retry test")]
            public override TestResult Execute(TestExecutionContext context) {
                int count = this._tryCount;

                while (count-- > 0) {
                    try {
                        context.CurrentResult = this.innerCommand.Execute(context: context);
                    }
                    // Commands are supposed to catch exceptions, but some don't
                    // and we want to look at restructuring the API in the future.
                    catch (Exception ex) {
                        if (context.CurrentResult == null) context.CurrentResult = context.CurrentTest.MakeTestResult();
                        context.CurrentResult.RecordException(ex);
                        context.OutWriter.WriteLine($"[{nameof(RetryAttribute)}] Error executing test: {ex}");
                    }

                    // In general we want to break on certain conditions, but general failures (even if not assertion failures), should be retried
                    if (context.CurrentResult.ResultState != ResultState.Failure && context.CurrentResult.ResultState.Status != ResultState.Error.Status) {
                        break;
                    }

                    // Clear result for retry
                    if (count > 0) {
                        context.CurrentResult = context.CurrentTest.MakeTestResult();
                        context.
                                CurrentRepeatCount
                            ++; // increment Retry count for next iteration. will only happen if we are guaranteed another iteration
                    }
                }

                return context.CurrentResult;
            }
        }

        #endregion
    }
}
