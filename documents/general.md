# FDA sẽ áp dụng kiến trúc Domain-Centric

<aside>
📢

**Domain-Centric**(Domain-Driven Architecture) nghĩa là **mọi thứ trong hệ thống xoay quanh Domain (nghiệp vụ)**.

Toàn bộ kiến trúc được thiết kế sao cho:

- **Nghiệp vụ (Domain Logic) là trung tâm**
- UI, database, framework, API… chỉ là phần “vỏ” bao quanh
- Domain không phụ thuộc vào bất kỳ công nghệ, thư viện, hay database nào
- Ứng dụng dễ bảo trì, mở rộng, kiểm thử đơn vị (unit test)
- **Tách biệt các mối quan tâm**: Ranh giới rõ ràng giữa logic nghiệp vụ, quy trình làm việc của ứng dụng và cơ sở hạ tầng
- **Khả năng kiểm thử**: Logic nghiệp vụ có thể được kiểm thử độc lập mà không cần cơ sở dữ liệu hoặc API
- **Khả năng mở rộng**: Dễ dàng thêm các tính năng mới mà không ảnh hưởng đến mã hiện có
- **Khả năng bảo trì**: Công nghệ có thể được thay đổi (ví dụ: PostgreSQL → MySQL) mà không cần thay đổi logic nghiệp vụ
- **Tổ chức tính năng**: Mã được tổ chức theo tính năng, giúp dễ dàng tìm kiếm và hiểu
</aside>

# Lý do áp dụng Kiến trúc này

1. **Tách biệt mối quan tâm (Separation of Concerns):**

- **Domain:** Tập trung vào các quy tắc nghiệp vụ và mô hình dữ liệu (Entity , Repository interface ).
- **Application:** Tập trung vào luồng công việc (workflow) và điều phối các tác vụ (Handler logic ).
- **Infrastructure:** Tập trung vào các chi tiết kỹ thuật (PostgreSQL, Redis, Cloudinary, SignalR) (triển khai chi tiết ).
- **Presentation:** Tập trung vào giao tiếp HTTP (FastEndpoints ).

1. **Khả năng kiểm thử (Testability):** Logic nghiệp vụ có thể được kiểm thử độc lập mà không cần khởi tạo cơ sở dữ liệu hoặc API.
2. **Khả năng mở rộng (Scalability & Extensibility):**

- Thêm một tính năng (Feature, ví dụ `Func1`) mới chỉ cần thêm một module mới, giảm thiểu sự ảnh hưởng đến các phần khác.
- Có thể dễ dàng thay thế công nghệ hạ tầng (ví dụ: chuyển từ PostgreSQL sang MySQL) mà không ảnh hưởng đến lớp `Core`.

1. **Tổ chức theo tính năng (Feature Organization):** Việc nhóm các file theo tính năng (như `Func1`, `Func3`) giúp dễ dàng tìm kiếm, hiểu và phát triển một tính năng cụ thể.

# Các tầng sẽ được sắp xếp như sau:

1. Core: Chứa logic nghiệp vụ cốt lõi và định nghĩa miền (Domain).

- Domain: Entity, Constraints, **Repositories, Unit of Works**
- Application: Request, Response[Áp dụng ***CQRS*** - là một *mẫu kiến trúc* được sử dụng để **tách rõ** hai loại hành động trong hệ thống: **Command** → thao tác *ghi* (create, update, delete), **Query** → thao tác *đọc* (get, list, search)]

1. External: Chứa các triển khai cụ thể cho các giao diện được định nghĩa trong lớp `Core`, bao gồm các dịch vụ hạ tầng và giao diện người dùng.

- Registration/DI (Building Block): Chứa logic đăng ký tự động các **Feature Handler,Repositories, và Unit of Works.**
- Infrastructure: Cung cấp các triển khai thực tế:

                Postgres,

                Redis Caching(Sử dụng cho việc lưu trữ dữ liệu tạm thời (cache) để tăng tốc độ phản hồi API.),

               Typesense(Công cụ tìm kiếm hiệu suất cao, giúp tối ưu hóa các truy vấn tìm kiếm phức tạp.),

               Quartz(Quản lý các **Background Jobs** (Tác vụ nền) theo lịch trình (ví dụ: gửi mail, cập nhật dữ liệu định kỳ),

                SignalR

                CloudinaryService(dành cho video) - ImageKit (dành cho ảnh sẽ tối ưu hơn)

- Presentation: Lớp đầu tiên tiếp nhận yêu cầu HTTP, sử dụng framework **FastEndpoints**

# Workflow:

1. **Presentation (FastEndpoints):** Nhận **HTTP Request** (ví dụ: POST `/api/v1/Func1/create`).

- Thực hiện **Authorization** và **Validation** (kiểm tra dữ liệu đầu vào).
- Chuyển đổi HTTP Request thành một **Application Request** (ví dụ: `Func1Request`) và gửi đến lớp Application.

1. **Application (Handlers):**

- **Handler** (`Func1Request`) nhận `Func1Request`.
- Thực thi **Logic nghiệp vụ** (ví dụ: kiểm tra quyền creator, tính toán data).
- Sử dụng **Unit of Work** và **Repository** để tương tác với lớp Domain.
- Sử dụng **Infrastructure Services** (File, Cache, Realtime) khi cần thiết.

1. **Domain (Persistence/Dữ liệu):**

- **Repository** thực thi các thao tác CRUD (Create, Read, Update, Delete) bằng cách giao tiếp với tầng Infrastructure (ví dụ: `AppDbContext` cho PostgreSQL).
- Đảm bảo các quy tắc nghiệp vụ (Constraints) trên các **Entity** được tuân thủ.

1. **Presentation (FastEndpoints):**

- Nhận **Application Response** (ví dụ: `Func1Reponse`).
- Chuyển đổi thành **HTTP Response** (JSON) và trả về cho client.
