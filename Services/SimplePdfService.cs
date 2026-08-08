using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using web_do_an1.Models;

namespace web_do_an1.Services;

/// <summary>
/// Builds the three printable documents used by the center. The renderer is
/// intentionally self-contained so exports work without a browser or a PDF
/// application installed on the server.
/// </summary>
public sealed class SimplePdfService
{
    private readonly PdfFont _font = PdfFont.Load();

    public byte[] BuildInvoice(Payment payment)
    {
        var canvas = new PdfCanvas(_font);
        canvas.PageBackground();

        var remaining = Math.Max(0, payment.Amount - payment.PaidAmount);
        var statusText = remaining <= 0
            ? "ĐÃ THANH TOÁN"
            : payment.PaidAmount > 0
                ? "THANH TOÁN MỘT PHẦN"
                : "CHƯA THANH TOÁN";
        var statusColor = remaining <= 0
            ? PdfColors.Green
            : payment.PaidAmount > 0
                ? PdfColors.Blue
                : PdfColors.Red;

        DrawHeader(canvas, "HÓA ĐƠN HỌC PHÍ", $"Số hóa đơn #{payment.Id:000000}", "Học phí", statusText, statusColor);

        // Hàng "header-meta" 3 mục ngay dưới tiêu đề, giống .header-meta trong bản mẫu HTML
        // (mẫu dùng Ngày xuất/Kỳ báo cáo/Người lập; với hóa đơn ta thay bằng số hóa đơn/ngày
        // thanh toán/hình thức thanh toán vì phù hợp ngữ cảnh hơn).
        canvas.Text(34, 720, "SỐ HÓA ĐƠN", 7, PdfColors.Blue);
        canvas.Text(34, 703, $"#{payment.Id:000000}", 9.5, PdfColors.Navy);
        canvas.Text(250, 720, "NGÀY THANH TOÁN", 7, PdfColors.Blue);
        canvas.Text(250, 703, payment.PaidDate?.ToString("dd/MM/yyyy") ?? "Chưa hoàn tất", 9.5, PdfColors.Navy);
        canvas.Text(433, 720, "HÌNH THỨC", 7, PdfColors.Blue);
        canvas.Text(433, 703, PaymentMethodText(payment.PaymentMethod), 9.5, PdfColors.Navy);

        // Khối "stats" 3 thẻ, thẻ thứ 3 tô đậm nền navy, giống .stats/.stat-card.highlight trong bản mẫu.
        canvas.MetricCard(34, 605, 162, 65, "TỔNG HỌC PHÍ", Money(payment.Amount), PdfColors.Blue);
        canvas.MetricCard(216, 605, 162, 65, "ĐÃ THANH TOÁN", Money(payment.PaidAmount), PdfColors.Green);
        canvas.MetricCard(399, 605, 162, 65, "CÒN LẠI", Money(remaining), PdfColors.Navy);

        canvas.Card(34, 513, 527, 76, PdfColors.White, PdfColors.Blue);
        canvas.Label(50, 565, "HỌC VIÊN");
        canvas.Text(50, 544, payment.Student.FullName, 14, PdfColors.Navy);
        canvas.Text(50, 526, $"Mã học viên: {payment.Student.Code}  ·  SĐT: {payment.Student.Phone}", 8.5, PdfColors.Muted);
        canvas.Text(50, 516, $"Email: {payment.Student.Email}", 8.5, PdfColors.Muted);
        canvas.Label(326, 565, "ĐƠN VỊ THU HỌC PHÍ");
        canvas.Text(326, 544, "Trung tâm tiếng Anh English Center", 10.5, PdfColors.Navy);
        canvas.Text(326, 528, "268 Đ. Lý Thường Kiệt, Q.10, TP. Hồ Chí Minh", 8, PdfColors.Muted);
        canvas.Text(326, 516, "Hotline: 1900 6868 · info@englishcenter.vn", 8, PdfColors.Muted);

        canvas.Label(34, 489, "CHI TIẾT HỌC PHÍ");
        canvas.TableHeader(34, 459, 527, new[] { ("KHÓA HỌC", 210f), ("THỜI LƯỢNG", 100f), ("TRÌNH ĐỘ", 110f), ("HỌC PHÍ", 107f) });
        var course = payment.Enrollment.Course;
        var classCode = payment.Enrollment.CourseClass?.Code ?? "Chưa xếp lớp";
        var registerDate = payment.Enrollment.RegisteredAt.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        canvas.TableRow(34, 436, 527, new[]
        {
            (course.Name, 210f),
            (course.Duration, 100f),
            (course.Level, 110f),
            (Money(payment.Amount), 107f)
        });
        canvas.Text(44, 424, $"Mã lớp: {classCode}  ·  Đăng ký ngày {registerDate}", 7, PdfColors.Muted);
        canvas.Line(34, 412, 561, PdfColors.Line);

        // Hàng dưới cùng: khối GHI CHÚ (trái, có nền màu) + khối chữ ký (phải, căn giữa),
        // giống .bottom-row { .note-box + .signature } trong bản mẫu HTML.
        canvas.Card(34, 280, 317, 110, PdfColors.NoteBg, borderColor: PdfColors.NoteBorder);
        canvas.Label(50, 364, "GHI CHÚ");
        canvas.WrapText(50, 344,
            "Hóa đơn được xác nhận điện tử bởi hệ thống quản lý English Center và có giá trị đối chiếu học phí. Học viên vui lòng lưu lại hóa đơn để đối chiếu khi cần hỗ trợ hoặc bảo lưu khóa học.",
            8.5, PdfColors.NoteText, 290, 13);

        const float sigCenterX = 361f + 200f / 2f;
        canvas.TextCenter(sigCenterX, 370, "XÁC NHẬN THU NGÂN", 10, PdfColors.Navy);
        canvas.TextCenter(sigCenterX, 352, "Ký và ghi rõ họ tên", 8.5, PdfColors.Muted);
        canvas.Line(361, 300, 561, PdfColors.Line);
        canvas.TextCenter(sigCenterX, 288, "Phòng Tài chính", 9.5, PdfColors.Navy);

        DrawFooter(canvas, "Hóa đơn học phí", BuildSignature(payment.Id.ToString(CultureInfo.InvariantCulture), payment.PaidAmount.ToString(CultureInfo.InvariantCulture)));
        return PdfDocument.Create(canvas);
    }

