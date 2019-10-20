// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : Paths.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Common {
    using System;
    using NUnit.Framework;

    public static class Paths {
        public static string TestArtifactDir => Environment.GetEnvironmentVariable("TEST_ARTIFACT_DIR") ?? TestContext.CurrentContext?.TestDirectory ?? Environment.CurrentDirectory;
    }
}
