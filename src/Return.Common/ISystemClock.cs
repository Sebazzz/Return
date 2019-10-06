// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : ISystemClock.cs
//  Project         : Return.Common
// ******************************************************************************

namespace Return.Common
{
    using System;

    public interface ISystemClock
    {
        DateTime Now { get; }

        DateTimeOffset CurrentTimeOffset { get; }
    }
}
