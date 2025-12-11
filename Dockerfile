# =================================================================
# STAGE 1: BUILD - Sử dụng SDK để restore và publish
# =================================================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 1. Sao chép SLN file và tất cả các file .csproj (Tận dụng Build Cache)
# Việc này giúp Docker chỉ chạy lại 'dotnet restore' khi các file .csproj thay đổi.
COPY ["FDA_Api.sln", "."]
# Sao chép các tệp .csproj theo cấu trúc thư mục của bạn
COPY ["src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/FDAAPI.Presentation.FastEndpointBasedApi.csproj", "src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/"]
COPY ["src/External/Infrastructure/Common/FDAAPI.Infra.Configuration/FDAAPI.Infra.Configuration.csproj", "src/External/Infrastructure/Common/FDAAPI.Infra.Configuration/"]
COPY ["src/Core/Application/FDAAPI.App.Common/FDAAPI.App.Common.csproj", "src/Core/Application/FDAAPI.App.Common/"]
COPY ["src/Core/Application/FDAAPI.App.FeatG1/FDAAPI.App.FeatG1.csproj", "src/Core/Application/FDAAPI.App.FeatG1/"]
COPY ["src/Core/Application/FDAAPI.App.FeatG2/FDAAPI.App.FeatG2.csproj", "src/Core/Application/FDAAPI.App.FeatG2/"]
COPY ["src/Core/Application/FDAAPI.App.FeatG3/FDAAPI.App.FeatG3.csproj", "src/Core/Application/FDAAPI.App.FeatG3/"]
COPY ["src/Core/Application/FDAAPI.App.FeatG4/FDAAPI.App.FeatG4.csproj", "src/Core/Application/FDAAPI.App.FeatG4/"]
COPY ["src/Core/Application/FDAAPI.App.FeatG5/FDAAPI.App.FeatG5.csproj", "src/Core/Application/FDAAPI.App.FeatG5/"]
COPY ["src/Core/Domain/FDAAPI.Domain.RelationalDb/FDAAPI.Domain.RelationalDb.csproj", "src/Core/Domain/FDAAPI.Domain.RelationalDb/"]
COPY ["src/External/Infrastructure/Persistence/FDAAPI.Infra.Persistence/FDAAPI.Infra.Persistence.csproj", "src/External/Infrastructure/Persistence/FDAAPI.Infra.Persistence/"]
COPY ["src/External/BuildingBlock/FDAAPI.BuildingBlock.FeatRegister/FDAAPI.BuildingBlock.FeatRegister.csproj", "src/External/BuildingBlock/FDAAPI.BuildingBlock.FeatRegister/"]

# 2. Restore các gói NuGet
RUN dotnet restore "src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/FDAAPI.Presentation.FastEndpointBasedApi.csproj"

# 3. Sao chép phần còn lại của mã nguồn và thực hiện Publish
COPY . .
WORKDIR "/src/src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi"

# Lệnh Publish cuối cùng (để tạo ra các file cần thiết cho runtime)
FROM build AS publish
# /p:UseAppHost=false là quan trọng để file chạy được trong container Linux
RUN dotnet publish "FDAAPI.Presentation.FastEndpointBasedApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# =================================================================
# STAGE 2: FINAL - Sử dụng Runtime image nhẹ hơn
# =================================================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
# Ứng dụng Kestrel mặc định chạy trên cổng 8080 (hoặc 8081 cho HTTPS)
EXPOSE 8080
EXPOSE 8081 

# Sao chép các file đã publish vào thư mục làm việc cuối cùng
COPY --from=publish /app/publish .

# Định nghĩa điểm vào (ENTRYPOINT) - Quan trọng: Tên file DLL phải chính xác
ENTRYPOINT ["dotnet", "FDAAPI.Presentation.FastEndpointBasedApi.dll"]