# ![Icon](doc/logo.png) Return

Retrospective tool built in ASP.NET Core and Blazor

Licensed: GNU GPL v3.0

[![Build status](https://ci.appveyor.com/api/projects/status/7bjrmgtek7j080d7?svg=true)](https://ci.appveyor.com/project/Sebazzz/Return)
[![CircleCI](https://circleci.com/gh/Sebazzz/Return.svg?style=svg)](https://circleci.com/gh/Sebazzz/Return)
[![Github CI](https://github.com/sebazzz/Return/workflows/Continuous%20integration/badge.svg)](https://github.com/Sebazzz/Return/actions?workflow=Continuous+integration)

## Features

-   Realtime retrospective app, ideal for remote teams
-   Shortcut support:
    -   Ctrl + lane number for adding notes or groups
    -   Ctrl + delete for deleting focused note
-   Create password protected retrospectives
-   As facilitator, manage the retrospective through the writing, grouping and voting phase.
-   Overview with highest voted items

### Browser Support

Developed and tested on:

-   Internet Explorer 11
-   Microsoft Edge
-   Google Chrome
-   Mozilla Firefox

## Download

Download the release for your OS from the [releases tab](https://github.com/Sebazzz/Return/releases) or download the cutting edge builds from [AppVeyor](https://ci.appveyor.com/project/Sebazzz/Return).

[Follow the installation instructions](docs/Installation.md) in the documentation to install it.

## Building Return from sources

If you prefer to build the application yourself, please follow the [compilation instructions](docs/Building-from-sources.md) in the documentation.

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
-   [ASP.NET Core 3.0](https://dot.net) (Blazor Server) with [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) for _server side logic and data persistence_;
