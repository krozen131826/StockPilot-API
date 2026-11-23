FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY Stockpilot.Domain/Stockpilot.Domain.csproj Stockpilot.Domain/
COPY Stockpilot.Application/Stockpilot.Application.csproj Stockpilot.Application/
COPY Stockpilot.Identity/Stockpilot.Identity.csproj Stockpilot.Identity/
COPY Stockpilot.Persistence/Stockpilot.Persistence.csproj Stockpilot.Persistence/
COPY Stockpilot.Api/Stockpilot.Api.csproj Stockpilot.Api/

RUN dotnet restore Stockpilot.Api/Stockpilot.Api.csproj

COPY . .

RUN dotnet publish Stockpilot.Api/Stockpilot.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "Stockpilot.Api.dll"]