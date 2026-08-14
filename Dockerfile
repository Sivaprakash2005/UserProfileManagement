# Multi-stage Dockerfile for ASP.NET Core 9.0 UserProfileManagement

# Stage 1: Base Runtime Environment
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Stage 2: Build & Compilation Environment
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["UserProfileManagement.csproj", "./"]
RUN dotnet restore "UserProfileManagement.csproj"

# Copy remaining files and compile Release build
COPY . .
WORKDIR "/src"
RUN dotnet build "UserProfileManagement.csproj" -c Release -o /app/build

# Stage 3: Publish Application
FROM build AS publish
RUN dotnet publish "UserProfileManagement.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Production Container Instance
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserProfileManagement.dll"]
