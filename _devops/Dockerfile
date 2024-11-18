FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS build
ARG TARGETARCH
WORKDIR /source

# copy csproj and restore as distinct layers
COPY ./*.sln /*.props ./

COPY src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p src/${file%.*}/ && mv $file src/${file%.*}/; done
COPY test/unit/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p test/unit/${file%.*}/ && mv $file test/unit/${file%.*}/; done
COPY test/component/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p test/component/${file%.*}/ && mv $file test/component/${file%.*}/; done
COPY perf/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p perf/${file%.*}/ && mv $file perf/${file%.*}/; done

RUN dotnet restore -a $TARGETARCH

# copy everything else and build app
COPY . .
RUN dotnet publish "./src/Press.Hosts.WebAPI/Press.Hosts.WebAPI.csproj" \
    -a $TARGETARCH \
    -c Release \
    -p:DebugType=None -p:DebugSymbols=false \
    # -p:PublishSingleFile=true \ https://github.com/dotnet/sdk/issues/37367
    --no-restore \
    -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy-chiseled-composite
EXPOSE 8080
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT [ "./Press.Hosts.WebAPI" ]