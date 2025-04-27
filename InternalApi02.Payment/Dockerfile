#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
#
## Install OpenTelemetry Auto-Instrumentation
#ARG OTEL_VERSION=1.11.0
#ENV OTEL_DOTNET_AUTO_HOME=/otel-dotnet-auto
#ADD https://github.com/open-telemetry/opentelemetry-dotnet-instrumentation/releases/download/v${OTEL_VERSION}/otel-dotnet-auto-install.sh otel-dotnet-auto-install.sh
#RUN apt-get update && apt-get install -y unzip && \
    #sh otel-dotnet-auto-install.sh
#RUN chmod +x /otel-dotnet-auto/instrument.sh
    
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
#COPY --from=build /otel-dotnet-auto /home/app/otel-dotnet-auto
#RUN chmod +x /app/InternalApi02.Payment.dll
#RUN chmod +x /home/app/otel-dotnet-auto/instrument.sh
#ENV OTEL_DOTNET_AUTO_HOME=/home/app/otel-dotnet-auto

ENTRYPOINT ["dotnet", "InternalApi02.Payment.dll"]