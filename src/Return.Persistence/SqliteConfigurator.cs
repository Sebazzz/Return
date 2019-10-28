namespace Return.Persistence {
    using Microsoft.Data.Sqlite;
    using Microsoft.EntityFrameworkCore;

    internal static class SqliteConfigurator {
        private static SqliteConnection? InMemoryConnection;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "EF will manage lifetime")]
        public static void ConfigureDbContext(
            DbContextOptionsBuilder optionsBuilder,
            IDatabaseOptions databaseOptions
        ) {
            string connString = databaseOptions.CreateConnectionString();

            if (IsInMemory(connString)) {
                // Create a static connection simply to keep the connection alive
                if (InMemoryConnection == null) {
                    InMemoryConnection = new SqliteConnection(connString);
                    InMemoryConnection.Open();
                }

                optionsBuilder.UseSqlite(connString);
            }
            else {
                // Create a connection ourselve because we run into timeouts when seeding
                // https://github.com/aspnet/EntityFrameworkCore/issues/18607

                var conn = new SqliteConnection(connString) {
                    DefaultTimeout = 180
                };

                optionsBuilder.UseSqlite(conn);
            }
        }

        private static bool IsInMemory(string connectionString) => new SqliteConnectionStringBuilder(connectionString).Mode == SqliteOpenMode.Memory;
    }
}
