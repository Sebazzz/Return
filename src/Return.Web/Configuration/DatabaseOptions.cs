// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : DatabaseOptions.cs
//  Project         : Return.Web
// ******************************************************************************

namespace Return.Web.Configuration {
    using System;
    using Microsoft.Data.SqlClient;
    using Microsoft.Data.Sqlite;
    using Persistence;

    public class DatabaseOptions : IDatabaseOptions {
        private string? _cachedConnectionString;

        public string? Server { get; set; }
        public string? Database { get; set; }
        public string? UserId { get; set; }
        public string? Password { get; set; }
        public bool? Encrypt { get; set; }
        public bool? IntegratedSecurity { get; set; }
        public int? ConnectionTimeout { get; set; }
        public string? ConnectionString { get; set; }
        public DatabaseProvider DatabaseProvider { get; set; }

        public string CreateConnectionString() {
            if (this._cachedConnectionString != null) {
                return this._cachedConnectionString;
            }

            // Create new conn string
            switch (this.DatabaseProvider) {
                case DatabaseProvider.SqlServer:
                    return this.CreateSqlServerConnectionString();
                case DatabaseProvider.Sqlite:
                    return this.CreateSqliteConnectionString();
                default:
                    throw new InvalidOperationException($"Invalid database provider: {this.DatabaseProvider}");
            }
        }

        private string CreateSqliteConnectionString() {
            var connStringBuilder = new SqliteConnectionStringBuilder();

            // Set values current connection string
            if (this.Database != null) connStringBuilder.DataSource = this.Database;
            connStringBuilder.ForeignKeys = true;
            connStringBuilder.Mode = SqliteOpenMode.ReadWriteCreate;
            connStringBuilder.Cache = SqliteCacheMode.Private;

            // Copy current connection string, overriding options here
            if (!String.IsNullOrEmpty(value: this.ConnectionString)) {
                var srcConnStringBuilder = new SqliteConnectionStringBuilder(connectionString: this.ConnectionString);
                foreach (string? key in srcConnStringBuilder.Keys ??
                                        throw new InvalidOperationException(message: "Invalid connection string")) {
                    if (key != null && !String.IsNullOrEmpty(srcConnStringBuilder[key]?.ToString())) {
                        connStringBuilder[key] = srcConnStringBuilder[key];
                    }
                }
            }

            return connStringBuilder.ToString();
        }

        private string CreateSqlServerConnectionString() {
            var connStringBuilder = new SqlConnectionStringBuilder();

            // Set values current connection string
            if (this.ConnectionTimeout != null) connStringBuilder.ConnectTimeout = this.ConnectionTimeout.Value;
            if (this.Encrypt != null) connStringBuilder.Encrypt = this.Encrypt.Value;
            if (this.IntegratedSecurity != null) connStringBuilder.IntegratedSecurity = this.IntegratedSecurity.Value;
            if (!String.IsNullOrEmpty(value: this.UserId)) connStringBuilder.UserID = this.UserId;
            if (!String.IsNullOrEmpty(value: this.Password)) connStringBuilder.Password = this.Password;
            if (!String.IsNullOrEmpty(value: this.Server)) connStringBuilder.DataSource = this.Server;
            if (!String.IsNullOrEmpty(value: this.Database)) connStringBuilder.InitialCatalog = this.Database;

            // Copy current connection string, overriding options here
            if (!String.IsNullOrEmpty(value: this.ConnectionString)) {
                var srcConnStringBuilder = new SqlConnectionStringBuilder(connectionString: this.ConnectionString);
                foreach (string? key in srcConnStringBuilder.Keys ??
                                        throw new InvalidOperationException(message: "Invalid connection string")) {
                    if (key != null) {
                        connStringBuilder[keyword: key] = srcConnStringBuilder[keyword: key];
                    }
                }
            }

            // Ensure MultipleActiveResultSets
            connStringBuilder.MultipleActiveResultSets = true;

            // Cache and return
            // (thread safety notice: assignment is atomic)
            return this._cachedConnectionString = connStringBuilder.ToString();
        }
    }
}
