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
ENTRYPOINT ["dotnet", "Kogase.Engine.dll"] 