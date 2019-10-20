// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : Name.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Common {
    using System;
    using Return.Common;

    public static class Name {
        public static string Create() => Guid.NewGuid().ToString("N", Culture.Invariant);
    }
}
