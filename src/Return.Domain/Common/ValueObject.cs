// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ValueObject.cs
//  Project         : Return.Domain
// ******************************************************************************

namespace Return.Domain.Common
{
    using System.Collections.Generic;
    using System.Linq;

    // Source: https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/microservice-ddd-cqrs-patterns/implement-value-objects
    public abstract class ValueObject
    {
        protected static bool EqualOperator(ValueObject? left, ValueObject? right)
        {
            if (left is null ^ right is null)
            {
                return false;
            }

            return left?.Equals(obj: right) != false;
        }

        protected static bool NotEqualOperator(ValueObject left, ValueObject right) =>
            !EqualOperator(left: left, right: right);

        protected abstract IEnumerable<object> GetAtomicValues();

        public override bool Equals(object? obj) {
            if (obj == null || obj.GetType() != this.GetType()) {
                return false;
            }

            var other = (ValueObject)obj;
            using IEnumerator<object> thisValues = this.GetAtomicValues().GetEnumerator();
            using IEnumerator<object> otherValues = other.GetAtomicValues().GetEnumerator();

            while (thisValues.MoveNext() && otherValues.MoveNext()) {
                if (thisValues.Current is null ^ otherValues.Current is null) {
                    return false;
                }

                if (thisValues.Current != null &&
                    !thisValues.Current.Equals(obj: otherValues.Current)) {
                    return false;
                }
            }

            return !thisValues.MoveNext() && !otherValues.MoveNext();
        }

        public override int GetHashCode() =>
            this.GetAtomicValues().
                Select(selector: x => x != null ? x.GetHashCode() : 0).
                Aggregate(func: (x, y) => x ^ y);
    }
}
