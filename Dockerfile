# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY RpgGame.sln .
COPY src/RpgGame.Core/RpgGame.Core.csproj src/RpgGame.Core/
COPY src/RpgGame/RpgGame.csproj src/RpgGame/
COPY tests/RpgGame.Core.Tests/RpgGame.Core.Tests.csproj tests/RpgGame.Core.Tests/
RUN dotnet restore RpgGame.sln

COPY . .
RUN dotnet publish src/RpgGame/RpgGame.csproj -c Release -o /app/publish --no-restore

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app

# Save file persists outside the container via a mounted volume at /data.
ENV RPG_DB_PATH=/data/rpg.db
VOLUME /data

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "RpgGame.dll"]
