// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : CurrentParticipantModel.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Common.Models {
    using System;

    public readonly struct CurrentParticipantModel : IEquatable<CurrentParticipantModel> {
        public int Id { get; }
        public string? Name { get; }
        public bool IsManager { get; }
        public bool IsAuthenticated => this.Id != 0;

        public CurrentParticipantModel(int id, string? name, bool isManager) {
            this.Id = id;
            this.Name = name;
            this.IsManager = isManager;
        }

        public bool Equals(CurrentParticipantModel other) => this.Id == other.Id;

        public override bool Equals(object? obj) => obj is CurrentParticipantModel other && this.Equals(other);

        public override int GetHashCode() => this.Id;

        public static bool operator ==(CurrentParticipantModel left, CurrentParticipantModel right) => left.Equals(right);

        public static bool operator !=(CurrentParticipantModel left, CurrentParticipantModel right) => !left.Equals(right);

        public void Deconstruct(out int participantId, out string? name, out bool isManager) {
            participantId = this.Id;
            name = this.Name;
            isManager = this.IsManager;
        }
    }
}
