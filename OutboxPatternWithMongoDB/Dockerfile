FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["OutboxPatternWithMongoDB/OutboxPatternWithMongoDB.csproj", "OutboxPatternWithMongoDB/"]
RUN dotnet restore "OutboxPatternWithMongoDB/OutboxPatternWithMongoDB.csproj"
COPY . .
WORKDIR "/src/OutboxPatternWithMongoDB"
RUN dotnet build "OutboxPatternWithMongoDB.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "OutboxPatternWithMongoDB.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OutboxPatternWithMongoDB.dll"]
