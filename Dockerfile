FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Tarea_01.csproj", "./"]
RUN dotnet restore "Tarea_01.csproj"

COPY . .
RUN dotnet publish "Tarea_01.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 10000

CMD ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-10000} dotnet Tarea_01.dll"]