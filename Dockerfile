# =================================================================
# STAGE 1: BUILD - Sử dụng SDK để restore và publish
# =================================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
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

# --- START NEW STEPS FOR MIGRATION IN BUILD STAGE ---
# Install the EF Core CLI tool globally in the build image
RUN dotnet tool install --global dotnet-ef --version 8.0.*
# Set the environment path so the 'dotnet ef' command is available
ENV PATH="${PATH}:/root/.dotnet/tools"

# Copy the entrypoint script into a known location for the final stage
COPY entrypoint.sh /usr/local/bin/entrypoint.sh
RUN chmod +x /usr/local/bin/entrypoint.sh
# --- END NEW STEPS ---

# Lệnh Publish cuối cùng (để tạo ra các file cần thiết cho runtime)
FROM build AS publish
# /p:UseAppHost=false là quan trọng để file chạy được trong container Linux
RUN dotnet publish "FDAAPI.Presentation.FastEndpointBasedApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# =================================================================
# STAGE 2: FINAL - Sử dụng Runtime image nhẹ hơn
# =================================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
# Ứng dụng Kestrel mặc định chạy trên cổng 8080 (hoặc 8081 cho HTTPS)
EXPOSE 8080
EXPOSE 8081 

# Sao chép các file đã publish vào thư mục làm việc cuối cùng
COPY --from=publish /app/publish .

# --- NEW ENTRYPOINT DEFINITION ---
# Copy the executable script from the build stage into the final image
COPY --from=build /usr/local/bin/entrypoint.sh /usr/local/bin/entrypoint.sh

# REPLACE THE OLD ENTRYPOINT
# OLD: ENTRYPOINT ["dotnet", "FDAAPI.Presentation.FastEndpointBasedApi.dll"] 
# NEW: Định nghĩa điểm vào là script, script này sẽ chạy migrations và sau đó chạy app
# Copy entrypoint từ build stage hoặc từ folder gốc
# ... (Phần trước giữ nguyên)

# Copy entrypoint
COPY entrypoint.sh /app/entrypoint.sh

# Lệnh này sẽ ép file về định dạng Linux (LF) ngay khi build
RUN sed -i 's/\r$//' /app/entrypoint.sh && chmod +x /app/entrypoint.sh

# Sử dụng ENTRYPOINT dạng mảng để tránh lỗi shell
ENTRYPOINT ["/bin/bash", "/app/entrypoint.sh"]
# --- END NEW ENTRYPOINT DEFINITION ---