    public byte[] BuildRevenueReport(int year, int quarter, IReadOnlyList<(int Month, decimal Revenue)> rows)
    {
        var canvas = new PdfCanvas(_font);
        canvas.PageBackground();
        DrawHeader(canvas, "BÁO CÁO DOANH THU", $"Quý {quarter} / {year}", "Báo cáo nội bộ");

        var total = rows.Sum(x => x.Revenue);
        var average = rows.Count == 0 ? 0 : total / rows.Count;
        canvas.MetricCard(34, 655, 162, 65, "SỐ THÁNG", rows.Count.ToString(CultureInfo.InvariantCulture), PdfColors.Blue);
        canvas.MetricCard(216, 655, 162, 65, "TB / THÁNG", Money(average), PdfColors.Teal);
        canvas.MetricCard(399, 655, 162, 65, "TỔNG DOANH THU QUÝ", Money(total), PdfColors.Navy);

        canvas.Label(34, 620, "DOANH THU THEO THÁNG");
        canvas.TableHeader(34, 590, 527, new[] { ("THÁNG", 135f), ("TỶ TRỌNG", 125f), ("DOANH THU", 267f) });
        var max = Math.Max(1m, rows.Select(x => x.Revenue).DefaultIfEmpty().Max());
        var y = 559f;
        foreach (var row in rows)
        {
            canvas.Text(50, y, $"Tháng {row.Month}", 9, PdfColors.Navy);
            var ratio = total <= 0 ? 0 : row.Revenue / total * 100;
            canvas.Text(185, y, $"{ratio:0.0}%", 9, PdfColors.Muted);
            canvas.RoundedRect(270, y - 2, 178, 9, 4, PdfColors.PaleBlue);
            canvas.RoundedRect(270, y - 2, 178 * (float)(row.Revenue / max), 9, 4, PdfColors.Blue);
            canvas.TextRight(561, y, Money(row.Revenue), 9, PdfColors.Navy);
            canvas.Line(34, y - 14, 561, PdfColors.Line);
            y -= 37;
        }

        canvas.Card(34, 330, 527, 58, PdfColors.PaleBlue);
        canvas.Text(50, 363, "GHI CHÚ", 8.5, PdfColors.Muted);
        canvas.WrapText(50, 345, "Số liệu được tổng hợp từ các giao dịch học phí đã được Admin/NVĐT duyệt thành công trong quý. Báo cáo phục vụ mục đích theo dõi nội bộ, không thay thế chứng từ kế toán chính thức.", 8.5, PdfColors.Navy, 490, 13);

        canvas.Card(34, 225, 527, 74, PdfColors.White);
        canvas.Label(50, 278, "XÁC NHẬN");
        canvas.Text(50, 258, "Ký và ghi rõ họ tên", 8.5, PdfColors.Muted);
        canvas.Text(50, 242, "Phòng Tài chính", 9, PdfColors.Navy);
        canvas.Label(342, 278, "KỲ BÁO CÁO");
        var startMonth = (quarter - 1) * 3 + 1;
        canvas.Text(342, 258, $"01/{startMonth:00}/{year} - {DateTime.DaysInMonth(year, startMonth + 2):00}/{startMonth + 2:00}/{year}", 8.5, PdfColors.Navy);

        DrawFooter(canvas, "Báo cáo doanh thu", BuildSignature(year.ToString(), quarter.ToString(), total.ToString(CultureInfo.InvariantCulture)));
        return PdfDocument.Create(canvas);
    }

