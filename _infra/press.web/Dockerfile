FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build-env
ARG StartupProject

WORKDIR /build
COPY . .
RUN dotnet publish ./src/${StartupProject}/${StartupProject}.csproj -c Release -o publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine
ARG StartupProject

ENV StartupProject=${StartupProject}
WORKDIR /app
COPY --from=build-env /build/publish .

RUN apk add --no-cache icu-libs
RUN apk add --no-cache icu-data-full
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
RUN apk add tzdata

ENTRYPOINT dotnet ${StartupProject}.dll