# web_do_an1

Ứng dụng quản lý trung tâm tiếng Anh được xây dựng bằng ASP.NET Core MVC,
Entity Framework Core và SQL Server theo hướng Code First.

## Cấu trúc

- `Models`: các entity có tên file trùng hoàn toàn với tên class.
- `Data/EnglishCenterDbContext.cs`: DbContext và toàn bộ cấu hình quan hệ.
- `Data/DatabaseSeeder.cs`: dữ liệu mẫu được tạo sau khi migration hoàn tất.
- `Controllers`: mỗi entity có một controller riêng; chức năng theo vai trò nằm trong
  controller của đúng entity đó.
- `Views`: dùng trực tiếp entity hoặc `ViewBag`, không tạo lớp trung gian.
- `Migrations`: migration Code First khởi tạo schema tiếng Anh.
- Không sử dụng thư mục `ViewModels`; view nhận entity trực tiếp hoặc dữ liệu đơn giản từ `ViewBag`.

## Entity và controller

| Entity | Controller |
| --- | --- |
| `Role` | `RolesController` |
| `UserAccount` | `UserAccountsController` |
| `Student` | `StudentsController` |
| `Teacher` | `TeachersController` |
| `Course` | `CoursesController` |
| `CourseClass` | `CourseClassesController` |
| `Enrollment` | `EnrollmentsController` |
| `Payment` | `PaymentsController` |
| `PaymentTransaction` | `PaymentTransactionsController` |
| `Score` | `ScoresController` |
| `AttendanceRecord` | `AttendanceRecordsController` |
| `CourseLecture` | `CourseLecturesController` |
| `SavedCourse` | `SavedCoursesController` |

## Chạy project

```powershell
dotnet restore .\web_do_an1.csproj
dotnet run --project .\web_do_an1.csproj
```

Ứng dụng tự chạy migration và seed dữ liệu khi khởi động. Connection string
`DefaultConnection` nằm trong `appsettings.json` và mặc định sử dụng SQL Server
LocalDB với database `EnglishCenterCodeFirstDb`.

Tài khoản quản trị mẫu:

- Tên đăng nhập: `admin`
- Mật khẩu: `123456`

Các tài khoản mẫu:

- Quản trị viên: `admin`
- Nhân viên đào tạo: `nvdt`
- Giáo viên: `gv01` đến `gv10`
- Học viên: `st01` đến `st20`

Tất cả tài khoản mẫu dùng mật khẩu cố định `123456`.

## Chức năng theo vai trò

- Quản trị viên: tổng quan, khóa học, tài khoản, vai trò và các danh mục hệ thống.
- Nhân viên đào tạo: thống kê vận hành, học viên, lớp học, duyệt đăng ký và học phí.
- Giáo viên: lớp phụ trách, danh sách học viên, điểm danh, điểm số, nhận xét và bài giảng.
- Học viên: tìm/lưu/đăng ký khóa học, lịch học, điểm, học phí, bài giảng và hồ sơ.
- Khách: xem khóa học, lịch khai giảng, chi tiết khóa học, đăng ký và đăng nhập.

Mật khẩu đang được lưu dưới dạng văn bản thuần theo yêu cầu của bản demo; không sử
dụng cách này khi triển khai hệ thống thực tế.

## Tạo migration mới

```powershell
dotnet tool install dotnet-ef --tool-path .tools --version 10.0.9
.\.tools\dotnet-ef.exe migrations add MigrationName --project .\web_do_an1.csproj
.\.tools\dotnet-ef.exe database update --project .\web_do_an1.csproj
```
