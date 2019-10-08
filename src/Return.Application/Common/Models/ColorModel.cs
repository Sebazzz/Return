// ******************************************************************************
//  ©  Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ColorModel.cs
//  Project         : Return.Application
// ******************************************************************************
namespace Return.Application.Common.Models {
    using System;
    using AutoMapper;
    using Domain.ValueObjects;
    using Mapping;

    public class ColorModel : IMapFrom<ParticipantColor> {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public string HexString => $"{this.R:X2}{this.G:X2}{this.B:X2}";

        public virtual void Mapping(Profile profile) {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            profile.CreateMap<ParticipantColor, ColorModel>();
        }

        public bool HasSameColors(ColorModel other) {
            if (other == null) throw new ArgumentNullException(nameof(other));
            return (other.R, other.G, other.B) == (this.R, this.G, this.B);
        }
    }
}
