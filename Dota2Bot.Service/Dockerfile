#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Dota2Bot.Service/Dota2Bot.Service.csproj", "Dota2Bot.Service/"]
COPY ["Dota2Bot.Core/Dota2Bot.Core.csproj", "Dota2Bot.Core/"]
COPY ["Dota2Bot.Domain/Dota2Bot.Domain.csproj", "Dota2Bot.Domain/"]
RUN dotnet restore "Dota2Bot.Service/Dota2Bot.Service.csproj"
COPY . .
WORKDIR "/src/Dota2Bot.Service"
RUN dotnet build "Dota2Bot.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Dota2Bot.Service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Dota2Bot.Service.dll"]