# Return installation guide

This guide will help you in installation of the application. The application is a ASP.NET Core web application and will in essence only require a SQL Server database server to get up and running.

**Note:** This installation guide covers very basic installation, just enough to get the application up and running. It does _not_ cover installation of the application as a systemd or Windows Service, nor setting it up behind any reverse proxy. Please refer to [hosting as a Windows service](https://docs.microsoft.com/nl-nl/aspnet/core/hosting/windows-service), [hosting in Windows IIS](https://docs.microsoft.com/nl-nl/aspnet/core/publishing/iis?tabs=aspnetcore2x) or [hosting on Linux](https://docs.microsoft.com/nl-nl/aspnet/core/publishing/linuxproduction?tabs=aspnetcore2x) pages on the official Microsoft docs for more information.

**Note:** Return also features experimental SQLite support.

## Manual installation

### Using Docker

PokerTime is available as a docker image. Simply pull it from the Docker hub, and run it:

    docker pull sebazzz/return:latest
    docker run -p 80:80 sebazzz/return

For further configuration you may want to mount a directory with [the configuration](#Configuration):

    docker run -p 80:80 -v /path/to/my/configuration/directory:/etc/return-retro sebazzz/return

### Getting a release

Download a release from the [releases](https://github.com/Sebazzz/Return/releases) tab. You may also [build the application from sources](Building-from-sources.md) if you like or get a build from AppVeyor.

### Prequisites

To run the application you'Il need:

-   A virtual machine or Azure website to deploy it to.
    -   If using Windows, install on Windows Server 2016 or higher.
    -   If using Linux, install on Ubuntu 16.x or higher.
-   Microsoft SQL Server for the database. The free SQL Server Express also works.
-   E-mail (SMTP) server if you want account recovery e-mails etc. to work

On Ubuntu install:

    sudo apt-get install libunwind8 liblttng-ust0 libcurl3 libssl1.0.0 libuuid1 libkrb5-3 zlib1g

In addition, for Ubuntu 16.x:

    sudo apt-get install libicu55 libgdiplus

For Ubuntu 18.x:

    sudo apt-get install libicu57 libgdiplus

### Installation

You can configure the application via environment variables or configuration files.

Settings are grouped per section, and consist of key-value pairs. In this documentation the section name is shown in the title of each configuration.

### Environment variables

Environment variables are more easier to configure, but usually also more insecure. The configurations shown below are environment variables (you can set them in bash by `export NAME=VALUE` and in Powershell via `$ENV:NAME = "VALUE"`).

Section parts and key-value pairs are concatenated by using two underscores. For instance with section `Server.Https` and setting `CertificatePath` becomes: `SERVER__HTTPS__CERTIFICATEPATH`.

### File-based configuration

Configuration files are searched on platform-specific paths:

-   Windows
    -   Common application data (usually `C:\ProgramData\return-retro\config.<extension>`)
-   Unix / Linux - excluding MacOS
    -   `/etc/return-retro/config.<extension>`

You can use either `.json` or `.ini` files to configure the application. Using both is not recommended.

#### INI files

INI files groups key-value pairs of the sections with a `[section:subsection]` header. Key-value pairs are simply `key=value`. It is probably the most human-editable file format.

For instance with section `Server.Https` and setting `CertificatePath` becomes:

    [Server:Https]
    CertificatePath=path

#### JSON files

JSON files follow a standard JSON file format. Each section is an nested object.

For instance with section `Server.Https` and setting `CertificatePath` becomes:

    {
       "Server": {
          "Https": {
              "CertificatePath": "path"
          }
       }
    }

You can add a `appsettings.Production.json` file to keep your own settings.

### General configuration - `Server`

`BaseUrl`: Base URL used for mailing. If not set, auto-detection is attempted.

### HTTPS configuration - `Server.Https`

To use HTTPs, use the following environment variables:

`CertificatePath`: Path to pfx file.

`CertificatePassword`: Password for pfx file.

The server will automatically start on port 80 and 443.

### Logging configuration - `Logging.File`

To configure logging to a file:

`Path`: Path to log file.

`FileSizeLimitBytes`: Maximum size of log file in bytes. 0 for unlimited.

`MaxRollingFiles`: Maximum file rollover. 0 for unlimited.

### Security settings

`LobbyCreationPassphrase`: Passphrase to create a lobby. Prevents anyone without this passphrase from creating retrospectives.

`EnableProxyMode`: Whether to detect (`True`) or ignore (`False`) headers sent by reverse proxies.

### Database set-up - `Database`

Create an new empty database with a case insensitive collation (`SQL_Latin1_General_CP1_CI_AS` is preferred).

You can set the database settings as follows:

`DatabaseProvider`: Either SqlServer or Sqlite. Sqlite support is experimental.

#### Sqlite configuration options

`Database`: Database file path.

**In-memory database:** For testing purposes you can add the following key/value:

`ConnectionString`: `Mode=Memory;Cache=Shared`

This will create an in-memory database.

##### Advanced configuration

Set the connection string using:

`ConnectionString`: Connection string used to connection to the database. Usually: `Data Source=file`.

Options in the connection string will override manual "simple" configured options above.

#### SQL Server configuration options

`Server`: Server name and instance.

`Database`: Database name.

`UserID`: User ID.

`Password`: Password.

`IntegratedSecurity`: Use integrated credentials. Cannot be combined with user id / password.

`Encrypt`: Database connection encryption enabled.

`ConnectionTimeout`: Connection timeout to SQL Server. Set to 0 for unlimited. Set to 60 seconds for cloud environments.

##### Advanced configuration

Set the connection string using:

`ConnectionString`: Connection string used to connection to the database. Usually: `Server=myserver;Integrated Security=true;Database=mydatabase;MultipleActiveResultSets=true`.

Options in the connection string will override manual "simple" configured options above.

## Application installation

Unpack the application on any location, for instance `/opt/return-retro`.

Modify the connection string in `launch.conf`.

You can try out the application using:

    ./launch run

Install the application as a systemd service using:

    ./launch install

View other options:

    ./launch --help

## Run

To run the application after installation, simply run:

    ./launch start

The application will launch at the URL specified in `launch.conf`.

The application will automatically create the database if allowed by its permissions.
