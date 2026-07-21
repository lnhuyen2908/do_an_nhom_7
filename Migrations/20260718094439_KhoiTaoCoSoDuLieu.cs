using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace web_do_an1.Migrations
{
    /// <inheritdoc />
    public partial class KhoiTaoCoSoDuLieu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GiaoVien",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Specialty = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiaoVien", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HocVien",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HocVien", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KhoaHoc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Level = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tuition = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Duration = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhoaHoc", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VaiTro",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaiTro", x => x.Id);
                    table.UniqueConstraint("AK_VaiTro_Name", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "BaiGiang",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaiGiang", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaiGiang_GiaoVien_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "GiaoVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BaiGiang_KhoaHoc_CourseId",
                        column: x => x.CourseId,
                        principalTable: "KhoaHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KhoaHocDaLuu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    SavedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhoaHocDaLuu", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KhoaHocDaLuu_HocVien_StudentId",
                        column: x => x.StudentId,
                        principalTable: "HocVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KhoaHocDaLuu_KhoaHoc_CourseId",
                        column: x => x.CourseId,
                        principalTable: "KhoaHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LopHoc",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    Room = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StudyTime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LopHoc", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LopHoc_GiaoVien_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "GiaoVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LopHoc_KhoaHoc_CourseId",
                        column: x => x.CourseId,
                        principalTable: "KhoaHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LinkedId = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaiKhoan_VaiTro_Role",
                        column: x => x.Role,
                        principalTable: "VaiTro",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DangKy",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    ClassId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DangKy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DangKy_HocVien_StudentId",
                        column: x => x.StudentId,
                        principalTable: "HocVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DangKy_KhoaHoc_CourseId",
                        column: x => x.CourseId,
                        principalTable: "KhoaHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DangKy_LopHoc_ClassId",
                        column: x => x.ClassId,
                        principalTable: "LopHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DiemDanh",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    StudyDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsPresent = table.Column<bool>(type: "bit", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiemDanh", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiemDanh_HocVien_StudentId",
                        column: x => x.StudentId,
                        principalTable: "HocVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DiemDanh_LopHoc_ClassId",
                        column: x => x.ClassId,
                        principalTable: "LopHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DiemSo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    Midterm = table.Column<double>(type: "float", nullable: false),
                    Final = table.Column<double>(type: "float", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiemSo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiemSo_HocVien_StudentId",
                        column: x => x.StudentId,
                        principalTable: "HocVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DiemSo_LopHoc_ClassId",
                        column: x => x.ClassId,
                        principalTable: "LopHoc",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HocPhi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    EnrollmentId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HocPhi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HocPhi_DangKy_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "DangKy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HocPhi_HocVien_StudentId",
                        column: x => x.StudentId,
                        principalTable: "HocVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LichSuThanhToan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RecordedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuThanhToan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LichSuThanhToan_HocPhi_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "HocPhi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LichSuThanhToan_HocVien_StudentId",
                        column: x => x.StudentId,
                        principalTable: "HocVien",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "GiaoVien",
                columns: new[] { "Id", "Code", "Email", "FullName", "Phone", "Specialty" },
                values: new object[,]
                {
                    { 1, "GV01", "minhanh@englishcenter.vn", "Nguyễn Minh Anh", "0901000001", "IELTS" },
                    { 2, "GV02", "quocbao@englishcenter.vn", "Trần Quốc Bảo", "0901000002", "Giao tiếp" },
                    { 3, "GV03", "thuha@englishcenter.vn", "Lê Thu Hà", "0901000003", "Ngữ pháp" },
                    { 4, "GV04", "duclong@englishcenter.vn", "Phạm Đức Long", "0901000004", "TOEIC" },
                    { 5, "GV05", "maiphuong@englishcenter.vn", "Hoàng Mai Phương", "0901000005", "IELTS Writing" },
                    { 6, "GV06", "thanhson@englishcenter.vn", "Võ Thanh Sơn", "0901000006", "IELTS Speaking" },
                    { 7, "GV07", "ngoclan@englishcenter.vn", "Đặng Ngọc Lan", "0901000007", "IELTS Advanced" },
                    { 8, "GV08", "quanghung@englishcenter.vn", "Bùi Quang Hưng", "0901000008", "TOEIC Listening" },
                    { 9, "GV09", "khanhvy@englishcenter.vn", "Đỗ Khánh Vy", "0901000009", "TOEIC Reading" },
                    { 10, "GV10", "anhtuan@englishcenter.vn", "Hồ Anh Tuấn", "0901000010", "Business English" },
                    { 11, "GV11", "baotram@englishcenter.vn", "Nguyễn Bảo Trâm", "0901000011", "Kids Starter" },
                    { 12, "GV12", "giahan@englishcenter.vn", "Trần Gia Hân", "0901000012", "Kids Movers" },
                    { 13, "GV13", "nhatminh@englishcenter.vn", "Lê Nhật Minh", "0901000013", "Teen English" },
                    { 14, "GV14", "hongnhung@englishcenter.vn", "Phạm Hồng Nhung", "0901000014", "Pronunciation" },
                    { 15, "GV15", "minhquan@englishcenter.vn", "Hoàng Minh Quân", "0901000015", "Grammar Foundation" },
                    { 16, "GV16", "thuylinh@englishcenter.vn", "Võ Thùy Linh", "0901000016", "Academic Writing" },
                    { 17, "GV17", "hainam@englishcenter.vn", "Đặng Hải Nam", "0901000017", "Speaking Club" },
                    { 18, "GV18", "phuonganh@englishcenter.vn", "Bùi Phương Anh", "0901000018", "Listening" },
                    { 19, "GV19", "viethoang@englishcenter.vn", "Đỗ Việt Hoàng", "0901000019", "Reading" },
                    { 20, "GV20", "ngocdiep@englishcenter.vn", "Hồ Ngọc Diệp", "0901000020", "English for Travel" }
                });

            migrationBuilder.InsertData(
                table: "HocVien",
                columns: new[] { "Id", "Address", "Code", "DateOfBirth", "Email", "FullName", "Phone" },
                values: new object[,]
                {
                    { 1, "TP.HCM", "HV01", new DateTime(2004, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv01@englishcenter.vn", "Nguyễn Hoàng Anh", "0912000001" },
                    { 2, "TP.HCM", "HV02", new DateTime(2004, 2, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv02@englishcenter.vn", "Trần Minh Châu", "0912000002" },
                    { 3, "TP.HCM", "HV03", new DateTime(2004, 3, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv03@englishcenter.vn", "Lê Gia Bảo", "0912000003" },
                    { 4, "TP.HCM", "HV04", new DateTime(2004, 4, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv04@englishcenter.vn", "Phạm Khánh Linh", "0912000004" },
                    { 5, "TP.HCM", "HV05", new DateTime(2004, 5, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv05@englishcenter.vn", "Hoàng Đức Anh", "0912000005" },
                    { 6, "TP.HCM", "HV06", new DateTime(2004, 6, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv06@englishcenter.vn", "Võ Ngọc Mai", "0912000006" },
                    { 7, "TP.HCM", "HV07", new DateTime(2004, 7, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv07@englishcenter.vn", "Đặng Quang Huy", "0912000007" },
                    { 8, "TP.HCM", "HV08", new DateTime(2004, 8, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv08@englishcenter.vn", "Bùi Thu Trang", "0912000008" },
                    { 9, "TP.HCM", "HV09", new DateTime(2004, 9, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv09@englishcenter.vn", "Đỗ Minh Khang", "0912000009" },
                    { 10, "TP.HCM", "HV10", new DateTime(2004, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv10@englishcenter.vn", "Hồ Phương Thảo", "0912000010" },
                    { 11, "TP.HCM", "HV11", new DateTime(2004, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv11@englishcenter.vn", "Nguyễn Tuấn Kiệt", "0912000011" },
                    { 12, "TP.HCM", "HV12", new DateTime(2004, 12, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv12@englishcenter.vn", "Trần Hải Yến", "0912000012" },
                    { 13, "TP.HCM", "HV13", new DateTime(2004, 1, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv13@englishcenter.vn", "Lê Nhật Nam", "0912000013" },
                    { 14, "TP.HCM", "HV14", new DateTime(2004, 2, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv14@englishcenter.vn", "Phạm Bảo Ngọc", "0912000014" },
                    { 15, "TP.HCM", "HV15", new DateTime(2004, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv15@englishcenter.vn", "Hoàng Anh Thư", "0912000015" },
                    { 16, "TP.HCM", "HV16", new DateTime(2004, 4, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv16@englishcenter.vn", "Võ Quốc Khánh", "0912000016" },
                    { 17, "TP.HCM", "HV17", new DateTime(2004, 5, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv17@englishcenter.vn", "Đặng Thanh Hà", "0912000017" },
                    { 18, "TP.HCM", "HV18", new DateTime(2004, 6, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv18@englishcenter.vn", "Bùi Gia Hân", "0912000018" },
                    { 19, "TP.HCM", "HV19", new DateTime(2004, 7, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv19@englishcenter.vn", "Đỗ Thành Đạt", "0912000019" },
                    { 20, "TP.HCM", "HV20", new DateTime(2004, 8, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv20@englishcenter.vn", "Hồ Minh Thư", "0912000020" },
                    { 21, "TP.HCM", "HV21", new DateTime(2004, 9, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv21@englishcenter.vn", "Nguyễn Đức Minh", "0912000021" },
                    { 22, "TP.HCM", "HV22", new DateTime(2004, 10, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv22@englishcenter.vn", "Trần Ngọc Ánh", "0912000022" },
                    { 23, "TP.HCM", "HV23", new DateTime(2004, 11, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv23@englishcenter.vn", "Lê Hoài Phương", "0912000023" },
                    { 24, "TP.HCM", "HV24", new DateTime(2004, 12, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv24@englishcenter.vn", "Phạm Công Thành", "0912000024" },
                    { 25, "TP.HCM", "HV25", new DateTime(2004, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv25@englishcenter.vn", "Hoàng Thùy Dương", "0912000025" },
                    { 26, "TP.HCM", "HV26", new DateTime(2004, 2, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv26@englishcenter.vn", "Võ Minh Triết", "0912000026" },
                    { 27, "TP.HCM", "HV27", new DateTime(2004, 3, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv27@englishcenter.vn", "Đặng Mai Anh", "0912000027" },
                    { 28, "TP.HCM", "HV28", new DateTime(2004, 4, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv28@englishcenter.vn", "Bùi Quốc Việt", "0912000028" },
                    { 29, "TP.HCM", "HV29", new DateTime(2004, 5, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv29@englishcenter.vn", "Đỗ Khánh Vy", "0912000029" },
                    { 30, "TP.HCM", "HV30", new DateTime(2004, 6, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv30@englishcenter.vn", "Hồ Gia Khiêm", "0912000030" },
                    { 31, "TP.HCM", "HV31", new DateTime(2004, 7, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv31@englishcenter.vn", "Nguyễn Phương Nhi", "0912000031" },
                    { 32, "TP.HCM", "HV32", new DateTime(2004, 8, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv32@englishcenter.vn", "Trần Đình Phúc", "0912000032" },
                    { 33, "TP.HCM", "HV33", new DateTime(2004, 9, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv33@englishcenter.vn", "Lê Ngọc Hân", "0912000033" },
                    { 34, "TP.HCM", "HV34", new DateTime(2004, 10, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv34@englishcenter.vn", "Phạm Trung Kiên", "0912000034" },
                    { 35, "TP.HCM", "HV35", new DateTime(2004, 11, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv35@englishcenter.vn", "Hoàng Bích Ngọc", "0912000035" },
                    { 36, "TP.HCM", "HV36", new DateTime(2004, 12, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv36@englishcenter.vn", "Võ Anh Khoa", "0912000036" },
                    { 37, "TP.HCM", "HV37", new DateTime(2004, 1, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv37@englishcenter.vn", "Đặng Thu Uyên", "0912000037" },
                    { 38, "TP.HCM", "HV38", new DateTime(2004, 2, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv38@englishcenter.vn", "Bùi Minh Quân", "0912000038" },
                    { 39, "TP.HCM", "HV39", new DateTime(2004, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv39@englishcenter.vn", "Đỗ Hải Anh", "0912000039" },
                    { 40, "TP.HCM", "HV40", new DateTime(2004, 4, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv40@englishcenter.vn", "Hồ Thanh Trúc", "0912000040" },
                    { 41, "TP.HCM", "HV41", new DateTime(2004, 5, 17, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv41@englishcenter.vn", "Nguyễn Quỳnh Như", "0912000041" },
                    { 42, "TP.HCM", "HV42", new DateTime(2004, 6, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv42@englishcenter.vn", "Trần Gia Huy", "0912000042" },
                    { 43, "TP.HCM", "HV43", new DateTime(2004, 7, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv43@englishcenter.vn", "Lê Mỹ Linh", "0912000043" },
                    { 44, "TP.HCM", "HV44", new DateTime(2004, 8, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv44@englishcenter.vn", "Phạm Quốc Bảo", "0912000044" },
                    { 45, "TP.HCM", "HV45", new DateTime(2004, 9, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "hv45@englishcenter.vn", "Hoàng Ngọc Diệp", "0912000045" }
                });

            migrationBuilder.InsertData(
                table: "KhoaHoc",
                columns: new[] { "Id", "Code", "Description", "Duration", "ImageUrl", "Level", "Name", "Tuition" },
                values: new object[,]
                {
                    { 1, "KH01", "Khóa học nền tảng phát âm, từ vựng và ngữ pháp cơ bản.", "8 tuần", "/images/courses/general.jpg", "A1", "English Basic", 1500000m },
                    { 2, "KH02", "Rèn luyện phản xạ nghe nói trong tình huống hằng ngày.", "10 tuần", "/images/courses/communication.jpg", "A2", "Giao tiếp cơ bản", 2000000m },
                    { 3, "KH03", "Làm quen IELTS Listening, Reading, Writing và Speaking.", "12 tuần", "/images/courses/ielts.jpg", "B1", "IELTS Foundation", 3500000m },
                    { 4, "KH04", "Hệ thống kiến thức trọng tâm cho mục tiêu TOEIC 450+.", "10 tuần", "/images/courses/toeic.jpg", "TOEIC", "TOEIC 450+", 2800000m },
                    { 5, "KH05", "Lộ trình tăng tốc cho học viên cần đạt IELTS 5.5 với chiến lược làm bài theo từng kỹ năng.", "12 tuần", "/images/courses/ielts.jpg", "IELTS", "IELTS Level 5.5", 4200000m },
                    { 6, "KH06", "Khóa học phát triển tư duy học thuật và kỹ năng xử lý đề IELTS mục tiêu 6.5.", "14 tuần", "/images/courses/ielts.jpg", "IELTS", "IELTS Level 6.5", 5200000m },
                    { 7, "KH07", "Luyện đề nâng cao, tối ưu band điểm Writing và Speaking cho mục tiêu IELTS 7.5.", "16 tuần", "/images/courses/ielts.jpg", "IELTS", "IELTS Level 7.5", 6800000m },
                    { 8, "KH08", "Củng cố ngữ pháp, từ vựng và chiến thuật làm bài cho mục tiêu TOEIC 650+.", "12 tuần", "/images/courses/toeic.jpg", "TOEIC", "TOEIC 650+", 3600000m },
                    { 9, "KH09", "Khóa học giải đề chuyên sâu, tăng tốc độ nghe đọc và xử lý bẫy đáp án.", "12 tuần", "/images/courses/toeic.jpg", "TOEIC", "TOEIC 750+", 4600000m },
                    { 10, "KH10", "Tiếng Anh công việc: email, họp, thuyết trình và giao tiếp với đối tác.", "10 tuần", "/images/courses/toeic.jpg", "B1", "Business English", 3900000m },
                    { 11, "KH11", "Lớp tiếng Anh trẻ em với hoạt động nghe nói, từ vựng và phản xạ ngôn ngữ tự nhiên.", "8 tuần", "/images/courses/kids.jpg", "Kids", "English for Kids Starter", 2400000m },
                    { 12, "KH12", "Mở rộng từ vựng và cấu trúc giao tiếp cho học viên nhỏ tuổi đã có nền tảng.", "8 tuần", "/images/courses/kids.jpg", "Kids", "English for Kids Movers", 2700000m },
                    { 13, "KH13", "Tiếng Anh thiếu niên theo chủ đề học tập, đời sống và thuyết trình ngắn.", "10 tuần", "/images/courses/kids.jpg", "A2", "English for Teens", 3100000m },
                    { 14, "KH14", "Sửa âm, trọng âm, nối âm và ngữ điệu để nói tiếng Anh rõ ràng hơn.", "6 tuần", "/images/courses/communication.jpg", "A2", "Pronunciation Mastery", 2200000m },
                    { 15, "KH15", "Hệ thống ngữ pháp nền tảng cho người mất gốc hoặc cần ôn lại từ đầu.", "6 tuần", "/images/courses/general.jpg", "A1", "Grammar Foundation", 1800000m },
                    { 16, "KH16", "Rèn cấu trúc bài viết học thuật, lập luận, liên kết ý và sửa lỗi diễn đạt.", "10 tuần", "/images/courses/general.jpg", "B2", "Academic Writing", 4300000m },
                    { 17, "KH17", "Thực hành nói theo chủ đề với giáo viên, tăng phản xạ và sự tự tin.", "4 tuần", "/images/courses/communication.jpg", "A2", "Speaking Club", 1600000m },
                    { 18, "KH18", "Luyện nghe ý chính, chi tiết và ghi chú nhanh qua nhiều giọng nói.", "6 tuần", "/images/courses/general.jpg", "B1", "Listening Booster", 2600000m },
                    { 19, "KH19", "Tăng tốc đọc hiểu, scanning, skimming và xử lý câu hỏi từ vựng.", "6 tuần", "/images/courses/general.jpg", "B1", "Reading Comprehension", 2600000m },
                    { 20, "KH20", "Tiếng Anh du lịch cho sân bay, khách sạn, nhà hàng và hỏi đường.", "5 tuần", "/images/courses/general.jpg", "A2", "English for Travel", 2100000m }
                });

            migrationBuilder.InsertData(
                table: "VaiTro",
                columns: new[] { "Id", "DisplayName", "Name" },
                values: new object[,]
                {
                    { 1, "Quản trị viên", "Admin" },
                    { 2, "Giáo viên", "Teacher" },
                    { 3, "Học viên", "Student" },
                    { 4, "Nhân viên đào tạo", "Staff" }
                });

            migrationBuilder.InsertData(
                table: "DangKy",
                columns: new[] { "Id", "ClassId", "CourseId", "RegisteredAt", "Status", "StudentId" },
                values: new object[] { 3, null, 2, new DateTime(2026, 7, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "ChoDuyet", 3 });

            migrationBuilder.InsertData(
                table: "LopHoc",
                columns: new[] { "Id", "Capacity", "Code", "CourseId", "Room", "StartDate", "StudyTime", "TeacherId" },
                values: new object[,]
                {
                    { 1, 24, "LH01", 1, "P101", new DateTime(2026, 7, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Thứ 2-4-6, 18:00-19:30", 1 },
                    { 2, 20, "LH02", 3, "P203", new DateTime(2026, 7, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Thứ 3-5, 19:00-21:00", 3 },
                    { 3, 22, "LH03", 2, "P102", new DateTime(2026, 7, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), "Thứ 7-CN, 08:00-10:00", 2 },
                    { 4, 24, "LH04", 4, "P204", new DateTime(2026, 7, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Thứ 2-4, 18:00-19:30", 4 },
                    { 5, 20, "LH05", 5, "P205", new DateTime(2026, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Thứ 3-5, 19:00-20:30", 5 },
                    { 6, 30, "LH06", 6, "Online Zoom 01", new DateTime(2026, 7, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Thứ 2-CN, 18:00-19:30, 20:00-21:30", 6 },
                    { 7, 18, "LH07", 7, "P301", new DateTime(2026, 7, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Thứ 7-CN, 09:00-10:30, 14:00-15:30", 7 },
                    { 8, 28, "LH08", 8, "P103", new DateTime(2026, 7, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "Thứ 2-4-6, 18:00-19:30", 8 },
                    { 9, 30, "LH09", 9, "Online Zoom 02", new DateTime(2026, 7, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), "Thứ 3-5, 20:00-21:30", 9 },
                    { 10, 22, "LH10", 10, "P401", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Thứ 2-4, 19:30-21:00", 10 },
                    { 11, 16, "LH11", 11, "PKids 01", new DateTime(2026, 8, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Thứ 7-CN, 08:00-09:30", 11 },
                    { 12, 16, "LH12", 12, "PKids 02", new DateTime(2026, 8, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Thứ 7-CN, 09:45-11:15", 12 },
                    { 13, 24, "LH13", 13, "P201", new DateTime(2026, 8, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Thứ 3-5, 17:30-19:00", 13 },
                    { 14, 20, "LH14", 14, "P202", new DateTime(2026, 8, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Thứ 2-4, 18:00-19:30", 14 },
                    { 15, 32, "LH15", 15, "Online Zoom 03", new DateTime(2026, 8, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Thứ 2-4-6, 20:00-21:00", 15 }
                });

            migrationBuilder.InsertData(
                table: "TaiKhoan",
                columns: new[] { "Id", "Email", "FullName", "LinkedId", "Password", "Phone", "Role", "UserName" },
                values: new object[,]
                {
                    { 1, "admin@englishcenter.vn", "Administrator", 0, "admin123", "", "Admin", "admin" },
                    { 2, "minhanh@englishcenter.vn", "Nguyễn Minh Anh", 1, "123456", "0901000001", "Teacher", "gv01" },
                    { 3, "quocbao@englishcenter.vn", "Trần Quốc Bảo", 2, "123456", "0901000002", "Teacher", "gv02" },
                    { 4, "thuha@englishcenter.vn", "Lê Thu Hà", 3, "123456", "0901000003", "Teacher", "gv03" },
                    { 5, "duclong@englishcenter.vn", "Phạm Đức Long", 4, "123456", "0901000004", "Teacher", "gv04" },
                    { 6, "maiphuong@englishcenter.vn", "Hoàng Mai Phương", 5, "123456", "0901000005", "Teacher", "gv05" },
                    { 7, "thanhson@englishcenter.vn", "Võ Thanh Sơn", 6, "123456", "0901000006", "Teacher", "gv06" },
                    { 8, "ngoclan@englishcenter.vn", "Đặng Ngọc Lan", 7, "123456", "0901000007", "Teacher", "gv07" },
                    { 9, "quanghung@englishcenter.vn", "Bùi Quang Hưng", 8, "123456", "0901000008", "Teacher", "gv08" },
                    { 10, "khanhvy@englishcenter.vn", "Đỗ Khánh Vy", 9, "123456", "0901000009", "Teacher", "gv09" },
                    { 11, "anhtuan@englishcenter.vn", "Hồ Anh Tuấn", 10, "123456", "0901000010", "Teacher", "gv10" },
                    { 12, "baotram@englishcenter.vn", "Nguyễn Bảo Trâm", 11, "123456", "0901000011", "Teacher", "gv11" },
                    { 13, "giahan@englishcenter.vn", "Trần Gia Hân", 12, "123456", "0901000012", "Teacher", "gv12" },
                    { 14, "nhatminh@englishcenter.vn", "Lê Nhật Minh", 13, "123456", "0901000013", "Teacher", "gv13" },
                    { 15, "hongnhung@englishcenter.vn", "Phạm Hồng Nhung", 14, "123456", "0901000014", "Teacher", "gv14" },
                    { 16, "minhquan@englishcenter.vn", "Hoàng Minh Quân", 15, "123456", "0901000015", "Teacher", "gv15" },
                    { 17, "thuylinh@englishcenter.vn", "Võ Thùy Linh", 16, "123456", "0901000016", "Teacher", "gv16" },
                    { 18, "hainam@englishcenter.vn", "Đặng Hải Nam", 17, "123456", "0901000017", "Teacher", "gv17" },
                    { 19, "phuonganh@englishcenter.vn", "Bùi Phương Anh", 18, "123456", "0901000018", "Teacher", "gv18" },
                    { 20, "viethoang@englishcenter.vn", "Đỗ Việt Hoàng", 19, "123456", "0901000019", "Teacher", "gv19" },
                    { 21, "ngocdiep@englishcenter.vn", "Hồ Ngọc Diệp", 20, "123456", "0901000020", "Teacher", "gv20" },
                    { 22, "daotao@englishcenter.vn", "Nhân viên đào tạo", 0, "123456", "0909000001", "Staff", "nvdt" },
                    { 23, "hv01@englishcenter.vn", "Nguyễn Hoàng Anh", 1, "123456", "0912000001", "Student", "hv01" },
                    { 24, "hv02@englishcenter.vn", "Trần Minh Châu", 2, "123456", "0912000002", "Student", "hv02" },
                    { 25, "hv03@englishcenter.vn", "Lê Gia Bảo", 3, "123456", "0912000003", "Student", "hv03" },
                    { 26, "hv04@englishcenter.vn", "Phạm Khánh Linh", 4, "123456", "0912000004", "Student", "hv04" },
                    { 27, "hv05@englishcenter.vn", "Hoàng Đức Anh", 5, "123456", "0912000005", "Student", "hv05" },
                    { 28, "hv06@englishcenter.vn", "Võ Ngọc Mai", 6, "123456", "0912000006", "Student", "hv06" },
                    { 29, "hv07@englishcenter.vn", "Đặng Quang Huy", 7, "123456", "0912000007", "Student", "hv07" },
                    { 30, "hv08@englishcenter.vn", "Bùi Thu Trang", 8, "123456", "0912000008", "Student", "hv08" },
                    { 31, "hv09@englishcenter.vn", "Đỗ Minh Khang", 9, "123456", "0912000009", "Student", "hv09" },
                    { 32, "hv10@englishcenter.vn", "Hồ Phương Thảo", 10, "123456", "0912000010", "Student", "hv10" },
                    { 33, "hv11@englishcenter.vn", "Nguyễn Tuấn Kiệt", 11, "123456", "0912000011", "Student", "hv11" },
                    { 34, "hv12@englishcenter.vn", "Trần Hải Yến", 12, "123456", "0912000012", "Student", "hv12" },
                    { 35, "hv13@englishcenter.vn", "Lê Nhật Nam", 13, "123456", "0912000013", "Student", "hv13" },
                    { 36, "hv14@englishcenter.vn", "Phạm Bảo Ngọc", 14, "123456", "0912000014", "Student", "hv14" },
                    { 37, "hv15@englishcenter.vn", "Hoàng Anh Thư", 15, "123456", "0912000015", "Student", "hv15" },
                    { 38, "hv16@englishcenter.vn", "Võ Quốc Khánh", 16, "123456", "0912000016", "Student", "hv16" },
                    { 39, "hv17@englishcenter.vn", "Đặng Thanh Hà", 17, "123456", "0912000017", "Student", "hv17" },
                    { 40, "hv18@englishcenter.vn", "Bùi Gia Hân", 18, "123456", "0912000018", "Student", "hv18" },
                    { 41, "hv19@englishcenter.vn", "Đỗ Thành Đạt", 19, "123456", "0912000019", "Student", "hv19" },
                    { 42, "hv20@englishcenter.vn", "Hồ Minh Thư", 20, "123456", "0912000020", "Student", "hv20" },
                    { 43, "hv21@englishcenter.vn", "Nguyễn Đức Minh", 21, "123456", "0912000021", "Student", "hv21" },
                    { 44, "hv22@englishcenter.vn", "Trần Ngọc Ánh", 22, "123456", "0912000022", "Student", "hv22" },
                    { 45, "hv23@englishcenter.vn", "Lê Hoài Phương", 23, "123456", "0912000023", "Student", "hv23" },
                    { 46, "hv24@englishcenter.vn", "Phạm Công Thành", 24, "123456", "0912000024", "Student", "hv24" },
                    { 47, "hv25@englishcenter.vn", "Hoàng Thùy Dương", 25, "123456", "0912000025", "Student", "hv25" },
                    { 48, "hv26@englishcenter.vn", "Võ Minh Triết", 26, "123456", "0912000026", "Student", "hv26" },
                    { 49, "hv27@englishcenter.vn", "Đặng Mai Anh", 27, "123456", "0912000027", "Student", "hv27" },
                    { 50, "hv28@englishcenter.vn", "Bùi Quốc Việt", 28, "123456", "0912000028", "Student", "hv28" },
                    { 51, "hv29@englishcenter.vn", "Đỗ Khánh Vy", 29, "123456", "0912000029", "Student", "hv29" },
                    { 52, "hv30@englishcenter.vn", "Hồ Gia Khiêm", 30, "123456", "0912000030", "Student", "hv30" },
                    { 53, "hv31@englishcenter.vn", "Nguyễn Phương Nhi", 31, "123456", "0912000031", "Student", "hv31" },
                    { 54, "hv32@englishcenter.vn", "Trần Đình Phúc", 32, "123456", "0912000032", "Student", "hv32" },
                    { 55, "hv33@englishcenter.vn", "Lê Ngọc Hân", 33, "123456", "0912000033", "Student", "hv33" },
                    { 56, "hv34@englishcenter.vn", "Phạm Trung Kiên", 34, "123456", "0912000034", "Student", "hv34" },
                    { 57, "hv35@englishcenter.vn", "Hoàng Bích Ngọc", 35, "123456", "0912000035", "Student", "hv35" },
                    { 58, "hv36@englishcenter.vn", "Võ Anh Khoa", 36, "123456", "0912000036", "Student", "hv36" },
                    { 59, "hv37@englishcenter.vn", "Đặng Thu Uyên", 37, "123456", "0912000037", "Student", "hv37" },
                    { 60, "hv38@englishcenter.vn", "Bùi Minh Quân", 38, "123456", "0912000038", "Student", "hv38" },
                    { 61, "hv39@englishcenter.vn", "Đỗ Hải Anh", 39, "123456", "0912000039", "Student", "hv39" },
                    { 62, "hv40@englishcenter.vn", "Hồ Thanh Trúc", 40, "123456", "0912000040", "Student", "hv40" },
                    { 63, "hv41@englishcenter.vn", "Nguyễn Quỳnh Như", 41, "123456", "0912000041", "Student", "hv41" },
                    { 64, "hv42@englishcenter.vn", "Trần Gia Huy", 42, "123456", "0912000042", "Student", "hv42" },
                    { 65, "hv43@englishcenter.vn", "Lê Mỹ Linh", 43, "123456", "0912000043", "Student", "hv43" },
                    { 66, "hv44@englishcenter.vn", "Phạm Quốc Bảo", 44, "123456", "0912000044", "Student", "hv44" },
                    { 67, "hv45@englishcenter.vn", "Hoàng Ngọc Diệp", 45, "123456", "0912000045", "Student", "hv45" }
                });

            migrationBuilder.InsertData(
                table: "DangKy",
                columns: new[] { "Id", "ClassId", "CourseId", "RegisteredAt", "Status", "StudentId" },
                values: new object[,]
                {
                    { 1, 1, 1, new DateTime(2026, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "DaDuyet", 1 },
                    { 2, 2, 3, new DateTime(2026, 7, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "DaDuyet", 2 }
                });

            migrationBuilder.InsertData(
                table: "DiemDanh",
                columns: new[] { "Id", "ClassId", "IsPresent", "Note", "StudentId", "StudyDate" },
                values: new object[,]
                {
                    { 1, 1, true, "", 1, new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, 2, true, "", 2, new DateTime(2026, 7, 8, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "DiemSo",
                columns: new[] { "Id", "ClassId", "Comment", "Final", "Midterm", "StudentId" },
                values: new object[,]
                {
                    { 1, 1, "Tiếp thu tốt, cần nói tự tin hơn.", 8.0, 7.5, 1 },
                    { 2, 2, "Tiến bộ ổn định.", 7.0, 6.5, 2 }
                });

            migrationBuilder.InsertData(
                table: "HocPhi",
                columns: new[] { "Id", "Amount", "EnrollmentId", "PaidAmount", "PaidDate", "PaymentMethod", "Status", "StudentId" },
                values: new object[,]
                {
                    { 3, 2000000m, 3, 0m, null, "Cash", "ChuaDong", 3 },
                    { 1, 1500000m, 1, 1500000m, new DateTime(2026, 7, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Cash", "DaDong", 1 },
                    { 2, 3500000m, 2, 1500000m, new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "Transfer", "DongMotPhan", 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BaiGiang_CourseId",
                table: "BaiGiang",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_BaiGiang_TeacherId",
                table: "BaiGiang",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_DangKy_ClassId",
                table: "DangKy",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_DangKy_CourseId",
                table: "DangKy",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_DangKy_StudentId",
                table: "DangKy",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_DiemDanh_ClassId",
                table: "DiemDanh",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_DiemDanh_StudentId",
                table: "DiemDanh",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_DiemSo_ClassId",
                table: "DiemSo",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_DiemSo_StudentId_ClassId",
                table: "DiemSo",
                columns: new[] { "StudentId", "ClassId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GiaoVien_Code",
                table: "GiaoVien",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HocPhi_EnrollmentId",
                table: "HocPhi",
                column: "EnrollmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HocPhi_StudentId",
                table: "HocPhi",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_HocVien_Code",
                table: "HocVien",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KhoaHoc_Code",
                table: "KhoaHoc",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KhoaHocDaLuu_CourseId",
                table: "KhoaHocDaLuu",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_KhoaHocDaLuu_StudentId_CourseId",
                table: "KhoaHocDaLuu",
                columns: new[] { "StudentId", "CourseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LichSuThanhToan_PaymentId",
                table: "LichSuThanhToan",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuThanhToan_StudentId",
                table: "LichSuThanhToan",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_LopHoc_CourseId",
                table: "LopHoc",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_LopHoc_TeacherId",
                table: "LopHoc",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoan_Role",
                table: "TaiKhoan",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoan_UserName",
                table: "TaiKhoan",
                column: "UserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BaiGiang");

            migrationBuilder.DropTable(
                name: "DiemDanh");

            migrationBuilder.DropTable(
                name: "DiemSo");

            migrationBuilder.DropTable(
                name: "KhoaHocDaLuu");

            migrationBuilder.DropTable(
                name: "LichSuThanhToan");

            migrationBuilder.DropTable(
                name: "TaiKhoan");

            migrationBuilder.DropTable(
                name: "HocPhi");

            migrationBuilder.DropTable(
                name: "VaiTro");

            migrationBuilder.DropTable(
                name: "DangKy");

            migrationBuilder.DropTable(
                name: "HocVien");

            migrationBuilder.DropTable(
                name: "LopHoc");

            migrationBuilder.DropTable(
                name: "GiaoVien");

            migrationBuilder.DropTable(
                name: "KhoaHoc");
        }
    }
}
