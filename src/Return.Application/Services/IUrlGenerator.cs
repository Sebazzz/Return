// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : IUrlGenerator.cs
//  Project         : Return.Application
// ******************************************************************************

namespace Return.Application.Services {
    using System;
    using Domain.ValueObjects;

    public interface IUrlGenerator {
        Uri GenerateUrlToRetrospectiveLobby(RetroIdentifier urlId);
    }
}
