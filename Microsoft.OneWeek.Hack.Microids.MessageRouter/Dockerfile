FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 AS runtime
WORKDIR /app
COPY published/** ./

ENTRYPOINT ["dotnet", "/app/Microsoft.OneWeek.Hack.Microids.MessageRouter.dll"]