FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base

#WORKDIR /app
#EXPOSE 34071
EXPOSE 7154
EXPOSE 5124

COPY ["bin/Debug/net6.0/", "."]

ENTRYPOINT ["dotnet", "Router.Api.dll"]
