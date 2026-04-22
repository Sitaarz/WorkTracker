# ---- build stage ----
    FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
    WORKDIR /src
    # kopiujemy pliki projektowe, żeby użyć cache na restore
    COPY src/WorkTracker.API/WorkTracker.API.csproj src/WorkTracker.API/
    COPY src/WorkTracker.Application/WorkTracker.Application.csproj src/WorkTracker.Application/
    COPY src/WorkTracker.Domain/WorkTracker.Domain.csproj src/WorkTracker.Domain/
    COPY src/WorkTracker.Infrastructure/WorkTracker.Infrastructure.csproj src/WorkTracker.Infrastructure/
    RUN dotnet restore src/WorkTracker.API/WorkTracker.API.csproj
    # reszta kodu
    COPY src/ src/
    RUN dotnet publish src/WorkTracker.API/WorkTracker.API.csproj -c Release -o /app/publish /p:UseAppHost=false
    # ---- runtime stage ----
    FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
    WORKDIR /app
    COPY --from=build /app/publish .
    ENV ASPNETCORE_URLS=http://+:8080
    EXPOSE 8080
    ENTRYPOINT ["dotnet", "WorkTracker.API.dll"]