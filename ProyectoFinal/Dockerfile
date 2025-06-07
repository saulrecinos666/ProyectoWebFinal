# Stage 1: Build the ASP.NET Core application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ProyectoFinal.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet publish "ProyectoFinal.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

COPY --from=build /src/Views ./Views

COPY --from=build /src/wwwroot ./wwwroot

ENV ASPNETCORE_URLS=http://0.0.0.0:80

ENTRYPOINT ["dotnet", "ProyectoFinal.dll"]