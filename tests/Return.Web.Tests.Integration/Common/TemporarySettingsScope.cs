// ******************************************************************************
//  © 2020 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : TemporarySettingsScope.cs
//  Project         : Return.Web.Tests.Integration
// ******************************************************************************

namespace Return.Web.Tests.Integration.Common;

using System;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;

/// <summary>
/// Allows within a, for instance, test fixture, to temporary change a setting
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class TemporarySettingsScope<T> where T : class, new() {
    private readonly ReturnAppFactory _app;
    private readonly IMapper _mapper;
    private T _savedSettings;

    public TemporarySettingsScope(ReturnAppFactory app) {
        this._app = app;

        var config =
            new MapperConfiguration(configure: cfg => { cfg.CreateMap<T, T>(); });

        this._mapper = config.CreateMapper();
    }

    public void SaveSettings(Action<T> callback) {
        IOptions<T> settingsAccessor = this._app.Services.GetRequiredService<IOptions<T>>();

        this._savedSettings = this._mapper.Map<T>(settingsAccessor.Value);

        TestContext.WriteLine($"Entering temporary settings scope for {typeof(T)}");

        callback.Invoke(settingsAccessor.Value);
    }

    public void RestoreSettings() {
        TestContext.WriteLine($"Exiting temporary settings scope for {typeof(T)}");

        if (this._savedSettings == null) {
            throw new InvalidOperationException($"{this.GetType().FullName}: Unable to restore settings, settings not set");
        }

        IOptions<T> settingsAccessor = this._app.Services.GetRequiredService<IOptions<T>>();
        this._mapper.Map(this._savedSettings, settingsAccessor.Value);

        this._savedSettings = null;
        TestContext.WriteLine($"Exited temporary settings scope for {typeof(T)}");
    }
}
