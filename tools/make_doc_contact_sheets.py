from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

root = Path(__file__).resolve().parents[1]
render_dir = root / "_doc_work" / "render_v3"
out_dir = root / "_doc_work" / "contact_sheets_v3"
out_dir.mkdir(parents=True, exist_ok=True)

pages = sorted(render_dir.glob("page-*.png"))
font = ImageFont.load_default(size=18)
for group_index in range(0, len(pages), 4):
    group = pages[group_index:group_index + 4]
    images = [Image.open(path).convert("RGB") for path in group]
    width = max(image.width for image in images)
    height = max(image.height for image in images)
    gutter = 24
    label_height = 34
    sheet = Image.new("RGB", (width * 2 + gutter * 3, (height + label_height) * 2 + gutter * 3), "#d9dde3")
    draw = ImageDraw.Draw(sheet)
    for offset, (path, image) in enumerate(zip(group, images)):
        col = offset % 2
        row = offset // 2
        x = gutter + col * (width + gutter)
        y = gutter + row * (height + label_height + gutter)
        draw.rectangle((x, y, x + width, y + label_height), fill="#172b4d")
        draw.text((x + 10, y + 7), path.stem.upper(), fill="white", font=font)
        sheet.paste(image, (x, y + label_height))
    number = group_index // 4 + 1
    sheet.save(out_dir / f"sheet-{number:02d}.png")

print(f"Created {((len(pages) + 3) // 4)} contact sheets for {len(pages)} pages in {out_dir}")
