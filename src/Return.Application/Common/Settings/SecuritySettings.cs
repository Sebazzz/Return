// ******************************************************************************
//  © 2020 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : SecuritySettings.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Common.Settings;

using System;

public sealed class SecuritySettings {
    /// <summary>
    /// Passphrase for creating lobby's - set on server side to prevent just anyone from hosting retrospectives
    /// </summary>
    public string? LobbyCreationPassphrase { get; set; }

    public bool LobbyCreationNeedsPassphrase => !String.IsNullOrEmpty(this.LobbyCreationPassphrase);

    /// <summary>
    /// Enable detection of X-HTTP-Forwarded-For HTTP headers
    /// </summary>
    public bool EnableProxyMode { get; set; }
}
