# =================================================
# STAGE 1: BUILD
# =================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy file solution và toàn bộ code vào
COPY FDA_Api.sln .
COPY src/ ./src/

# Restore các NuGet packages
RUN dotnet restore "src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/FDAAPI.Presentation.FastEndpointBasedApi.csproj"

# Build và Publish ứng dụng sang folder /app/publish
WORKDIR "/src/src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi"
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# =================================================
# STAGE 2: RUNTIME
# =================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Mở port 8080 mặc định của .NET 8
EXPOSE 8080

# Copy kết quả đã build từ stage build sang stage runtime
COPY --from=build /app/publish .

# Chạy ứng dụng
ENTRYPOINT ["dotnet", "FDAAPI.Presentation.FastEndpointBasedApi.dll"]