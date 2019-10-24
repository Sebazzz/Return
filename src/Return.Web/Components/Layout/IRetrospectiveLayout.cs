// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : IRetrospectiveLayout.cs
//  Project         : Return.Web
// ******************************************************************************

namespace Return.Web.Components.Layout {
    using System;
    using Domain.Entities;

    public interface IRetrospectiveLayout {
        void Update(in RetrospectiveLayoutInfo layoutInfo);
    }

    public readonly struct RetrospectiveLayoutInfo : IEquatable<RetrospectiveLayoutInfo> {
        public RetrospectiveStage? Stage { get; }

        public string Title { get; }

        public RetrospectiveLayoutInfo(string title) : this() {
            this.Title = title;
        }

        public RetrospectiveLayoutInfo(string title, RetrospectiveStage? stage) {
            this.Stage = stage;
            this.Title = title;
        }

        public bool Equals(RetrospectiveLayoutInfo other) => this.Stage == other.Stage && this.Title == other.Title;

        public override bool Equals(object? obj) => obj is RetrospectiveLayoutInfo other && this.Equals(other);

        public override int GetHashCode() {
            unchecked {
                return (this.Stage.GetHashCode() * 397) ^ (this.Title != null ? this.Title.GetHashCode(StringComparison.InvariantCulture) : 0);
            }
        }

        public static bool operator ==(RetrospectiveLayoutInfo left, RetrospectiveLayoutInfo right) => left.Equals(right);

        public static bool operator !=(RetrospectiveLayoutInfo left, RetrospectiveLayoutInfo right) => !left.Equals(right);
    }
}
