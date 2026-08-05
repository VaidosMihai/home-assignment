# Stage 1: Build the .NET application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["HomeLibrary.Api.csproj", "."]
RUN dotnet restore "HomeLibrary.Api.csproj"
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Run the application
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "HomeLibrary.Api.dll"]