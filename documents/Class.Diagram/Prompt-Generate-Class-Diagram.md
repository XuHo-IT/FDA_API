# Prompt Template: Generate PlantUML Class Diagram for FDA API Feature

## 🎯 Objective
Generate a comprehensive PlantUML Class Diagram for a specific feature in the FDA API project following Domain-Centric Architecture (Clean Architecture + CQRS pattern).

---

## 📋 Prompt Template

```
Tạo PlantUML Class Diagram cho feature [FEATURE_CODE] - [FEATURE_NAME] của dự án FDA API.

**Context dự án:**
- Architecture: Domain-Centric Architecture (Clean Architecture + CQRS)
- Pattern: MediatR CQRS (IRequest/IRequestHandler)
- Framework: ASP.NET Core 8.0 + FastEndpoints
- Database: PostgreSQL với Entity Framework Core
- Authentication: JWT Bearer

**Yêu cầu diagram:**

1. **Phân chia theo 4-5 packages chính:**
   - PRESENTATION LAYER (FastEndpoints + DTOs) - màu #E1D5E7
   - APPLICATION LAYER (CQRS Handlers + Request/Response) - màu #D5E8D4
   - DOMAIN LAYER - Entities (Entity classes) - màu #F8CECC
   - DOMAIN LAYER - Repositories (Repository Interfaces) - màu #FFE6CC
   - COMMON SERVICES (Infrastructure Services) - màu #E1D5E7

2. **PRESENTATION LAYER phải bao gồm:**
   - [FeatureName]Endpoint class với:
     - Field: `- _mediator: IMediator`
     - Method: `+ Configure(): void`
     - Method: `+ HandleAsync(req: [RequestDto], ct): Task`
   - [RequestDto] class với các properties input
   - [ResponseDto] class với các properties output
   - Các nested DTOs nếu có

3. **APPLICATION LAYER phải bao gồm:**
   - [FeatureName]Request record với đầy đủ properties
   - [FeatureName]Response class với đầy đủ properties
   - [FeatureName]Handler class với:
     - Dependencies (repositories, services) as private fields
     - Method: `+ Handle(request, ct): Task<Response>`
     - Các private helper methods nếu có
   - Interface: `IRequestHandler<[Request], [Response]>`

4. **DOMAIN LAYER - Entities phải bao gồm:**
   - Tất cả Entity classes liên quan đến feature
   - Mỗi entity có đầy đủ properties và navigation properties
   - Relationships giữa các entities (1-1, 1-many, many-many)

5. **DOMAIN LAYER - Repositories phải bao gồm:**
   - Interface definitions cho repositories
   - Methods với signature đầy đủ
   - Generic types rõ ràng

6. **COMMON SERVICES phải bao gồm:**
   - Các service interfaces được handler sử dụng
   - IMediator interface

7. **Relationships phải thể hiện:**
   - Endpoint uses MediatR: `..>`
   - Endpoint sends Request: `..>`
   - Handler implements IRequestHandler: `..|>`
   - Handler uses Repositories: `..>`
   - Handler uses Services: `..>`
   - Response contains nested DTOs: `*--`
   - Entity relationships: `"1" *-- "many"`

8. **Legend phải có:**
   - Giải thích màu sắc các layers
   - Giải thích các loại relationships
   - Đặt ở `legend bottom left`

9. **Styling requirements:**
    - Sử dụng skinparam để định nghĩa màu sắc
    - Title rõ ràng với feature code và name
    - Sử dụng stereotypes: <<Endpoint>>, <<DTO>>, <<Handler>>, <<Entity>>, <<Interface>>
    - Phân tách rõ ràng giữa fields và methods bằng `--`

**Feature cần tạo diagram:**
- Feature Code: [FEATURE_CODE]
- Feature Name: [FEATURE_NAME]
- Feature Description: [MÔ TẢ CHI TIẾT FEATURE]

**Các file source code liên quan:**
- Endpoint: [ĐỊA CHỈ FILE ENDPOINT]
- Handler: [ĐỊA CHỈ FILE HANDLER]
- Entities: [DANH SÁCH ENTITIES]
- Repositories: [DANH SÁCH REPOSITORIES]

**Output format:**
- File PlantUML (.puml) có thể render được
- Lưu vào: d:\Capstone Project\FDA_API\documents\Class.Diagram\[FEATURE_CODE]_[FeatureName].puml
- Syntax hoàn chỉnh, không có lỗi
- Có thể preview trực tiếp trong VSCode với PlantUML extension

**Reference example:**
Tham khảo cấu trúc từ file: d:\Capstone Project\FDA_API\documents\Class.Diagram\FeatG7_AuthLogin.puml
```

