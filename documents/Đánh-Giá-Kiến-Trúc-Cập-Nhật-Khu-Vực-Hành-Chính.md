# Đánh Giá Kiến Trúc: Cập Nhật Mô Hình Khu Vực Hành Chính

**Tài liệu:** Đánh giá kiến trúc hệ thống dự báo lũ lụt  
**Ngày tạo:** 2026-02-05  
**Phiên bản:** 1.0  
**Tác giả:** System Architect & GIS Engineer Review

---

## Mục Lục

1. [Tổng Quan Hệ Thống Hiện Tại](#1-tổng-quan-hệ-thống-hiện-tại)
2. [Tóm Tắt Các Thay Đổi Được Đề Xuất](#2-tóm-tắt-các-thay-đổi-được-đề-xuất)
3. [So Sánh Mô Hình Khu Vực Hành Chính](#3-so-sánh-mô-hình-khu-vực-hành-chính)
4. [Đánh Giá GIS và Mô Hình Không Gian](#4-đánh-giá-gis-và-mô-hình-không-gian)
5. [Kiến Trúc Triển Khai Được Đề Xuất](#5-kiến-trúc-triển-khai-được-đề-xuất)
6. [Rủi Ro và Biện Pháp Giảm Thiểu](#6-rủi-ro-và-biện-pháp-giảm-thiểu)
7. [Khuyến Nghị Cuối Cùng](#7-khuyến-nghị-cuối-cùng)

---

## 1. Tổng Quan Hệ Thống Hiện Tại

### 1.1. Kiến Trúc Tổng Thể

Hệ thống FDA API áp dụng **Domain-Centric Architecture** với các tầng:

- **Presentation Layer**: FastEndpoints (HTTP API)
- **Application Layer**: CQRS Handlers (Business Logic)
- **Domain Layer**: Entities, Repository Interfaces, Business Rules
- **Infrastructure Layer**: PostgreSQL, Redis, PostGIS (nếu có)

### 1.2. Mô Hình Khu Vực Hành Chính Hiện Tại

**Cấu trúc phân cấp:**
```
City (Thành phố)
  └── District (Quận/Huyện)
      └── Ward (Phường/Xã)
          └── Station (Trạm quan trắc)
```

**Thực thể chính:**

1. **AdministrativeArea Entity**
   - `Level`: "city", "district", "ward"
   - `ParentId`: Self-referencing để tạo cây phân cấp
   - `Geometry`: JSON string chứa GeoJSON Polygon (tùy chọn)
   - Quan hệ: `Parent` ↔ `Children` (1-N)

2. **Station Entity**
   - `AdministrativeAreaId`: Foreign key → AdministrativeArea
   - `RoadName`: String (tên đường, không có cấu trúc)
   - `Latitude`, `Longitude`: Tọa độ điểm
   - Quan hệ: N Stations → 1 AdministrativeArea

### 1.3. Luồng Xử Lý Hiện Tại

**FeatG55 - AdministrativeAreasEvaluate:**
- **Input**: AdministrativeAreaId (district hoặc city)
- **Logic**:
  1. Nếu `level == "district"`: Lấy tất cả child wards → Lấy stations trong các wards đó
  2. Nếu `level == "city"`: Lấy tất cả districts → Lấy tất cả wards → Lấy stations
  3. Tính toán flood status dựa trên sensor readings từ các stations
- **Output**: Aggregated flood status với danh sách contributing stations

**FeatG76 - LogPrediction:**
- **Input**: AdministrativeAreaId (ward/district/city)
- **Logic**: Tương tự FeatG55, nhưng lưu prediction log thay vì evaluate real-time

**Ràng buộc hiện tại:**
- Ward level **KHÔNG được hỗ trợ** trong FeatG55 (chỉ district và city)
- Stations phải được gán vào một AdministrativeArea (ward hoặc district)
- Không có entity riêng cho Street/Road

### 1.4. Giả Định và Hạn Chế

**Giả định:**
1. Mỗi station thuộc về một ward cụ thể
2. District là cấp trung gian cần thiết để tổng hợp dữ liệu
3. Geometry của AdministrativeArea là tùy chọn (có thể null)

**Hạn chế:**
1. **Thiếu độ chính xác không gian**: Stations chỉ có điểm (lat/lng), không có thông tin về đoạn đường cụ thể
2. **Không có mô hình đường phố**: `RoadName` chỉ là string, không có geometry
3. **Phụ thuộc vào cấp quận**: Logic xử lý phụ thuộc vào district level
4. **Thiếu dữ liệu ranh giới chính xác**: Geometry có thể null hoặc không chính xác

---

## 2. Tóm Tắt Các Thay Đổi Được Đề Xuất

### 2.1. Cấu Trúc Mới

**Cấu trúc phân cấp đề xuất:**
```
City (Thành phố)
  └── Ward/Commune (Phường/Xã)
      └── Street (Đường phố)
          └── Station (Trạm quan trắc)
```

**Thay đổi chính:**
1. **Loại bỏ cấp District** khỏi logic cốt lõi
2. **Thêm cấp Street** như một entity riêng biệt
3. **Stations được đặt ở cấp Street** thay vì Ward
4. **Ranh giới Ward/Commune** lấy từ GeoJSON file: `Đà Nẵng (phường xã) - 34.geojson`
5. **Street geometry** được ước tính bằng đoạn thẳng (start point - end point)

### 2.2. Dữ Liệu Đầu Vào

**GeoJSON Ward/Commune:**
- File: `Đà Nẵng (phường xã) - 34.geojson`
- Format: GeoJSON FeatureCollection
- Geometry: Polygon (ranh giới phường/xã)
- Properties: Tên phường/xã, mã hành chính

**Street Representation:**
- Geometry: LineString (2 điểm: start lat/lng, end lat/lng)
- Không có dữ liệu đường phố chi tiết từ nguồn bên ngoài
- Phải ước tính từ vị trí stations

### 2.3. Logic Xử Lý Mới

**Dự kiến:**
1. City → Lấy tất cả Wards/Communes (từ GeoJSON)
2. Ward/Commune → Lấy tất cả Streets trong ward đó
3. Street → Lấy Stations trên street đó
4. Tính toán flood prediction dựa trên stations

---

## 3. So Sánh Mô Hình Khu Vực Hành Chính

### 3.1. So Sánh Cấu Trúc

| Khía cạnh | Mô hình hiện tại | Mô hình đề xuất |
|-----------|------------------|-----------------|
| **Cấp độ** | City → District → Ward → Station | City → Ward → Street → Station |
| **Số cấp** | 4 cấp | 4 cấp (nhưng khác cấu trúc) |
| **Cấp trung gian** | District (bắt buộc) | Không có (bỏ qua District) |
| **Cấp mới** | Không có | Street (mới) |
| **Station location** | Ward/District level | Street level |
| **Geometry source** | Tùy chọn, có thể null | GeoJSON file (bắt buộc) |

### 3.2. Ưu Điểm của Mô Hình Mới

1. **Độ chính xác không gian cao hơn**
   - Stations được gán vào street cụ thể thay vì chỉ ward
   - Có thể xác định đoạn đường nào bị ngập

2. **Phù hợp với mô hình lũ lụt**
   - Lũ lụt thường xảy ra ở đường phố, không phải ranh giới hành chính
   - Dự báo theo street giúp người dùng tránh các đoạn đường cụ thể

3. **Dữ liệu ranh giới chính xác**
   - Sử dụng GeoJSON từ nguồn chính thức
   - Đảm bảo tính nhất quán về ranh giới hành chính

4. **Đơn giản hóa phân cấp**
   - Bỏ qua district level giảm độ phức tạp truy vấn
   - Giảm số lượng bước truy vấn database

### 3.3. Nhược Điểm và Thách Thức

1. **Mất thông tin cấp quận**
   - Một số báo cáo/analytics có thể cần cấp district
   - Cần migration dữ liệu hoặc tính toán lại

2. **Thiếu dữ liệu đường phố**
   - Không có nguồn dữ liệu đường phố chính thức
   - Phải ước tính từ vị trí stations (có thể không chính xác)

3. **Phức tạp hóa mô hình dữ liệu**
   - Thêm entity Street mới
   - Cần quản lý quan hệ City → Ward → Street → Station

4. **Rủi ro về chất lượng dữ liệu**
   - Street geometry ước tính có thể không chính xác
   - Có thể có streets không có stations

---

## 4. Đánh Giá GIS và Mô Hình Không Gian

### 4.1. Đánh Giá Cấu Trúc Phân Cấp Mới

**Từ góc nhìn GIS:**

✅ **Hợp lý:**
- Phân cấp City → Ward → Street phù hợp với cấu trúc đô thị
- Ward/Commune là đơn vị hành chính cơ bản, có ranh giới rõ ràng
- Street là đơn vị không gian nhỏ nhất có ý nghĩa cho dự báo lũ

⚠️ **Cần lưu ý:**
- Một street có thể đi qua nhiều wards (ví dụ: đường lớn)
- Cần quyết định: Street thuộc về ward nào? (ward chứa điểm giữa, hay ward chứa điểm đầu?)

**Từ góc nhìn mô hình lũ lụt:**

✅ **Phù hợp:**
- Lũ lụt xảy ra ở đường phố, không phải ranh giới hành chính
- Dự báo theo street giúp người dùng tránh các đoạn đường cụ thể
- Stations đặt ở street level phản ánh đúng vị trí thực tế

⚠️ **Cần xem xét:**
- Một street có thể có nhiều stations (đoạn dài)
- Cần logic tổng hợp: Làm thế nào để tính flood status của một street từ nhiều stations?

**Tính nhất quán và khả năng mở rộng:**

✅ **Nhất quán:**
- Cấu trúc phân cấp rõ ràng, dễ hiểu
- Phù hợp với cấu trúc hành chính Việt Nam

⚠️ **Khả năng mở rộng:**
- Nếu mở rộng sang thành phố khác, cần có GeoJSON tương ứng
- Nếu có dữ liệu đường phố chính thức sau này, cần migration

### 4.2. Đánh Giá Sử Dụng GeoJSON Ward/Commune

**Điểm mạnh:**

1. **Nguồn dữ liệu chính thức**
   - GeoJSON từ cơ quan hành chính đảm bảo tính chính xác
   - Ranh giới được xác định rõ ràng

2. **Format chuẩn**
   - GeoJSON là format phổ biến, dễ xử lý
   - Hỗ trợ tốt bởi các thư viện GIS

3. **Tích hợp dễ dàng**
   - Có thể import trực tiếp vào PostgreSQL với PostGIS
   - Hoặc lưu trữ dưới dạng JSON trong cột Geometry

**Rủi ro và hạn chế:**

1. **Cập nhật dữ liệu**
   - Ranh giới hành chính có thể thay đổi (sáp nhập, chia tách)
   - Cần cơ chế cập nhật định kỳ

2. **Chất lượng dữ liệu**
   - Cần validate GeoJSON trước khi import
   - Kiểm tra: Polygon hợp lệ, không có self-intersection, coordinate system đúng

3. **Hiệu năng**
   - GeoJSON lớn có thể ảnh hưởng hiệu năng truy vấn
   - Cần index spatial (PostGIS GIST index)

**Khuyến nghị:**

1. **Validation pipeline:**
   - Validate GeoJSON structure
   - Check polygon validity
   - Verify coordinate system (WGS84/EPSG:4326)

2. **Storage strategy:**
   - Option A: Lưu trong cột `Geometry` (text/JSON) - đơn giản, nhưng không tối ưu cho spatial queries
   - Option B: Import vào PostGIS với `GEOGRAPHY` type - tối ưu cho spatial queries, nhưng cần PostGIS extension

3. **Indexing:**
   - Nếu dùng PostGIS: Tạo GIST index trên geometry column
   - Nếu dùng JSON: Cần extract bounding box và index trên đó

### 4.3. Đánh Giá Mô Hình Hóa Đường Phố

**Phương pháp đề xuất:**
- Street được biểu diễn bằng LineString (2 điểm: start lat/lng, end lat/lng)
- Geometry được ước tính từ vị trí stations

**Rủi ro:**

1. **Độ chính xác thấp**
   - Đường phố thực tế không phải là đường thẳng
   - Chỉ có 2 điểm không đủ để mô tả đường cong, đường zigzag

2. **Thiếu thông tin**
   - Không biết chiều rộng đường
   - Không biết độ cao (elevation)
   - Không biết loại đường (đường chính, đường phụ)

3. **Vấn đề với nhiều stations**
   - Nếu một street có nhiều stations, làm thế nào để xác định geometry?
   - Có thể tạo MultiLineString? Hoặc chọn 2 stations xa nhất?

4. **Vấn đề với streets không có stations**
   - Làm thế nào để tạo street entity nếu không có stations?
   - Cần nguồn dữ liệu đường phố bên ngoài

**Giảm thiểu rủi ro:**

1. **Cải thiện mô hình:**
   - Sử dụng MultiPoint hoặc LineString với nhiều điểm hơn (nếu có dữ liệu)
   - Lưu trữ thêm metadata: road width, elevation, road type

2. **Nguồn dữ liệu:**
   - Tích hợp OpenStreetMap (OSM) để lấy geometry đường phố
   - Sử dụng GraphHopper routing data (đã có trong hệ thống)

3. **Fallback strategy:**
   - Nếu không có geometry, sử dụng buffer zone quanh stations
   - Hoặc sử dụng ward geometry làm fallback

---

## 5. Kiến Trúc Triển Khai Được Đề Xuất

### 5.1. Mô Hình Dữ Liệu

#### 5.1.1. Entity: Street

```csharp
public class Street : EntityWithId<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; } // Mã đường (nếu có)
    public Guid WardId { get; set; } // Foreign key → AdministrativeArea (ward)
    
    // Geometry: LineString (start point, end point)
    public decimal StartLatitude { get; set; }
    public decimal StartLongitude { get; set; }
    public decimal EndLatitude { get; set; }
    public decimal EndLongitude { get; set; }
    
    // Optional: Full geometry as GeoJSON LineString
    public string? Geometry { get; set; } // GeoJSON LineString
    
    // Metadata
    public string? RoadType { get; set; } // "main", "secondary", "alley"
    public decimal? Width { get; set; } // Chiều rộng đường (mét)
    public decimal? Elevation { get; set; } // Độ cao (mét)
    
    // Navigation
    public virtual AdministrativeArea Ward { get; set; }
    public virtual ICollection<Station> Stations { get; set; }
}
```

**Lưu ý:**
- `WardId` thay vì `AdministrativeAreaId` để rõ ràng hơn
- Geometry có thể là LineString đơn giản (2 điểm) hoặc LineString phức tạp (nhiều điểm)
- Metadata fields là tùy chọn, có thể bổ sung sau

#### 5.1.2. Cập Nhật Entity: Station

```csharp
// Thêm vào Station entity
public Guid? StreetId { get; set; } // Foreign key → Street
public virtual Street? Street { get; set; }

// Giữ lại AdministrativeAreaId để backward compatibility
// Nhưng ưu tiên sử dụng StreetId
```

**Migration strategy:**
- Giữ `AdministrativeAreaId` để không phá vỡ code hiện tại
- Thêm `StreetId` mới
- Logic mới sử dụng `StreetId`, logic cũ vẫn dùng `AdministrativeAreaId`

#### 5.1.3. Cập Nhật Entity: AdministrativeArea

```csharp
// Không thay đổi cấu trúc, nhưng:
// - Level "district" vẫn tồn tại trong database (backward compatibility)
// - Logic mới bỏ qua district level
// - Có thể đánh dấu district là deprecated
public bool IsDeprecated { get; set; } = false; // Optional: đánh dấu district không dùng nữa
```

### 5.2. Lưu Trữ và Truy Vấn Ranh Giới Hành Chính

#### 5.2.1. Import GeoJSON Ward/Commune

**Option A: Lưu trong cột Geometry (JSON string)**

```csharp
// Trong AdministrativeArea entity
public string? Geometry { get; set; } // GeoJSON Polygon as JSON string

// Import process:
// 1. Parse GeoJSON file
// 2. For each Feature:
//    - Extract properties (name, code)
//    - Extract geometry (Polygon)
//    - Create/Update AdministrativeArea with level="ward"
//    - Set Geometry = JsonSerializer.Serialize(geometry)
```

**Ưu điểm:**
- Đơn giản, không cần PostGIS
- Dễ migrate từ hiện tại

**Nhược điểm:**
- Không tối ưu cho spatial queries
- Phải parse JSON mỗi lần cần geometry

**Option B: PostGIS Geography Type (Khuyến nghị)**

```sql
-- Migration
ALTER TABLE "AdministrativeAreas" 
ADD COLUMN "GeometryPostGIS" GEOGRAPHY(POLYGON, 4326);

CREATE INDEX "ix_administrative_areas_geometry_gist" 
ON "AdministrativeAreas" USING GIST("GeometryPostGIS");

-- Import process:
-- 1. Parse GeoJSON
-- 2. Convert GeoJSON Polygon to PostGIS Geography
-- 3. Insert/Update with ST_GeomFromGeoJSON()
```

**Ưu điểm:**
- Tối ưu cho spatial queries (ST_Within, ST_Intersects)
- Hỗ trợ spatial indexing (GIST)
- Có thể tính toán distance, area chính xác

**Nhược điểm:**
- Cần PostGIS extension
- Phức tạp hơn trong migration

**Khuyến nghị:** Sử dụng Option B nếu có PostGIS, hoặc Option A nếu không có.

#### 5.2.2. Validation Pipeline

```csharp
public class GeoJsonValidator
{
    public ValidationResult ValidateWardGeoJson(string geoJsonContent)
    {
        // 1. Parse JSON structure
        // 2. Validate FeatureCollection
        // 3. For each Feature:
        //    - Check geometry type is Polygon
        //    - Validate coordinates (lat: -90 to 90, lng: -180 to 180)
        //    - Check polygon is closed (first point == last point)
        //    - Check no self-intersection (optional, complex)
        // 4. Return validation result
    }
}
```

### 5.3. Xử Lý và Xác Thực GeoJSON

**Import Process:**

1. **Pre-processing:**
   - Validate GeoJSON structure
   - Check coordinate system (should be WGS84/EPSG:4326)
   - Normalize feature properties (name, code)

2. **Data Mapping:**
   - Map GeoJSON properties → AdministrativeArea fields
   - Extract geometry → Geometry field
   - Set level = "ward"
   - Set parentId = City ID

3. **Post-processing:**
   - Verify all wards are imported
   - Check for duplicates
   - Validate parent-child relationships

**Error Handling:**
- Invalid geometry → Log warning, skip feature
- Missing properties → Use default values or skip
- Duplicate names → Append suffix or use code

### 5.4. Biểu Diễn Đường Phố

#### 5.4.1. Cấu Trúc Dữ Liệu

**Simple LineString (2 points):**
```json
{
  "type": "LineString",
  "coordinates": [
    [108.2200, 16.0670], // Start point
    [108.2250, 16.0700]  // End point
  ]
}
```

**MultiPoint (nếu có nhiều stations):**
```json
{
  "type": "LineString",
  "coordinates": [
    [108.2200, 16.0670], // Station 1
    [108.2220, 16.0680], // Station 2
    [108.2250, 16.0700]  // Station 3
  ]
}
```

#### 5.4.2. Loại Hình Học

**Recommendation:**
- **LineString** cho streets có 2+ stations (nối các stations)
- **Point** cho streets chỉ có 1 station (fallback)
- **Buffer zone** cho streets không có stations (nếu cần)

#### 5.4.3. Tạo Street từ Stations

**Algorithm:**

```csharp
public class StreetGenerator
{
    public Street CreateStreetFromStations(
        string streetName,
        Guid wardId,
        List<Station> stations)
    {
        if (stations.Count == 0)
            throw new ArgumentException("Cannot create street without stations");
        
        if (stations.Count == 1)
        {
            // Single station: Use point geometry
            var station = stations[0];
            return new Street
            {
                Name = streetName,
                WardId = wardId,
                StartLatitude = station.Latitude.Value,
                StartLongitude = station.Longitude.Value,
                EndLatitude = station.Latitude.Value,
                EndLongitude = station.Longitude.Value,
                Geometry = CreatePointGeoJson(station.Latitude, station.Longitude)
            };
        }
        
        // Multiple stations: Create LineString
        // Option 1: Order by distance from start point
        var orderedStations = OrderStationsByPath(stations);
        
        // Option 2: Use convex hull or path finding
        var coordinates = orderedStations
            .Select(s => new[] { s.Longitude.Value, s.Latitude.Value })
            .ToArray();
        
        return new Street
        {
            Name = streetName,
            WardId = wardId,
            StartLatitude = orderedStations.First().Latitude.Value,
            StartLongitude = orderedStations.First().Longitude.Value,
            EndLatitude = orderedStations.Last().Latitude.Value,
            EndLongitude = orderedStations.Last().Longitude.Value,
            Geometry = CreateLineStringGeoJson(coordinates)
        };
    }
}
```

**Challenges:**
- Làm thế nào để sắp xếp stations theo thứ tự đúng? (cần routing data hoặc heuristic)
- Nếu stations không nằm trên cùng một đường thẳng, LineString có thể không chính xác

### 5.5. Liên Kết Stations với Wards và Streets

#### 5.5.1. Quan Hệ

```
AdministrativeArea (Ward)
  └── Streets (1-N)
      └── Stations (1-N)

Station
  ├── StreetId (FK → Street) [NEW, Primary]
  └── AdministrativeAreaId (FK → AdministrativeArea) [KEEP, Fallback]
```

#### 5.5.2. Logic Gán Station vào Street

**Approach 1: Manual Assignment (Khuyến nghị cho Phase 1)**
- Admin gán station vào street thủ công
- Có UI để quản lý

**Approach 2: Automatic Assignment**
- Dựa trên `RoadName` trong Station
- Match với Street.Name
- Nếu không match, tạo street mới

**Approach 3: Spatial Assignment**
- Tìm street gần nhất với station (dùng distance calculation)
- Gán station vào street đó

**Khuyến nghị:** Kết hợp Approach 1 và 2:
- Phase 1: Manual assignment với UI hỗ trợ
- Phase 2: Auto-assignment dựa trên RoadName matching

#### 5.5.3. Migration Existing Stations

```csharp
public class StationMigrationService
{
    public async Task MigrateStationsToStreets(CancellationToken ct)
    {
        // 1. Get all stations with AdministrativeAreaId (ward level)
        var stations = await _stationRepository.GetStationsWithWardAsync(ct);
        
        // 2. Group by RoadName and WardId
        var grouped = stations
            .Where(s => !string.IsNullOrEmpty(s.RoadName))
            .GroupBy(s => new { s.RoadName, s.AdministrativeAreaId });
        
        // 3. For each group:
        foreach (var group in grouped)
        {
            // 3a. Find or create Street
            var street = await FindOrCreateStreetAsync(
                group.Key.RoadName,
                group.Key.AdministrativeAreaId.Value,
                group.ToList(),
                ct);
            
            // 3b. Update stations with StreetId
            foreach (var station in group)
            {
                station.StreetId = street.Id;
                await _stationRepository.UpdateAsync(station, ct);
            }
        }
    }
}
```

### 5.6. Phân Chia Logic: Server vs AI

#### 5.6.1. Server-Side Logic

**Responsibility:**
1. **Data Management:**
   - Import và quản lý GeoJSON wards
   - Tạo và quản lý Street entities
   - Gán stations vào streets

2. **Spatial Queries:**
   - Tìm stations trong một ward
   - Tìm streets trong một ward
   - Tính toán distance, intersection

3. **Data Aggregation:**
   - Tổng hợp sensor readings theo street
   - Tổng hợp flood status theo ward

4. **API Endpoints:**
   - CRUD operations cho Street
   - Query stations by street
   - Query streets by ward

#### 5.6.2. AI-Side Logic

**Responsibility:**
1. **Prediction:**
   - Nhận AdministrativeAreaId (ward hoặc city)
   - Tính toán flood prediction dựa trên stations
   - Gửi prediction log về server

2. **Model Training:**
   - Sử dụng historical data từ stations
   - Không cần biết về street structure

**Interface:**
- AI vẫn gửi `AdministrativeAreaId` (ward level)
- Server xử lý mapping ward → streets → stations
- AI không cần biết về Street entity

**Khuyến nghị:**
- Giữ interface đơn giản: AI chỉ cần biết ward/city
- Server xử lý tất cả logic street-level internally

---

## 6. Rủi Ro và Biện Pháp Giảm Thiểu

### 6.1. Rủi Ro Kỹ Thuật

#### 6.1.1. Migration Dữ Liệu

**Rủi ro:**
- Mất dữ liệu trong quá trình migration
- Phá vỡ backward compatibility
- Downtime trong quá trình migration

**Biện pháp giảm thiểu:**
1. **Backup trước khi migration:**
   ```bash
   ./scripts/backup_dev_schema.sh
   ```

2. **Migration strategy:**
   - Phase 1: Thêm Street entity, giữ nguyên logic cũ
   - Phase 2: Migrate stations sang streets
   - Phase 3: Cập nhật logic mới, giữ logic cũ làm fallback
   - Phase 4: Deprecate logic cũ sau khi verify

3. **Rollback plan:**
   - Giữ migration scripts có thể rollback
   - Test migration trên UAT environment trước

#### 6.1.2. Performance

**Rủi ro:**
- Spatial queries chậm nếu không có index
- GeoJSON parsing tốn tài nguyên
- Nhiều joins (City → Ward → Street → Station) có thể chậm

**Biện pháp giảm thiểu:**
1. **Indexing:**
   ```sql
   -- Spatial index (nếu dùng PostGIS)
   CREATE INDEX ix_administrative_areas_geometry_gist 
   ON "AdministrativeAreas" USING GIST("GeometryPostGIS");
   
   -- B-tree indexes
   CREATE INDEX ix_street_ward ON "Streets"("WardId");
   CREATE INDEX ix_station_street ON "Stations"("StreetId");
   ```

2. **Caching:**
   - Cache ward boundaries (GeoJSON)
   - Cache street-station mappings
   - Cache aggregated flood status

3. **Query optimization:**
   - Sử dụng eager loading cho navigation properties
   - Batch queries thay vì N+1 queries

#### 6.1.3. Data Quality

**Rủi ro:**
- GeoJSON không hợp lệ
- Streets không có stations
- Stations không có street assignment

**Biện pháp giảm thiểu:**
1. **Validation:**
   - Validate GeoJSON trước khi import
   - Check polygon validity
   - Verify coordinate ranges

2. **Data integrity:**
   - Foreign key constraints
   - Check constraints (e.g., street must have at least 1 station)
   - Regular data quality checks

3. **Monitoring:**
   - Alert nếu có streets không có stations
   - Alert nếu có stations không có street assignment

### 6.2. Rủi Ro Logic

#### 6.2.1. Mất Thông Tin Cấp Quận

**Rủi ro:**
- Một số báo cáo/analytics cần cấp district
- Users có thể quen với district-level data

**Biện pháp giảm thiểu:**
1. **Backward compatibility:**
   - Giữ district data trong database
   - Tính toán district-level aggregation từ wards (nếu cần)

2. **Virtual district:**
   - Tạo computed view hoặc materialized view cho district-level data
   - Không cần entity riêng, chỉ tính toán khi cần

#### 6.2.2. Street Geometry Không Chính Xác

**Rủi ro:**
- Street geometry ước tính từ stations có thể không chính xác
   - Đường cong bị làm thẳng
   - Đường zigzag bị làm đơn giản hóa

**Biện pháp giảm thiểu:**
1. **Cải thiện algorithm:**
   - Sử dụng routing data (GraphHopper) nếu có
   - Sử dụng OpenStreetMap data nếu có
   - Sử dụng MultiPoint thay vì LineString đơn giản

2. **Fallback:**
   - Nếu không có geometry, sử dụng buffer zone quanh stations
   - Hoặc sử dụng ward geometry làm fallback

3. **Manual correction:**
   - Cho phép admin chỉnh sửa street geometry thủ công
   - Có UI để visualize và edit

#### 6.2.3. Streets Đi Qua Nhiều Wards

**Rủi ro:**
- Một street có thể đi qua nhiều wards
- Làm thế nào để xác định street thuộc ward nào?

**Biện pháp giảm thiểu:**
1. **Primary ward:**
   - Xác định ward chứa điểm giữa của street
   - Hoặc ward chứa nhiều stations nhất

2. **Multi-ward streets:**
   - Cho phép street có nhiều wards (many-to-many relationship)
   - Hoặc tách street thành nhiều segments, mỗi segment thuộc một ward

**Khuyến nghị:** Sử dụng primary ward approach (đơn giản hơn)

### 6.3. Rủi Ro Dữ Liệu

#### 6.3.1. Thiếu Dữ Liệu Đường Phố

**Rủi ro:**
- Không có nguồn dữ liệu đường phố chính thức
- Phải ước tính từ stations (có thể không đầy đủ)

**Biện pháp giảm thiểu:**
1. **Nguồn dữ liệu thay thế:**
   - OpenStreetMap (OSM)
   - GraphHopper routing data (đã có trong hệ thống)
   - Google Maps API (nếu có budget)

2. **Gradual improvement:**
   - Phase 1: Ước tính từ stations
   - Phase 2: Tích hợp OSM data
   - Phase 3: Manual correction và refinement

#### 6.3.2. Cập Nhật Ranh Giới Hành Chính

**Rủi ro:**
- Ranh giới hành chính có thể thay đổi (sáp nhập, chia tách)
   - Wards có thể được sáp nhập
   - Wards có thể được chia tách thành nhiều wards mới

**Biện pháp giảm thiểu:**
1. **Versioning:**
   - Lưu trữ version của GeoJSON
   - Track changes trong database

2. **Update process:**
   - Có script để import GeoJSON mới
   - So sánh với version cũ để detect changes
   - Migrate existing data (stations, streets) sang wards mới

3. **Audit trail:**
   - Log tất cả changes
   - Có thể rollback nếu cần

---

## 7. Khuyến Nghị Cuối Cùng

### 7.1. Khuyến Nghị Tổng Thể

**✅ Nên triển khai mô hình mới với các điều kiện:**

1. **Phase 1: Foundation (2-3 tuần)**
   - Import GeoJSON wards vào database
   - Tạo Street entity và migration
   - Migrate existing stations sang streets (manual hoặc semi-automatic)
   - Giữ backward compatibility với logic cũ

2. **Phase 2: Logic Mới (2-3 tuần)**
   - Cập nhật FeatG55, FeatG76 để sử dụng street-level logic
   - Test và verify với dữ liệu thực tế
   - Performance tuning và optimization

3. **Phase 3: Enhancement (1-2 tuần)**
   - Cải thiện street geometry (tích hợp OSM hoặc routing data)
   - UI để quản lý streets
   - Monitoring và alerting

### 7.2. Điều Kiện Tiên Quyết

**Trước khi triển khai:**

1. **Dữ liệu:**
   - ✅ GeoJSON file đã có sẵn
   - ⚠️ Cần validate GeoJSON quality
   - ⚠️ Cần quyết định storage strategy (JSON string vs PostGIS)

2. **Infrastructure:**
   - ⚠️ Cần PostGIS extension nếu muốn tối ưu spatial queries
   - ✅ Database schema có thể migrate

3. **Resources:**
   - ⚠️ Cần thời gian để migration và testing
   - ⚠️ Cần người có kinh nghiệm GIS để review

### 7.3. Rủi Ro Chính Cần Quan Tâm

1. **Street geometry không chính xác** (High)
   - Giảm thiểu: Sử dụng routing data hoặc OSM

2. **Migration phức tạp** (Medium)
   - Giảm thiểu: Phased approach, backup, testing

3. **Performance degradation** (Medium)
   - Giảm thiểu: Indexing, caching, query optimization

### 7.4. Kế Hoạch Hành Động

**Immediate (Tuần 1-2):**
1. Review và validate GeoJSON file
2. Quyết định storage strategy (JSON vs PostGIS)
3. Design Street entity và migration script
4. Create UAT environment để test

**Short-term (Tuần 3-6):**
1. Implement Street entity và migration
2. Import GeoJSON wards
3. Migrate stations sang streets
4. Update logic trong FeatG55, FeatG76

**Long-term (Tuần 7+):**
1. Cải thiện street geometry
2. UI để quản lý streets
3. Monitoring và optimization

### 7.5. Kết Luận

Mô hình mới (City → Ward → Street → Station) **phù hợp hơn** với mục tiêu dự báo lũ lụt vì:
- ✅ Độ chính xác không gian cao hơn
- ✅ Phù hợp với mô hình lũ lụt (lũ xảy ra ở đường phố)
- ✅ Dữ liệu ranh giới chính xác từ GeoJSON

Tuy nhiên, cần **cẩn thận** với:
- ⚠️ Street geometry ước tính có thể không chính xác
- ⚠️ Migration phức tạp và có rủi ro
- ⚠️ Cần nguồn dữ liệu đường phố tốt hơn trong tương lai

**Khuyến nghị:** Triển khai theo phased approach với backward compatibility để giảm thiểu rủi ro.

---

**Tài liệu kết thúc**