    public byte[] BuildStudentResult(Score score)
    {
        var canvas = new PdfCanvas(_font);
        canvas.PageBackground();
        DrawHeader(canvas, "KẾT QUẢ HỌC TẬP", "Chứng nhận hoàn thành khóa học", "Đào tạo");

        canvas.Card(34, 650, 527, 70, PdfColors.White, PdfColors.Blue);
        canvas.Label(50, 694, "HỌC VIÊN");
        canvas.Text(50, 673, score.Student.FullName, 14, PdfColors.Navy);
        canvas.Text(50, 655, $"Mã học viên: {score.Student.Code}", 8.5, PdfColors.Muted);
        canvas.Label(326, 694, "KHÓA HỌC");
        canvas.Text(326, 673, score.CourseClass.Course.Name, 10.5, PdfColors.Navy);
        canvas.Text(326, 655, $"Lớp: {score.CourseClass.Code}  ·  Giáo viên: {score.CourseClass.Teacher.FullName}", 8, PdfColors.Muted);

        canvas.Label(34, 615, "BẢNG ĐIỂM");
        canvas.MetricCard(34, 548, 157, 55, "ĐIỂM GIỮA KỲ", $"{score.MidtermScore:0.0}", PdfColors.Blue);
        canvas.MetricCard(219, 548, 157, 55, "ĐIỂM CUỐI KỲ", $"{score.FinalScore:0.0}", PdfColors.Teal);
        canvas.MetricCard(404, 548, 157, 55, "ĐIỂM TRUNG BÌNH", $"{score.AverageScore:0.0}", PdfColors.Navy);

        var passed = score.Result == "Đạt";
        canvas.Badge(34, 515, passed ? "ĐẠT" : "CHƯA ĐẠT", passed ? PdfColors.Green : PdfColors.Red, PdfColors.White);
        canvas.Text(120, 519, passed ? "Đạt - Hoàn thành khóa học" : "Chưa đạt - Cần cải thiện", 10, PdfColors.Navy);

        // Cột xếp loại (Xuất sắc / Giỏi / Khá / Trung bình / Yếu) - đồng bộ với bảng điểm ở trang giáo viên.
        var classificationColor = score.Classification switch
        {
            "Xuất sắc" => PdfColors.Purple,
            "Giỏi" => PdfColors.Green,
            "Khá" => PdfColors.Blue,
            "Trung bình" => PdfColors.Amber,
            _ => PdfColors.Red
        };
        canvas.Label(445, 533, "XẾP LOẠI");
        canvas.Badge(445, 508, score.Classification.ToUpperInvariant(), classificationColor, PdfColors.White);

        canvas.Label(34, 470, "NHẬN XÉT CỦA GIÁO VIÊN");
        canvas.Card(34, 386, 527, 64, PdfColors.PaleBlue);
        canvas.WrapText(50, 425, string.IsNullOrWhiteSpace(score.Comment) ? "Chưa có nhận xét." : score.Comment, 9, PdfColors.Navy, 490, 14);

        canvas.Card(34, 263, 527, 72, PdfColors.White);
        canvas.Label(50, 315, "GIÁO VIÊN PHỤ TRÁCH");
        canvas.Text(50, 295, "Ký và ghi rõ họ tên", 8.5, PdfColors.Muted);
        canvas.Text(50, 279, "Phòng Đào tạo", 9, PdfColors.Navy);
        canvas.Label(342, 315, "MÃ LỚP");
        canvas.Text(342, 295, score.CourseClass.Code, 9, PdfColors.Navy);
        canvas.Text(342, 279, $"Ngày xuất: {DateTime.Now:dd/MM/yyyy}", 8.5, PdfColors.Muted);

        DrawFooter(canvas, "Kết quả học tập", BuildSignature(score.Student.Code, score.CourseClass.Code, score.AverageScore.ToString(CultureInfo.InvariantCulture)));
        return PdfDocument.Create(canvas);
    }