---

## 🔧 Cách sử dụng Prompt

### Bước 1: Điền thông tin feature
Thay thế các placeholder trong prompt:
- `[FEATURE_CODE]`: Mã feature (VD: FeatG28)
- `[FEATURE_NAME]`: Tên feature (VD: GetMapPreferences)
- `[MÔ TẢ CHI TIẾT FEATURE]`: Mô tả chi tiết về feature
- `[ĐỊA CHỈ FILE ENDPOINT]`: Path đến endpoint file
- `[ĐỊA CHỈ FILE HANDLER]`: Path đến handler file
- `[DANH SÁCH ENTITIES]`: Các entities liên quan
- `[DANH SÁCH REPOSITORIES]`: Các repositories sử dụng

### Bước 2: Paste prompt vào Claude
Copy toàn bộ prompt đã điền thông tin và paste vào Claude Code

### Bước 3: Verify output
- Kiểm tra file .puml được tạo ra
- Preview trong VSCode với PlantUML extension (Alt+D)
- Verify các layers, relationships, notes có đầy đủ không

---

## 📝 Example Usage

### Example 1: FeatG28_GetMapPreferences

```
Tạo PlantUML Class Diagram cho feature FeatG28 - GetMapPreferences của dự án FDA API.

**Context dự án:**
- Architecture: Domain-Centric Architecture (Clean Architecture + CQRS)
- Pattern: MediatR CQRS (IRequest/IRequestHandler)
- Framework: ASP.NET Core 8.0 + FastEndpoints
- Database: PostgreSQL với Entity Framework Core
- Authentication: JWT Bearer

**Yêu cầu diagram:**
[... giống như trên ...]

**Feature cần tạo diagram:**
- Feature Code: FeatG28
- Feature Name: GetMapPreferences
- Feature Description: API endpoint để lấy map preferences của user (baseMap, layers, zoom, center)

**Các file source code liên quan:**
- Endpoint: src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/Endpoints/FeatG28_GetMapPreferences/GetMapPreferencesEndpoint.cs
- Handler: src/Core/Application/FDAAPI.App.FeatG28_GetMapPreferences/GetMapPreferencesHandler.cs
- Entities: UserPreference, User
- Repositories: IUserPreferenceRepository, IUserRepository

**Output format:**
- File PlantUML (.puml) có thể render được
- Lưu vào: d:\Capstone Project\FDA_API\documents\Class.Diagram\FeatG28_GetMapPreferences.puml
- Syntax hoàn chỉnh, không có lỗi
- Có thể preview trực tiếp trong VSCode với PlantUML extension

**Reference example:**
Tham khảo cấu trúc từ file: d:\Capstone Project\FDA_API\documents\Class.Diagram\FeatG7_AuthLogin.puml
```

### Example 2: FeatG20_UserCreate

