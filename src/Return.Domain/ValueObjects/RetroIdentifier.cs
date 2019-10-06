// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : RetroIdentifier.cs
//  Project         : IFS.Web
// ******************************************************************************

namespace Return.Domain.ValueObjects {
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Common;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    public sealed class RetroIdentifier : ValueObject {
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
        public string StringId { get; set; }

        protected override IEnumerable<object> GetAtomicValues() => new[] { this.StringId };

        public RetroIdentifier() {
            // Needed for EF construction
        }

        public RetroIdentifier(string stringId) {
            this.StringId = stringId;
        }
    }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
}
