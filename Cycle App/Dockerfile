
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY ["Cycle App/Cycle App.csproj", "Cycle App/"]
RUN dotnet restore "Cycle App/Cycle App.csproj"

COPY . .
RUN dotnet publish "Cycle App/Cycle App.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Cycle App.dll"]
