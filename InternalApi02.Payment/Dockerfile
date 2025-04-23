#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["InternalApi02.Payment/InternalApi02.Payment.csproj", "InternalApi02.Payment/"]
COPY ["Observability.IoC/Observability.IoC.csproj", "Observability.IoC/"]
RUN dotnet restore "./InternalApi02.Payment/InternalApi02.Payment.csproj"
COPY . .
WORKDIR "/src/InternalApi02.Payment"
RUN dotnet build "./InternalApi02.Payment.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./InternalApi02.Payment.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InternalApi02.Payment.dll"]