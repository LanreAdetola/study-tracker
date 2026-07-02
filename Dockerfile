# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS client-build
WORKDIR /src
COPY client/ .
RUN dotnet publish client.csproj -c Release -o /app/client-publish

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS api-build
WORKDIR /src
COPY api/ .
RUN dotnet publish api.csproj -c Release -o /app/api-publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=api-build /app/api-publish .
COPY --from=client-build /app/client-publish/wwwroot ./wwwroot

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "api.dll"]
