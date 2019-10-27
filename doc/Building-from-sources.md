# Building Return from sources

If you like to build Return from sources, you can follow the instructions below.

## Prequisites

-   [.NET Core 3.0 SDK](https://www.microsoft.com/net/download/core).
-   [Node.js LTS](https://nodejs.org/en/download/) or higher (Node.js 10.x is supported too).
-   [Yarn](https://yarnpkg.com/en/docs/install) or higher

### Additional prequisites on Windows

-   Powershell 5.x

### Additional prequisites on Linux (Ubuntu)

Environment:

-   Ensure `yarn` and `node` are in your `PATH`.
-   Ensure `dotnet` is in your `PATH`.

For running the build script:

-   Ensure the Powershell execution policy is set to [**RemoteSigned**](https://technet.microsoft.com/en-us/library/ee176961.aspx).

## Check-out

Pull the sources from this repository's [home page](https://github.com/Sebazzz/Return).

## Building

Use the build script in the root to build the application:

    build

To create a deployment to one of the supported platforms:

    build -Target Publish

The results will be emitted in the `build/publish` folder. For additional supported command line parameters run:

    build -h

## Development

After you've build the application once you can start developing.

To develop, just run the application using `dotnet run`.

If you have not created a database yet, please run `build -Target Generate-MigrationScript` to generate a migration script and run it on a local database. The application will also attempt to seed the database with some base data.

## Code style and linting

Code style and linting of TS/JS/JSON is enforced via TSLint and Prettier. If you have run `yarn`, prettier will be run as a pre-commit hook.

### Editors

Both Visual Studio and Visual Studio Code work well with the project.

Recommended extensions for Visual Studio:

-   ReSharper
-   TSLint
-   [Prettier](https://github.com/madskristensen/JavaScriptPrettier)

Recommende extensions for Visual Studio Code:

-   Editor support
    -   csharp
-   Code formatting and linting
    -   tslint
    -   vscode-prettier
    -   vscode-status-bar-format-toggle
