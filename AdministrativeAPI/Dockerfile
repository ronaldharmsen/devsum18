FROM microsoft/aspnetcore:2.0-nanoserver-1803 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/aspnetcore-build:2.0-nanoserver-1803 AS build
WORKDIR /src
COPY ["AdministrativeAPI/AdministrativeAPI.csproj", "AdministrativeAPI/"]
RUN dotnet restore "AdministrativeAPI/AdministrativeAPI.csproj"
COPY . .
WORKDIR "/src/AdministrativeAPI"
RUN dotnet build "AdministrativeAPI.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "AdministrativeAPI.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "AdministrativeAPI.dll"]