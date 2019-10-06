// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : MachineDateTime.cs
//  Project         : Return.Infrastructure
// ******************************************************************************

namespace Return.Infrastructure {
    using System;
    using Common;

    public sealed class MachineSystemClock : ISystemClock {
        public DateTime Now => DateTime.Now;
        public DateTimeOffset CurrentTimeOffset => DateTimeOffset.Now;
    }
}
