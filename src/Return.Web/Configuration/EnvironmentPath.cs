// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : EnvironmentPath.cs
//  Project         : Return.Web
// ******************************************************************************

namespace Return.Web.Configuration {
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    public static class EnvironmentPath {
        private const string AppSpecificFolder = "return-retro";

        public static string CreatePath(string subPath) {
            if (RuntimeInformation.IsOSPlatform(osPlatform: OSPlatform.Windows)) {
                return MakeWin32FilePath(subPath: subPath);
            }

            if (RuntimeInformation.IsOSPlatform(osPlatform: OSPlatform.Linux)) {
                return MakeUnixFilePath(subPath: subPath);
            }

            throw new InvalidOperationException($"Unsupported platform family '{RuntimeInformation.OSDescription}'");
        }

        private static string MakeWin32FilePath(string subPath) =>
            Path.Combine(
                Environment.GetFolderPath(folder: Environment.SpecialFolder.CommonApplicationData),
                path2: AppSpecificFolder,
                path3: subPath
            );

        private static string MakeUnixFilePath(string subPath) =>
            Path.Combine(
                path1: "/etc",
                path2: AppSpecificFolder,
                path3: subPath
            );
    }
}
