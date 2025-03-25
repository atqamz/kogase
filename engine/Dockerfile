FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Kogase.Engine/Kogase.Engine.csproj", "Kogase.Engine/"]
RUN dotnet restore "Kogase.Engine/Kogase.Engine.csproj"
COPY . .
WORKDIR "/src/Kogase.Engine"
RUN dotnet build "Kogase.Engine.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Kogase.Engine.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create a script to check database connection
RUN echo '#!/bin/sh\n\
echo "Waiting for PostgreSQL to be ready..."\n\
while ! nc -z postgres 5432; do\n\
  echo "PostgreSQL is unavailable - sleeping"\n\
  sleep 1\n\
done\n\
echo "PostgreSQL is up - starting app"\n\
exec dotnet Kogase.Engine.dll\n' > /app/entrypoint.sh \
    && chmod +x /app/entrypoint.sh

# Add netcat for the connection check
RUN apt-get update && apt-get install -y netcat-openbsd && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["/app/entrypoint.sh"] 