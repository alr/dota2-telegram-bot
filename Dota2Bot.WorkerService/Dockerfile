#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Dota2Bot.WorkerService/Dota2Bot.WorkerService.csproj", "Dota2Bot.WorkerService/"]
COPY ["Dota2Bot.Domain/Dota2Bot.Domain.csproj", "Dota2Bot.Domain/"]
COPY ["Dota2Bot.Core/Dota2Bot.Core.csproj", "Dota2Bot.Core/"]
RUN dotnet restore "Dota2Bot.WorkerService/Dota2Bot.WorkerService.csproj"
COPY . .
WORKDIR "/src/Dota2Bot.WorkerService"
RUN dotnet build "Dota2Bot.WorkerService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Dota2Bot.WorkerService.csproj" -c Release -o /app/publish
# waitfor
WORKDIR /app/tools
RUN git clone https://github.com/vishnubob/wait-for-it.git

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=publish /app/tools/wait-for-it/wait-for-it.sh ./wait-for-it.sh
ENTRYPOINT ["dotnet", "Dota2Bot.WorkerService.dll"]