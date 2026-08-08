from __future__ import annotations

import os
import re
import sys
from zipfile import ZipFile


def main() -> None:
    path = sys.argv[1]
    with ZipFile(path) as package:
        xml = package.read("word/document.xml").decode("utf-8")

    bad_tokens = {
        token: xml.count(token)
        for token in ("�", "Ã", "Ä", "TODO", "TBD", "Lorem ipsum")
    }
    print(f"bytes={os.path.getsize(path)}")
    print(f"scenario_headings={len(re.findall(r'F\d{2} - ', xml))}")
    print(f"has_23_of_23={'Kịch bản 23/23' in xml}")
    print(f"bad_tokens={bad_tokens}")
    for token, count in bad_tokens.items():
        if count:
            position = xml.find(token)
            print(f"context_{ord(token[0]):04x}={xml[max(0, position - 60):position + 80]}")


if __name__ == "__main__":
    main()
