# FE-24: Giải thích tính năng mới

FE-24 thêm **3 tính năng mới** so với FE-21 hiện tại:

---

## 1. Waypoints (điểm trung gian)

**FE-21 hiện tại**: Chỉ có điểm A → điểm B (2 điểm).

**FE-24 thêm**: Điểm A → điểm C → điểm D → điểm B (qua nhiều điểm trung gian).

Ví dụ thực tế: Người dùng muốn đi từ nhà → ghé siêu thị → ghé trường học → đến công ty, nhưng vẫn tránh vùng ngập.

```
FE-21: [nhà] ────────────────────→ [công ty]
FE-24: [nhà] → [siêu thị] → [trường] → [công ty]
```

GraphHopper đã hỗ trợ sẵn multi-point routing qua `points` array, nên chỉ cần truyền thêm tọa độ.

**Lưu ý**: Khi có waypoints, GraphHopper không hỗ trợ `alternative_route` → chỉ gọi 2 requests (safe + shortest) thay vì 3.

---

## 2. Departure Time + Flood Trend (dự đoán xu hướng ngập)

**FE-21 hiện tại**: Lấy sensor reading **mới nhất** → tính severity → tạo polygon. Chỉ phản ánh **hiện tại**.

**FE-24 thêm**: Nếu user cung cấp `departureTime` (ví dụ "tôi sẽ đi lúc 14:00"), hệ thống sẽ:

- Query **3 readings gần nhất** của mỗi station
- Tính xu hướng: nước đang **tăng** hay **giảm**?
- Nếu tăng > 10% → tăng severity lên 1 bậc (warning → critical)

```
Station A - 3 readings gần nhất:
  12:00 → 25cm
  12:30 → 28cm
  13:00 → 32cm  ← đang TĂNG (+28%)
  → Severity hiện tại: warning (level 2)
  → Projected severity: critical (level 3) ← tăng 1 bậc
  → Polygon radius: 150m thay vì 100m
```

Mục đích: Tránh tình huống người dùng khởi hành 30 phút sau, nhưng lúc đến nơi thì nước đã dâng cao hơn.

---

## 3. Response Caching (bộ nhớ đệm)

**FE-21 hiện tại**: Mỗi request luôn gọi GraphHopper 3 lần + query database → chậm.

**FE-24 thêm**: Cache kết quả trong `IMemoryCache` (bộ nhớ trong, built-in ASP.NET Core):

- Request đầu tiên: gọi GraphHopper + DB → lưu kết quả vào cache (TTL 5 phút)
- Request giống hệt trong 5 phút tiếp theo: trả kết quả từ cache ngay lập tức

```
Request 1 (cold):  User A yêu cầu route X→Y  → gọi GraphHopper → ~2s → cache
Request 2 (warm):  User B yêu cầu route X→Y  → trả từ cache    → ~50ms
Request 3 (warm):  User A yêu cầu route X→Y  → trả từ cache    → ~50ms
...5 phút sau cache hết hạn...
Request 4 (cold):  User C yêu cầu route X→Y  → gọi GraphHopper lại → ~2s
```

Cache key = hash của tọa độ + profile + departureTime (rounded to 5 phút), nên cùng input → cùng output = **deterministic** (consistency test).

---

## Tóm lại

| # | Tính năng | FE-21 (hiện tại) | FE-24 (thêm mới) |
|---|-----------|-------------------|-------------------|
| 1 | Route points | 2 điểm (A→B) | 2-7 điểm (A→waypoints→B) |
| 2 | Flood data | Chỉ hiện tại | Hiện tại + dự đoán xu hướng |
| 3 | Performance | Luôn gọi API mới | Cache 5 phút, response < 100ms |
