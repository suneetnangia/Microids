FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build-env
WORKDIR /app

COPY . ./

RUN dotnet restore

RUN dotnet publish -c Release -o out --no-restore

FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 AS runtime
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=build-env app .
WORKDIR /app/out
ENTRYPOINT ["dotnet", "Microsoft.OneWeek.Hack.Microids.IoTDevice.Metadata.Restful.dll"]