// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : RetryConstraint.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Common {
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;
    using NUnit.Framework;
    using NUnit.Framework.Constraints;
    using Return.Common;

    /// <summary>
    ///     Provides extension methods for a constraint that can be retried
    /// </summary>
    public static class RetryConstraint {
        /// <summary>
        ///     Gets or sets the time to wait between retrying
        /// </summary>
        public static int DefaultRetryTimeGap { get; set; } = 1000;

        /// <summary>
        ///     Gets or sets the number of time to retry
        /// </summary>
        public static int DefaultRetryCount { get; set; } = 10;

        /// <summary>
        ///     Specifies the current constraint must be retried a certain amount of time
        /// </summary>
        public static IResolveConstraint Retry(this IResolveConstraint source, int count = -1, int timeGap = -1) {
            if (count <= 0) {
                count = DefaultRetryCount;
            }

            if (timeGap <= 0) {
                timeGap = DefaultRetryTimeGap;
            }

            var settings = new RetrySettings(retryCount: count, timeGap: timeGap);
            if (source is IConstraint constraint) {
                return new RetryableWithResolveConstraintImpl(wrapped: constraint, retrySettings: settings);
            }

            return new RetryableResolveConstraintImpl(wrapped: source, retrySettings: settings);
        }

        internal static bool IsRetry(IResolveConstraint source) =>
            source is RetryableResolveConstraintImpl || source is RetryableWithResolveConstraintImpl;

        [StructLayout(layoutKind: LayoutKind.Auto)]
        private struct RetrySettings {
            public RetrySettings(int retryCount, int timeGap) {
                this.RetryCount = retryCount;
                this.TimeGap = timeGap;
            }

            public override string ToString() => String.Format(Culture.Invariant, "Retry {0} times waiting {1} ms", this.RetryCount, this.TimeGap);

            public readonly int TimeGap;

            public readonly int RetryCount;
        }


        private sealed class RetryableResolveConstraintImpl : IResolveConstraint {
            private readonly RetrySettings _retrySettings;


            private readonly IResolveConstraint _wrapped;

            /// <inheritdoc />
            public RetryableResolveConstraintImpl(IResolveConstraint wrapped, RetrySettings retrySettings) {
                this._wrapped = wrapped;
                this._retrySettings = retrySettings;
            }

            /// <inheritdoc />
            public IConstraint Resolve() =>
                new RetryableWithResolveConstraintImpl(this._wrapped.Resolve(), retrySettings: this._retrySettings);
        }


        private sealed class RetryableWithResolveConstraintImpl : IConstraint {
            private readonly RetrySettings _retrySettings;


            private readonly IConstraint _wrapped;

            /// <inheritdoc />
            public RetryableWithResolveConstraintImpl(IConstraint wrapped, RetrySettings retrySettings) {
                this._wrapped = wrapped;
                this._retrySettings = retrySettings;
            }


            public IConstraint Resolve() =>
                new RetryableWithResolveConstraintImpl(this._wrapped.Resolve(), retrySettings: this._retrySettings);

            /// <inheritdoc />
            public ConstraintResult ApplyTo<TActual>(TActual actual) =>
                this.ApplyToRetry(resultFactory: () => this._wrapped.ApplyTo(actual: actual));

            /// <inheritdoc />
            public ConstraintResult ApplyTo<TActual>(ActualValueDelegate<TActual> del) =>
                this.ApplyToRetry(resultFactory: () => this._wrapped.ApplyTo(del: del));

            /// <inheritdoc />
            public ConstraintResult ApplyTo<TActual>(ref TActual actual) {
                TActual copy = actual;
                ConstraintResult result;
                try {
                    result = this.ApplyToRetry(resultFactory: delegate {
                        TActual copy2 = copy;
                        ConstraintResult result2 = this._wrapped.ApplyTo(actual: ref copy2);
                        copy = copy2;
                        return result2;
                    });
                }
                finally {
                    actual = copy;
                }

                return result;
            }

            /// <inheritdoc />

            public string DisplayName => $"{this._wrapped.DisplayName} ({this._retrySettings})";

            /// <inheritdoc />

            public string Description => $"{this._wrapped.Description} ({this._retrySettings})";

            /// <inheritdoc />

            public object[] Arguments => this._wrapped.Arguments;

            /// <inheritdoc />
            public ConstraintBuilder Builder {
                get => this._wrapped.Builder;
                set => this._wrapped.Builder = value;
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Constraint testing")]
            private ConstraintResult ApplyToRetry(Func<ConstraintResult> resultFactory) {
                var result = new ConstraintResult(this, this, ConstraintStatus.Failure);
                for (int counter = 0; counter < this._retrySettings.RetryCount; counter++) {
                    try {
                        result = resultFactory();
                        if (result.IsSuccess) {
                            TestContext.WriteLine($"Constraint {result.Status}: {result.Name} {result.Description} - Success");
                            return result;
                        }

                        TestContext.WriteLine($"Constraint {result.Status}: {result.Name} {result.Description} - Retrying");
                    }
                    catch (Exception ex) {
                        TestContext.WriteLine($"Constraint exception: {ex} - Retrying");
                    }

                    Thread.Sleep(this._retrySettings.TimeGap);
                }

                return result;
            }
        }
    }
}
