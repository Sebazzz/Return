# ![Icon](doc/logo.png) Return

Retrospective tool built in ASP.NET Core and Blazor

Licensed: GNU GPL v3.0

|                    | master                                                                                                                                                                                                            | develop                                                                                                                                                                                                             |
| ------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **AppVeyor CI**    | [![Build status](https://ci.appveyor.com/api/projects/status/7bjrmgtek7j080d7/branch/master?svg=true)](https://ci.appveyor.com/project/Sebazzz/Return/branch/master)                                              | [![Build status](https://ci.appveyor.com/api/projects/status/7bjrmgtek7j080d7/branch/develop?svg=true)](https://ci.appveyor.com/project/Sebazzz/Return/branch/develop)                                              |
| **CircleCI**       | [![CircleCI](https://circleci.com/gh/Sebazzz/Return/tree/master.svg?style=shield)](https://circleci.com/gh/Sebazzz/Return/tree/master)                                                                            | [![CircleCI](https://circleci.com/gh/Sebazzz/Return/tree/develop.svg?style=shield)](https://circleci.com/gh/Sebazzz/Return/tree/develop)                                                                            |
| **Github actions** | [![Github CI](https://github.com/sebazzz/Return/workflows/Continuous%20integration/badge.svg?branch=master)](https://github.com/Sebazzz/Return/actions?workflow=Continuous+integration&branch=master)             | [![Github CI](https://github.com/sebazzz/Return/workflows/Continuous%20integration/badge.svg?branch=develop)](https://github.com/Sebazzz/Return/actions?workflow=Continuous+integration&branch=develop)             |
| **Codeconv**       | [![codecov](https://codecov.io/gh/Sebazzz/Return/branch/master/graph/badge.svg)](https://codecov.io/gh/Sebazzz/Return)                                                                                            | [![codecov](https://codecov.io/gh/Sebazzz/Return/branch/develop/graph/badge.svg)](https://codecov.io/gh/Sebazzz/Return)                                                                                             |
| **Daily build**    | [![Github CI](https://github.com/sebazzz/Return/workflows/Build%20installation%20packages/badge.svg?branch=master)](https://github.com/Sebazzz/Return/actions?workflow=Build+installation+packages&branch=master) | [![Github CI](https://github.com/sebazzz/Return/workflows/Build%20installation%20packages/badge.svg?branch=develop)](https://github.com/Sebazzz/Return/actions?workflow=Build+installation+packages&branch=develop) |

## Features

-   Realtime retrospective app, ideal for remote teams
-   Shortcut support:
    -   Ctrl + lane number for adding notes or groups
    -   Ctrl + delete for deleting focused note
-   Create password protected retrospectives
-   As facilitator, lead the retrospective through the writing, grouping and voting phase.
-   Overview with highest voted items

### Browser Support

Developed and tested on:

-   Internet Explorer 11
-   Microsoft Edge
-   Google Chrome
-   Mozilla Firefox

## Download

### Docker

Return is available as a docker image. Simply pull it from the Docker hub, and run it:

    docker pull sebazzz/return:latest
    docker run -p 80:80 sebazzz/return

For further configuration you may want to mount a directory with [the configuration](doc/Installation.md#Configuration):

    docker run -p 80:80 -v /path/to/my/configuration/directory:/etc/return-retro sebazzz/return

### Manual installation

Download the release for your OS from the [releases tab](https://github.com/Sebazzz/Return/releases) or download the cutting edge builds from [AppVeyor](https://ci.appveyor.com/project/Sebazzz/Return).

[Follow the installation instructions](doc/Installation.md) in the documentation to install it.

## Building Return from sources

If you prefer to build the application yourself, please follow the [compilation instructions](doc/Building-from-sources.md) in the documentation.

## Screenshots

**Create a retrospective**

![Create retrospective](doc/create-retro.png)

**Joining a retrospective**

![Join retrospective](doc/join-retro.png)

**Writing down findings**

![Writing phase](doc/writing.png)

**Grouping**

![Grouping](doc/grouping.png)

**Voting on items**

![Voting](doc/voting.png)

**Finish and review**

![Review](doc/finish-1.png)
![Overview](doc/finish-2.png)

## Contributions

Contributions are allowed and encouraged. In general the rules are: same code style (simply use the included `.editorconfig`), and write automated tests for the changes.

Please submit an issue to communicate in advance to prevent disappointments.

## Attribution

Application icon:

-   Icon made by [Freepik](https://www.flaticon.com/free-icon/rethink_69507) from [www.flaticon.com](http://www.flaticon.com/)

Built on:

-   [Bulma](https://bulma.io) _CSS framework_;
-   [Fontawesome](http://fontawesome.io/) as _icon framework_;
-   [ASP.NET Core 3.1](https://dot.net) (Blazor Server) with [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) for _server side logic and data persistence_;
