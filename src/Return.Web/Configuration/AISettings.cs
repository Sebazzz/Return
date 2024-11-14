// ******************************************************************************
//  © 2024 Sebastiaan Dammann | damsteen.nl
//
//  File:           : AISettings.cs
//  Project         : Return.Web
// ******************************************************************************

namespace Return.Web.Configuration;

using System.ComponentModel.DataAnnotations;

public class AISettings
{
    [Required]
    public required string Url { get; set; }

    /// <summary>
    /// https://ollama.com/search?c=tools
    /// </summary>
    public string? Model { get; set; }
}
