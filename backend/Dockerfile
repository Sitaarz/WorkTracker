# ---- Stage 1: build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
    WORKDIR /build

    COPY ./src/WorkTracker.API/WorkTracker.API.csproj ./src/WorkTracker.API/
    COPY ./src/WorkTracker.Application/WorkTracker.Application.csproj ./src/WorkTracker.Application/
    COPY ./src/WorkTracker.Infrastructure/WorkTracker.Infrastructure.csproj ./src/WorkTracker.Infrastructure/
    COPY ./src/WorkTracker.Domain/WorkTracker.Domain.csproj ./src/WorkTracker.Domain/

    RUN dotnet restore ./src/WorkTracker.API/WorkTracker.API.csproj

    COPY ./src/ ./src/
    RUN dotnet publish ./src/WorkTracker.API/WorkTracker.API.csproj -c Release -o ./publish --no-restore

# ---- Stage 2: runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
    WORKDIR /app
    ENV HTTP_PORTS=8080

    # curl is used by the docker-compose healthcheck; the base image ships neither curl nor wget.
    RUN apt-get update \
        && apt-get install -y --no-install-recommends curl \
        && rm -rf /var/lib/apt/lists/*

    USER $APP_UID

    COPY --from=build /build/publish/ ./
    EXPOSE 8080
    ENTRYPOINT [ "dotnet", "WorkTracker.API.dll"]