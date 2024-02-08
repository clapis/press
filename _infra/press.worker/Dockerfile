FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build-env
ARG StartupProject

WORKDIR /build
COPY . .
RUN dotnet publish ./src/${StartupProject}/${StartupProject}.csproj -c Release -o publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:7.0-alpine
ARG StartupProject

ENV StartupProject=${StartupProject}
WORKDIR /app
COPY --from=build-env /build/publish .

ENTRYPOINT dotnet ${StartupProject}.dll