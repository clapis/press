FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build-env
ARG TARGETARCH
ARG StartupProject

WORKDIR /build
COPY . .
RUN dotnet publish ./src/${StartupProject}/${StartupProject}.csproj -a $TARGETARCH -c Release -o publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine
ARG StartupProject

ENV \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    LC_ALL=en_US.UTF-8 \
    LANG=en_US.UTF-8

RUN apk add --no-cache \
    icu-data-full \
    icu-libs

WORKDIR /app
COPY --from=build-env /build/publish .
ENV StartupProject=${StartupProject}
ENTRYPOINT dotnet ${StartupProject}.dll