```
Tạo PlantUML Class Diagram cho feature FeatG20 - UserCreate của dự án FDA API.

**Feature cần tạo diagram:**
- Feature Code: FeatG20
- Feature Name: UserCreate
- Feature Description: API endpoint để tạo user mới (ADMIN/SUPERADMIN only)

**Các file source code liên quan:**
- Endpoint: src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/Endpoints/FeatG20_UserCreate/CreateUserEndpoint.cs
- Handler: src/Core/Application/FDAAPI.App.FeatG20_UserCreate/CreateUserHandler.cs
- Entities: User, Role, UserRole
- Repositories: IUserRepository, IRoleRepository, IUserRoleRepository

**Output format:**
- Lưu vào: d:\Capstone Project\FDA_API\documents\Class.Diagram\FeatG20_UserCreate.puml
```

---

## 🎨 PlantUML Skinparam Reference

```plantuml
skinparam backgroundColor #FEFEFE
skinparam classBackgroundColor #FFFFFF
skinparam classBorderColor #000000
skinparam packageBackgroundColor #E8E8E8
skinparam packageBorderColor #666666
skinparam stereotypeCBackgroundColor #ADD1B2
skinparam arrowColor #333333
```

## 🎨 Color Scheme

| Layer | Color Code | Hex |
|-------|------------|-----|
| Presentation Layer | Purple | #E1D5E7 |
| Application Layer | Green | #D5E8D4 |
| Domain Entities | Red | #F8CECC |
| Domain Repositories | Orange | #FFE6CC |
| Common Services | Purple | #E1D5E7 |

## 🔗 Relationship Types

| Symbol | Meaning | Usage |
|--------|---------|-------|
| `-->` | Dependency | Normal dependency |
| `..>` | Uses/Dashed | Temporary usage, interface |
| `..\|>` | Implements | Interface implementation |
| `*--` | Composition | Strong ownership |
| `o--` | Aggregation | Weak ownership |
| `"1" *-- "many"` | One-to-Many | Database relationship |

---

## ✅ Checklist

Khi tạo diagram xong, verify các items sau:

- [ ] File .puml syntax đúng, không có lỗi
- [ ] Có 4-5 packages rõ ràng
- [ ] Presentation Layer đầy đủ (Endpoint + DTOs)
- [ ] Application Layer đầy đủ (Request/Response/Handler)
- [ ] Domain Entities đầy đủ với relationships
- [ ] Repository Interfaces đầy đủ với methods
- [ ] Common Services (IMediator, etc.)
- [ ] Có legend giải thích màu sắc và relationships
- [ ] Title rõ ràng với feature code
- [ ] Relationships đúng và đầy đủ
- [ ] Có thể preview trong VSCode PlantUML extension
- [ ] File được lưu đúng vị trí: `documents/Class.Diagram/[FeatureCode]_[FeatureName].puml`

---

## 📚 Additional Resources

- PlantUML Official: https://plantuml.com/class-diagram
- Online Editor: http://www.plantuml.com/plantuml/uml/
- VSCode Extension: PlantUML (jebbs.plantuml)
- Reference Diagram: `d:\Capstone Project\FDA_API\documents\Class.Diagram\FeatG7_AuthLogin.puml`

---

## 💡 Tips

1. **Start with layers**: Tạo packages trước, sau đó mới add classes
2. **Group related classes**: Đặt các classes liên quan gần nhau
3. **Use stereotypes**: <<Endpoint>>, <<DTO>>, <<Handler>>, etc.
4. **Verify relationships**: Đảm bảo arrows đi đúng hướng
5. **Keep it readable**: Không quá nhiều classes trong 1 diagram (max 15-20)
6. **Use colors consistently**: Giữ màu sắc nhất quán cho từng layer
7. **Preview frequently**: Preview trong VSCode để catch lỗi sớm

---

## 🔄 Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2026-01-11 | Initial prompt template creation |

---

## 📞 Support

Nếu gặp vấn đề khi generate diagram:
1. Kiểm tra syntax PlantUML
2. Verify file paths trong prompt
3. Reference FeatG7_AuthLogin.puml example
4. Check PlantUML extension đã install chưa
