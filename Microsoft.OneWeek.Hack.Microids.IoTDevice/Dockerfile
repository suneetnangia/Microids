# Build from the repo root with the following command:
# docker build -f Microsoft.OneWeek.Hack.Microids.IoTDevice/Dockerfile .
FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build-env
WORKDIR /app

COPY Microsoft.OneWeek.Hack.Microids.IoTDevice /app/Microsoft.OneWeek.Hack.Microids.IoTDevice
COPY Microsoft.OneWeek.Hack.Microids.Common /app/Microsoft.OneWeek.Hack.Microids.Common

WORKDIR /app/Microsoft.OneWeek.Hack.Microids.IoTDevice

RUN dotnet restore

RUN dotnet publish -c Release -o /app/out --no-restore

FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 AS runtime
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=build-env app .
WORKDIR /app/out
ENTRYPOINT ["dotnet", "Microsoft.OneWeek.Hack.Microids.IoTDevice.dll"]
