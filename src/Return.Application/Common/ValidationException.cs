// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ValidationException.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Common {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentValidation.Results;

    public class ValidationException : Exception {
        public ValidationException()
            : base(message: "One or more validation failures have occurred.") {
        }

        public ValidationException(string message) : base(message) {
        }

        public ValidationException(string message, Exception innerException) : base(message, innerException) {
        }

        public ValidationException(List<ValidationFailure> failures)
            : this() {
            IEnumerable<string> propertyNames = failures.Select(selector: e => e.PropertyName).Distinct();

            foreach (string propertyName in propertyNames) {
                string[] propertyFailures = failures.Where(predicate: e => e.PropertyName == propertyName).
                    Select(selector: e => e.ErrorMessage).
                    ToArray();

                this.Failures.Add(key: propertyName, value: propertyFailures);
            }
        }

        public IDictionary<string, string[]> Failures { get; } = new Dictionary<string, string[]>();
    }
}
