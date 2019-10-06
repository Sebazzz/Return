// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : HttpsServerOptions.cs
//  Project         : Return.Web
// ******************************************************************************

namespace Return.Web.Configuration {
    public sealed class HttpsServerOptions {
        public string? CertificatePath { get; set; }
        public string? CertificatePassword { get; set; }

        public bool EnableRedirect { get; set; }

        public bool UseStrongHttps { get; set; }
    }
}
