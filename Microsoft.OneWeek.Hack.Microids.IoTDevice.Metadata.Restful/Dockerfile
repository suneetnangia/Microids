# Build from the repo root with the following command:
# docker build -f Microsoft.OneWeek.Hack.Microids.IoTDevice.Metadata.Restful/Dockerfile .
FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build-env
WORKDIR /app

COPY Microsoft.OneWeek.Hack.Microids.IoTDevice.Metadata.Restful /app/Microsoft.OneWeek.Hack.Microids.IoTDevice.Metadata.Restful
COPY Microsoft.OneWeek.Hack.Microids.Common /app/Microsoft.OneWeek.Hack.Microids.Common

WORKDIR /app/Microsoft.OneWeek.Hack.Microids.IoTDevice.Metadata.Restful

RUN dotnet restore

RUN dotnet publish -c Release -o /app/out --no-restore

FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 AS runtime
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=build-env app .
WORKDIR /app/out
ENTRYPOINT ["dotnet", "Microsoft.OneWeek.Hack.Microids.IoTDevice.Metadata.Restful.dll"]