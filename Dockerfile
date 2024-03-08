# SPDX-License-Identifier: MIT
# Copyright: 2023 Econolite Systems, Inc.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
RUN apt-get update -y && apt-get install -y libgdiplus libc6-dev
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ENV SolutionDir /src
WORKDIR /src
COPY . .
# Generate model files
RUN dotnet new tool-manifest
RUN dotnet tool install Mapster.Tool
RUN dotnet build ./connected-vehicle/Models.ConnectedVehicle/ -c Release -o /app/build
RUN dotnet build ./device-manager/Models.DeviceManager/ -c Release -o /app/build
RUN dotnet build ./pavement-condition/Models.PavementCondition/ -c Release -o /app/build
RUN dotnet build ./wrong-way-driver/Models.WrongWayDriver/ -c Release -o /app/build
# Build Api
WORKDIR "/src/Api.Configuration"
RUN dotnet build Api.Configuration.csproj -c Release -o /app/build


FROM build AS publish
RUN dotnet publish Api.Configuration.csproj --no-restore -c Release -o /app/publish


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Api.Configuration.dll"]
