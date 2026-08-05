FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Worker/HomeLibrary.Worker.csproj", "src/Worker/"]
COPY ["src/Worker/Directory.Build.props", "src/Worker/"]
COPY ["HomeLibrary.Api/HomeLibrary.Api.csproj", "HomeLibrary.Api/"]
RUN dotnet restore "src/Worker/HomeLibrary.Worker.csproj"
COPY . .
RUN dotnet publish "src/Worker/HomeLibrary.Worker.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "HomeLibrary.Worker.dll"]