    private void DrawHeader(PdfCanvas canvas, string title, string subtitle, string section, string? badgeText = null, string? badgeColor = null)
    {
        // Dùng nền navy đặc để mọi tài liệu PDF có header đồng nhất,
        // không chuyển sang xanh dương ở phía bên phải.
        canvas.Rect(0, 742, 595, 100, PdfColors.Navy);
        canvas.Circle(560, 815, 130, PdfColors.HeaderGlow);
        canvas.Circle(600, 758, 90, PdfColors.HeaderGlow);

        canvas.RoundedRect(34, 778, 38, 38, 9, PdfColors.Blue);
        canvas.Text(47, 790, "E", 22, PdfColors.White);
        canvas.Text(84, 802, "English Center", 12, PdfColors.White);
        canvas.Text(84, 787, "TRUNG TÂM TIẾNG ANH", 7, PdfColors.LightBlue);
        canvas.Text(34, 754, title, 17, PdfColors.White);
        canvas.Text(400, 805, "NGÀY XUẤT", 7, PdfColors.LightBlue);
        canvas.Text(400, 789, DateTime.Now.ToString("dd/MM/yyyy HH:mm"), 8.5, PdfColors.White);
        canvas.Text(400, 770, section, 8, PdfColors.LightBlue);
        canvas.Text(34, 730, subtitle, 9, PdfColors.Muted);

        if (badgeText is not null)
        {
            canvas.Badge(478, 800, badgeText, badgeColor ?? PdfColors.Green, PdfColors.White);
        }
    }

    private void DrawFooter(PdfCanvas canvas, string name, string signature)
    {
        canvas.Line(34, 188, 561, PdfColors.Line);
        canvas.Text(34, 166, "Cảm ơn bạn đã đồng hành cùng English Center!", 8.5, PdfColors.Muted);
        canvas.Text(34, 150, "Tài liệu được xác nhận điện tử bởi hệ thống quản lý.", 7.5, PdfColors.Muted);
        canvas.Text(360, 166, "CHỮ KÝ SỐ", 7, PdfColors.Muted);
        canvas.Text(360, 150, signature, 6.5, PdfColors.Navy);
        canvas.Text(34, 126, name, 7, PdfColors.Muted);
    }

