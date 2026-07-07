# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["PodPilot.slnx", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["stylecop.json", "./"]
COPY ["src/PodPilot.Domain/PodPilot.Domain.csproj", "src/PodPilot.Domain/"]
COPY ["src/PodPilot.Contracts/PodPilot.Contracts.csproj", "src/PodPilot.Contracts/"]
COPY ["src/PodPilot.Application/PodPilot.Application.csproj", "src/PodPilot.Application/"]
COPY ["src/PodPilot.Infrastructure/PodPilot.Infrastructure.csproj", "src/PodPilot.Infrastructure/"]
COPY ["src/PodPilot.Api/PodPilot.Api.csproj", "src/PodPilot.Api/"]

RUN dotnet restore "src/PodPilot.Api/PodPilot.Api.csproj"

COPY . .
WORKDIR /src/src/PodPilot.Api
RUN dotnet publish "PodPilot.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
RUN mkdir -p /app/logs

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "PodPilot.Api.dll"]
