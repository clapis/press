FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS build
ARG TARGETARCH
WORKDIR /source
COPY . .
RUN dotnet publish "./src/Press.Hosts.WebAPI/Press.Hosts.WebAPI.csproj" \
    -a $TARGETARCH \
    -c Release \
    -p:DebugType=None -p:DebugSymbols=false \
    -p:PublishSingleFile=true \
    -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy-chiseled-composite
EXPOSE 8080
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT [ "./Press.Hosts.WebAPI" ]