    private static string Money(decimal amount) => $"{amount.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"))} đ";

    private static string PaymentMethodText(PaymentMethod method) => method switch
    {
        PaymentMethod.BankTransfer => "Chuyển khoản ngân hàng",
        PaymentMethod.Card => "Thẻ",
        _ => "Tiền mặt"
    };

    private static string BuildSignature(params string[] values)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("|", values)));
        return Convert.ToHexString(hash)[..32];
    }

    private static class PdfColors
    {
        public const string Navy = "0.06 0.16 0.34";
        public const string Blue = "0.12 0.40 0.85";
        public const string Teal = "0.05 0.55 0.55";
        public const string Green = "0.08 0.55 0.30";
        public const string Red = "0.76 0.17 0.18";
        public const string Purple = "0.36 0.20 0.68";
        public const string Amber = "0.70 0.45 0.05";
        public const string White = "1 1 1";
        public const string PaleBlue = "0.94 0.97 1";
        public const string LightBlue = "0.70 0.83 1";
        public const string Muted = "0.35 0.43 0.54";
        public const string Line = "0.82 0.86 0.92";

        // Nền/viền cho khối "GHI CHÚ", mô phỏng --note-bg/--note-border trong bản mẫu HTML.
        public const string NoteBg = "0.99 0.97 0.91";
        public const string NoteBorder = "0.94 0.89 0.69";
        public const string NoteText = "0.36 0.32 0.22";

        // Sắc navy nhạt hơn một chút dùng cho các vòng tròn trang trí ở header,
        // tạo cảm giác chiều sâu gần với gradient CSS mà không cần alpha blending.
        public const string HeaderGlow = "0.14 0.24 0.44";
    }

    private sealed class PdfCanvas
    {
        private readonly StringBuilder _content = new();
        private readonly PdfFont _font;

        public PdfCanvas(PdfFont font) => _font = font;

        public string Content => _content.ToString();
        public PdfFont Font => _font;

        public void PageBackground() => Rect(0, 0, 595, 842, "0.98 0.99 1");

        public void Rect(float x, float y, float width, float height, string color)
        {
            _content.AppendLine($"q {color} rg {x:0.##} {y:0.##} {width:0.##} {height:0.##} re f Q");
        }

        public void RoundedRect(float x, float y, float width, float height, float radius, string color)
        {
            var k = 0.5522848f;
            var c = radius * k;
            _content.AppendLine(
                $"q {color} rg " +
                $"{x + radius:0.##} {y:0.##} m " +
                $"{x + width - radius:0.##} {y:0.##} l " +
                $"{x + width - radius + c:0.##} {y:0.##} {x + width:0.##} {y + radius - c:0.##} {x + width:0.##} {y + radius:0.##} c " +
                $"{x + width:0.##} {y + height - radius:0.##} l " +
                $"{x + width:0.##} {y + height - radius + c:0.##} {x + width - radius + c:0.##} {y + height:0.##} {x + width - radius:0.##} {y + height:0.##} c " +
                $"{x + radius:0.##} {y + height:0.##} l " +
                $"{x + radius - c:0.##} {y + height:0.##} {x:0.##} {y + height - radius + c:0.##} {x:0.##} {y + height - radius:0.##} c " +
                $"{x:0.##} {y + radius:0.##} l " +
                $"{x:0.##} {y + radius - c:0.##} {x + radius - c:0.##} {y:0.##} {x + radius:0.##} {y:0.##} c h f Q");
        }

        /// <summary>Vẽ một hình tròn (dùng lại RoundedRect với bán kính bằng nửa đường kính).</summary>
        public void Circle(float centerX, float centerY, float diameter, string color)
        {
            RoundedRect(centerX - diameter / 2, centerY - diameter / 2, diameter, diameter, diameter / 2, color);
        }

        /// <summary>Mô phỏng gradient CSS bằng cách vẽ nhiều dải màu nội suy liên tiếp.</summary>
        public void GradientBand(float x, float y, float width, float height, string colorStart, string colorEnd)
        {
            const int steps = 28;
            var start = ParseColor(colorStart);
            var end = ParseColor(colorEnd);
            var stepWidth = width / steps;
            for (var i = 0; i < steps; i++)
            {
                var t = steps <= 1 ? 0f : i / (float)(steps - 1);
                var color = LerpColor(start, end, t);
                Rect(x + i * stepWidth, y, stepWidth + 0.75f, height, color);
            }
        }

        public void Card(float x, float y, float width, float height, string color, string? accent = null, string? borderColor = null)
        {
            RoundedRect(x, y, width, height, 8, color);
            _content.AppendLine($"q {borderColor ?? PdfColors.Line} RG {x:0.##} {y:0.##} {width:0.##} {height:0.##} re S Q");
            if (accent is not null)
            {
                // Viền nhấn màu bên trái, giống style .info-card border-left của bản mẫu HTML.
                Rect(x, y, 3.4f, height, accent);
            }
        }

        public void Line(float x1, float y1, float x2, string color)
        {
            _content.AppendLine($"q {color} RG 0.7 w {x1:0.##} {y1:0.##} m {x2:0.##} {y1:0.##} l S Q");
        }

        public void Text(float x, float y, string value, double size, string color)
        {
            _content.AppendLine($"BT /F1 {size:0.##} Tf {color} rg 1 0 0 1 {x:0.##} {y:0.##} Tm {_font.Encode(value)} Tj ET");
        }

        /// <summary>Vẽ text căn phải theo mép rightX (dùng cho số tiền trong bảng/tổng kết).</summary>
        public void TextRight(float rightX, float y, string value, double size, string color)
        {
            var width = (float)_font.Measure(value, size);
            Text(rightX - width, y, value, size, color);
        }

        public void Label(float x, float y, string value) => Text(x, y, value, 7, PdfColors.Blue);

        /// <summary>Vẽ text căn giữa theo trục centerX (dùng cho khối chữ ký kiểu .signature trong bản mẫu).</summary>
        public void TextCenter(float centerX, float y, string value, double size, string color)
        {
            var width = (float)_font.Measure(value, size);
            Text(centerX - width / 2, y, value, size, color);
        }

        public void WrapText(float x, float y, string value, double size, string color, float maxWidth, float lineHeight)
        {
            var line = new StringBuilder();
            foreach (var word in value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                var candidate = line.Length == 0 ? word : $"{line} {word}";
                if (line.Length > 0 && _font.Measure(candidate, size) > maxWidth)
                {
                    Text(x, y, line.ToString(), size, color);
                    y -= lineHeight;
                    line.Clear();
                }
                if (line.Length > 0) line.Append(' ');
                line.Append(word);
            }
            if (line.Length > 0) Text(x, y, line.ToString(), size, color);
        }

        public void Badge(float x, float y, string value, string background, string foreground)
        {
            var width = (float)Math.Max(76, _font.Measure(value, 7.2) + 20);
            RoundedRect(x, y, width, 22, 8, background);
            Text(x + 10, y + 7, value, 7.2f, foreground);
        }

        public void MetricCard(float x, float y, float width, float height, string label, string value, string accent)
        {
            Card(x, y, width, height, PdfColors.White);
            Rect(x, y + height - 5, width, 5, accent);
            Text(x + 12, y + height - 22, label, 7, PdfColors.Muted);
            Text(x + 12, y + 17, value, value.Length > 14 ? 9 : 15, PdfColors.Navy);
        }

        public void TableHeader(float x, float y, float width, IReadOnlyList<(string Label, float Width)> columns)
        {
            Rect(x, y, width, 25, PdfColors.Navy);
            var offset = x;
            foreach (var column in columns)
            {
                Text(offset + 10, y + 9, column.Label, 6.5f, PdfColors.White);
                offset += column.Width;
            }
        }

        public void TableRow(float x, float y, float width, IReadOnlyList<(string Value, float Width)> columns)
        {
            var offset = x;
            foreach (var column in columns)
            {
                Text(offset + 10, y + 8, column.Value, 8, PdfColors.Navy);
                offset += column.Width;
            }
        }

        private static (float R, float G, float B) ParseColor(string value)
        {
            var parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return (
                float.Parse(parts[0], CultureInfo.InvariantCulture),
                float.Parse(parts[1], CultureInfo.InvariantCulture),
                float.Parse(parts[2], CultureInfo.InvariantCulture));
        }

        private static string LerpColor((float R, float G, float B) a, (float R, float G, float B) b, float t)
        {
            var r = a.R + (b.R - a.R) * t;
            var g = a.G + (b.G - a.G) * t;
            var bl = a.B + (b.B - a.B) * t;
            return $"{r.ToString("0.###", CultureInfo.InvariantCulture)} {g.ToString("0.###", CultureInfo.InvariantCulture)} {bl.ToString("0.###", CultureInfo.InvariantCulture)}";
        }
    }

    private static class PdfDocument
    {
        public static byte[] Create(PdfCanvas canvas) => Create(new[] { canvas.Content }, canvasContentFont: canvas);

        private static byte[] Create(IReadOnlyList<string> pages, PdfCanvas canvasContentFont)
        {
            var font = GetFont(canvasContentFont);
            var objects = new List<PdfObject> { new(""), new("") };
            var fontFileId = AddStream(objects, $"<< /Length 0 /Length1 {font.Data.Length} >>", font.Data);
            var descriptorId = Add(objects, $"<< /Type /FontDescriptor /FontName /Arial /Flags 32 /FontBBox [-665 -325 2000 1000] /ItalicAngle 0 /Ascent 905 /Descent -212 /CapHeight 700 /StemV 80 /FontFile2 {fontFileId} 0 R >>");
            var cidWidths = font.BuildWidths();
            var cidId = Add(objects, $"<< /Type /Font /Subtype /CIDFontType2 /BaseFont /Arial /CIDSystemInfo << /Registry (Adobe) /Ordering (Identity) /Supplement 0 >> /FontDescriptor {descriptorId} 0 R /DW 600 /W {cidWidths} /CIDToGIDMap /Identity >>");
            var toUnicodeId = AddStream(objects, $"<< /Length 0 >>", font.BuildToUnicode());
            var type0Id = Add(objects, $"<< /Type /Font /Subtype /Type0 /BaseFont /Arial /Encoding /Identity-H /DescendantFonts [{cidId} 0 R] /ToUnicode {toUnicodeId} 0 R >>");

            var pageIds = new List<int>();
            foreach (var page in pages)
            {
                var contentId = AddStream(objects, $"<< /Length 0 >>", Encoding.ASCII.GetBytes(page));
                pageIds.Add(Add(objects, $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 {type0Id} 0 R >> >> /Contents {contentId} 0 R >>"));
            }

            objects[0] = new PdfObject("<< /Type /Catalog /Pages 2 0 R >>");
            objects[1] = new PdfObject($"<< /Type /Pages /Kids [{string.Join(' ', pageIds.Select(x => $"{x} 0 R"))}] /Count {pageIds.Count} >>");
            return Write(objects);
        }

        private static PdfFont GetFont(PdfCanvas canvas) => canvas.Font;

        private static int Add(List<PdfObject> objects, string value)
        {
            objects.Add(new PdfObject(value));
            return objects.Count;
        }

        private static int AddStream(List<PdfObject> objects, string dictionary, byte[] stream)
        {
            objects.Add(new PdfObject(dictionary.Replace("/Length 0", $"/Length {stream.Length}"), stream));
            return objects.Count;
        }

        private static byte[] Write(IReadOnlyList<PdfObject> objects)
        {
            using var stream = new MemoryStream();
            WriteAscii(stream, "%PDF-1.7\n%\xE2\xE3\xCF\xD3\n");
            var offsets = new long[objects.Count + 1];
            for (var i = 0; i < objects.Count; i++)
            {
                offsets[i + 1] = stream.Position;
                WriteAscii(stream, $"{i + 1} 0 obj\n");
                WriteAscii(stream, objects[i].Dictionary);
                if (objects[i].Stream is not null)
                {
                    WriteAscii(stream, "\nstream\n");
                    stream.Write(objects[i].Stream);
                    WriteAscii(stream, "\nendstream");
                }
                WriteAscii(stream, "\nendobj\n");
            }
            var xref = stream.Position;
            WriteAscii(stream, $"xref\n0 {objects.Count + 1}\n0000000000 65535 f \n");
            for (var i = 1; i < offsets.Length; i++) WriteAscii(stream, $"{offsets[i]:0000000000} 00000 n \n");
            WriteAscii(stream, $"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF");
            return stream.ToArray();
        }

        private static void WriteAscii(Stream stream, string value) => stream.Write(Encoding.ASCII.GetBytes(value));

        private sealed record PdfObject(string Dictionary, byte[]? Stream = null);
    }

    private sealed class PdfFont
    {
        private readonly byte[] _data;
        private readonly Dictionary<int, int> _cmap;
        private readonly int[] _advances;
        private readonly int _unitsPerEm;
        private readonly Dictionary<int, int> _used = new();

        public byte[] Data => _data;

        private PdfFont(byte[] data, Dictionary<int, int> cmap, int[] advances, int unitsPerEm)
        {
            _data = data;
            _cmap = cmap;
            _advances = advances;
            _unitsPerEm = unitsPerEm;
        }

        public static PdfFont Load()
        {
            var candidates = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf"),
                @"C:\Windows\Fonts\arial.ttf",
                "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf"
            };
            var path = candidates.FirstOrDefault(File.Exists);
            if (path is null) throw new FileNotFoundException("Không tìm thấy font Unicode để xuất PDF.");
            var data = File.ReadAllBytes(path);
            var tables = ReadTables(data);
            var units = U16(data, tables["head"] + 18);
            var glyphCount = U16(data, tables["maxp"] + 4);
            var metricCount = U16(data, tables["hhea"] + 34);
            var advances = new int[glyphCount];
            var hmtx = tables["hmtx"];
            var last = 0;
            for (var i = 0; i < glyphCount; i++)
            {
                if (i < metricCount) last = U16(data, hmtx + i * 4);
                advances[i] = last;
            }
            return new PdfFont(data, ReadCmap(data, tables["cmap"]), advances, units);
        }

        public string Encode(string value)
        {
            var bytes = new StringBuilder("<");
            foreach (var character in value ?? string.Empty)
            {
                var glyph = _cmap.TryGetValue(character, out var mapped) ? mapped : 0;
                _used.TryAdd(glyph, character);
                bytes.Append(glyph.ToString("X4", CultureInfo.InvariantCulture));
            }
            return bytes.Append('>').ToString();
        }

        public double Measure(string value, double size)
        {
            var total = 0d;
            foreach (var character in value ?? string.Empty)
            {
                var glyph = _cmap.TryGetValue(character, out var mapped) ? mapped : 0;
                total += (glyph < _advances.Length ? _advances[glyph] : _advances[0]) * size / _unitsPerEm;
            }
            return total;
        }

        public string BuildWidths()
        {
            var parts = new StringBuilder("[");
            foreach (var glyph in _used.Keys.OrderBy(x => x))
            {
                var width = glyph < _advances.Length ? _advances[glyph] * 1000 / _unitsPerEm : 600;
                parts.Append($"{glyph} [{width}] ");
            }
            return parts.Append(']').ToString();
        }

        public byte[] BuildToUnicode()
        {
            var mappings = _used.OrderBy(x => x.Key).ToList();
            var builder = new StringBuilder();
            builder.AppendLine("/CIDInit /ProcSet findresource begin");
            builder.AppendLine("12 dict begin begincmap");
            builder.AppendLine("/CIDSystemInfo << /Registry (Adobe) /Ordering (UCS) /Supplement 0 >> def");
            builder.AppendLine("/CMapName /ArialUnicode def /CMapType 2 def");
            builder.AppendLine("1 begincodespacerange <0000> <FFFF> endcodespacerange");
            foreach (var chunk in mappings.Chunk(100))
            {
                builder.AppendLine($"{chunk.Length} beginbfchar");
                foreach (var mapping in chunk)
                {
                    var unicode = mapping.Value.ToString("X4", CultureInfo.InvariantCulture);
                    builder.AppendLine($"<{mapping.Key:X4}> <{unicode}>");
                }
                builder.AppendLine("endbfchar");
            }
            builder.AppendLine("endcmap CMapName currentdict /CMap defineresource pop end end");
            return Encoding.ASCII.GetBytes(builder.ToString());
        }

        private static Dictionary<string, int> ReadTables(byte[] data)
        {
            var count = U16(data, 4);
            var result = new Dictionary<string, int>();
            for (var i = 0; i < count; i++)
            {
                var at = 12 + i * 16;
                var tag = Encoding.ASCII.GetString(data, at, 4);
                result[tag] = (int)U32(data, at + 8);
            }
            return result;
        }

        private static Dictionary<int, int> ReadCmap(byte[] data, int cmapOffset)
        {
            var result = new Dictionary<int, int>();
            var records = U16(data, cmapOffset + 2);
            var chosen = 0;
            for (var i = 0; i < records; i++)
            {
                var at = cmapOffset + 4 + i * 8;
                var platform = U16(data, at);
                var encoding = U16(data, at + 2);
                var offset = (int)U32(data, at + 4);
                var format = U16(data, cmapOffset + offset);
                if (format == 4 && (platform == 3 || platform == 0) && (encoding == 1 || encoding == 10 || platform == 0))
                {
                    chosen = cmapOffset + offset;
                    break;
                }
            }
            if (chosen == 0) return result;
            var segments = U16(data, chosen + 6) / 2;
            var end = chosen + 14;
            var start = end + segments * 2 + 2;
            var delta = start + segments * 2;
            var range = delta + segments * 2;
            for (var segment = 0; segment < segments; segment++)
            {
                var endCode = U16(data, end + segment * 2);
                var startCode = U16(data, start + segment * 2);
                var idDelta = (short)U16(data, delta + segment * 2);
                var idRange = U16(data, range + segment * 2);
                for (var code = startCode; code <= endCode && code != 0xFFFF; code++)
                {
                    var glyph = idRange == 0
                        ? (code + idDelta) & 0xFFFF
                        : U16(data, range + segment * 2 + idRange + (code - startCode) * 2);
                    if (idRange != 0 && glyph != 0) glyph = (glyph + idDelta) & 0xFFFF;
                    result[code] = glyph;
                }
            }
            return result;
        }

        private static int U16(byte[] data, int at) => (data[at] << 8) | data[at + 1];
        private static uint U32(byte[] data, int at) => (uint)(data[at] << 24 | data[at + 1] << 16 | data[at + 2] << 8 | data[at + 3]);
    }
}
