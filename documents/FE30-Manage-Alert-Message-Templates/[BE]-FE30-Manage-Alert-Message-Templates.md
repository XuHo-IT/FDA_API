# FE-30: Manage Alert Message Templates

## A) Nghiệp vụ & Scope

**Mục tiêu:** Cho phép Admin tạo/chỉnh sửa các template tin nhắn cảnh báo ngập lụt thay vì hardcode như hiện tại.

**Nghiệp vụ hiện tại (problem):**
- `NotificationTemplateService` hiện tại hardcode message templates
- Không thể tùy chỉnh nội dung theo nhu cầu
- Không hỗ trợ đa ngôn ngữ (i18n)

**Scope FE-30:**
1. Tạo entity `AlertTemplate` lưu template trong DB
2. CRUD APIs để quản lý templates (Admin only)
3. Template engine hỗ trợ variables
4. Preview template với sample data
5. Integration với notification dispatch

---

## B) Data/Entities

### Entity: AlertTemplate

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| Name | string | e.g., "Critical Push Template" |
| Channel | string | Push, Email, SMS, InApp |
| Severity | string? | info, caution, warning, critical (null = all) |
| TitleTemplate | string | Template for title |
| BodyTemplate | string | Template for body |
| IsActive | bool | Default: true |
| SortOrder | int | Display order |
| CreatedBy | Guid | Audit |
| CreatedAt | DateTime | Audit |
| UpdatedBy | Guid | Audit |
| UpdatedAt | DateTime | Audit |

### Supported Variables:
| Variable | Description |
|----------|-------------|
| `{{station_name}}` | Tên trạm |
| `{{water_level}}` | Mức nước + đơn vị |
| `{{water_level_raw}}` | Chỉ số |
| `{{severity}}` | Mức độ |
| `{{time}}` | Thời gian |
| `{{threshold}}` | Ngưỡng cảnh báo |
| `{{address}}` | Địa chỉ trạm |
| `{{message}}` | Alert message |

---

## C) API Contract

### Routes:

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/v1/admin/alert-templates` | Admin | Create template |
| GET | `/api/v1/admin/alert-templates` | Admin | List all templates |
| GET | `/api/v1/admin/alert-templates/{id}` | Admin | Get template by ID |
| PUT | `/api/v1/admin/alert-templates/{id}` | Admin | Update template |
| DELETE | `/api/v1/admin/alert-templates/{id}` | Admin | Delete template |
| POST | `/api/v1/admin/alert-templates/{id}/preview` | Admin | Preview rendered message |

---

## D) Plan triển khai

### FeatG100: AlertTemplateCreate - Create & List
- [x] Entity `AlertTemplate.cs`
- [x] Repository `IAlertTemplateRepository.cs`
- [x] DbSet in AppDbContext
- [x] Migration
- [x] CreateAlertTemplateHandler + Request
- [x] GetAlertTemplatesHandler + Request

### FeatG101: AlertTemplateUpdate - Update
- [x] UpdateAlertTemplateHandler + Request

### FeatG102: AlertTemplateDelete - Delete
- [x] DeleteAlertTemplateHandler + Request

### FeatG103: AlertTemplateGet - Get By ID
- [x] GetAlertTemplateByIdHandler + Request

### FeatG104: AlertTemplatePreview - Preview
- [ ] PreviewAlertTemplateHandler + Request

### Presentation Layer:
- [x] CreateAlertTemplateEndpoint + DTOs
- [x] GetAlertTemplatesEndpoint + DTOs
- [x] UpdateAlertTemplateEndpoint + DTOs
- [x] DeleteAlertTemplateEndpoint + DTOs
- [x] GetAlertTemplateByIdEndpoint + DTOs
- [ ] PreviewAlertTemplateEndpoint + DTOs

### Template Rendering:
- [ ] Create ITemplateRenderService
- [ ] Modify NotificationTemplateService để ưu tiên load từ DB

---

## E) File Structure (tách theo style)

```
src/Core/Application/
├── FDAAPI.App.FeatG100_AlertTemplateCreate/
│   ├── CreateAlertTemplateRequest.cs
│   ├── CreateAlertTemplateHandler.cs
│   ├── CreateAlertTemplateResponse.cs
│   ├── GetAlertTemplatesRequest.cs
│   ├── GetAlertTemplatesHandler.cs
│   ├── GetAlertTemplatesResponse.cs
│   └── FDAAPI.App.FeatG100_AlertTemplateCreate.csproj
│
├── FDAAPI.App.FeatG101_AlertTemplateUpdate/
│   ├── UpdateAlertTemplateRequest.cs
│   ├── UpdateAlertTemplateHandler.cs
│   ├── UpdateAlertTemplateResponse.cs
│   └── FDAAPI.App.FeatG101_AlertTemplateUpdate.csproj
│
├── FDAAPI.App.FeatG102_AlertTemplateDelete/
│   ├── DeleteAlertTemplateRequest.cs
│   ├── DeleteAlertTemplateHandler.cs
│   ├── DeleteAlertTemplateResponse.cs
│   └── FDAAPI.App.FeatG102_AlertTemplateDelete.csproj
│
├── FDAAPI.App.FeatG103_AlertTemplateGet/
│   ├── GetAlertTemplateByIdRequest.cs
│   ├── GetAlertTemplateByIdHandler.cs
│   ├── GetAlertTemplateByIdResponse.cs
│   └── FDAAPI.App.FeatG103_AlertTemplateGet.csproj
│
└── FDAAPI.App.FeatG104_AlertTemplatePreview/
    ├── PreviewAlertTemplateRequest.cs
    ├── PreviewAlertTemplateHandler.cs
    ├── PreviewAlertTemplateResponse.cs
    └── FDAAPI.App.FeatG104_AlertTemplatePreview.csproj

src/External/Presentation/FDAAPI.Presentation.FastEndpointBasedApi/Endpoints/
├── FeatG100_AlertTemplateCreate/
│   ├── CreateAlertTemplateEndpoint.cs
│   ├── GetAlertTemplatesEndpoint.cs
│   └── DTOs/
├── FeatG101_AlertTemplateUpdate/
│   ├── UpdateAlertTemplateEndpoint.cs
│   └── DTOs/
├── FeatG102_AlertTemplateDelete/
│   ├── DeleteAlertTemplateEndpoint.cs
│   └── DTOs/
├── FeatG103_AlertTemplateGet/
│   ├── GetAlertTemplateByIdEndpoint.cs
│   └── DTOs/
└── FeatG104_AlertTemplatePreview/
    ├── PreviewAlertTemplateEndpoint.cs
    └── DTOs/
```

---

## F) Test Cases

| Test Case | Flow | Expected |
|-----------|------|----------|
| Create template | POST with valid data | 201 Created |
| Create invalid | POST with empty body | 400 Bad Request |
| Update template | PUT with new content | 200 OK |
| Delete template | DELETE existing | 200 OK |
| Get by ID | GET /{id} | 200 OK |
| List templates | GET / | List of templates |
| Preview | POST preview request | Rendered message |
| Fallback | No template found | Hardcoded message |
