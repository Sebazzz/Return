### BUILD
FROM mcr.microsoft.com/dotnet/core/sdk:3.1.201 AS build-env
WORKDIR /source

# Prerequisites
COPY utils/* utils/
RUN utils/install-all-prereqs.sh

# ... libgit2sharp debian 10.x support
RUN dotnet tool install -g GitVersion.Tool --version 5.2.4
ENV LD_LIBRARY_PATH=/root/.dotnet/tools/.store/gitversion.tool/5.2.4/gitversion.tool/5.2.4/tools/netcoreapp3.1/any/runtimes/debian.9-x64/native/

# Copy csproj and restore as distinct layers
# ... sources
COPY src/Return.Application/*.csproj src/Return.Application/
COPY src/Return.Common/*.csproj src/Return.Common/
COPY src/Return.Domain/*.csproj src/Return.Domain/
COPY src/Return.Infrastructure/*.csproj src/Return.Infrastructure/
COPY src/Return.Persistence/*.csproj src/Return.Persistence/
COPY src/Return.Web/*.csproj src/Return.Web/
COPY src/Common.props src/

# ... tests
COPY tests/Return.Application.Tests.Unit/*.csproj tests/Return.Application.Tests.Unit/
COPY tests/Return.Domain.Tests.Unit/*.csproj tests/Return.Domain.Tests.Unit/
COPY tests/Return.Web.Tests.Unit/*.csproj tests/Return.Web.Tests.Unit/
COPY tests/Return.Web.Tests.Integration/*.csproj tests/Return.Web.Tests.Integration/
COPY tests/Common.props tests/

COPY *.sln .
COPY dotnet-tools.json .
RUN dotnet restore
RUN dotnet tool restore

# Yarn (although it isn't as large, still worth caching)
COPY src/Return.Web/package.json src/Return.Web/
COPY src/Return.Web/yarn.lock src/Return.Web/
RUN yarn --cwd src/Return.Web/

## Skip build script pre-warm
## This causes later invocations of the build script to fail with "Failed to uninstall tool package 'cake.tool': Invalid cross-device link"
#COPY build.* .
#RUN ./build.sh --target=restore-node-packages

### TEST
FROM build-env AS test

# ... run tests
COPY . .
ENV RETURN_TEST_WAIT_TIME 30
ENV SCREENSHOT_TEST_FAILURE_TOLERANCE True
RUN ./build.sh --target=test

### PUBLISHING
FROM build-env AS publish

# ... run publish
COPY . .
RUN ./build.sh --target=Publish-Ubuntu-18.04-x64 --publish-dir=publish --verbosity=verbose --skip-compression=true

### RUNTIME IMAGE
FROM mcr.microsoft.com/dotnet/core/runtime-deps:3.1
WORKDIR /app

# ... Run libgdi install
COPY utils/install-app-prereqs.sh utils/
RUN bash utils/install-app-prereqs.sh

# ... Copy published app
COPY --from=publish /source/publish/ubuntu.18.04-x64/ .

ENV ASPNETCORE_ENVIRONMENT Production

# Config directory
VOLUME ["/etc/return-retro"]

# Set some defaults for a "direct run" experience
ENV DATABASE__DATABASE "/app/data.db"
ENV DATABASE__DATABASEPROVIDER Sqlite

# ... enable proxy mode
ENV SECURITY__ENABLEPROXYMODE True

# ... health check
HEALTHCHECK CMD curl --fail http://localhost/health || exit

ENTRYPOINT [ "./launch", "run" ]