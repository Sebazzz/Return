// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : HttpsServerOptions.cs
//  Project         : Return.Web
// ******************************************************************************

namespace Return.Web.Configuration;

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage] // No use including this in coverage in integration tests
public sealed class HttpsServerOptions {
    public string? CertificatePath { get; set; }
    public string? CertificatePassword { get; set; }

    public bool EnableRedirect { get; set; }

    public bool UseStrongHttps { get; set; }
}
