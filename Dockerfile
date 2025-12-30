# =================================================
# STAGE 1: BUILD
# =================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY FDA_Api.sln .
COPY src ./src

RUN dotnet restore "src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/FDAAPI.Presentation.FastEndpointBasedApi.csproj"

WORKDIR /src/src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# =================================================
# STAGE 2: RUNTIME (BẮT BUỘC)
# =================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FDAAPI.Presentation.FastEndpointBasedApi.dll"]
