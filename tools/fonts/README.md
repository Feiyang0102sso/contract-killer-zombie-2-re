# tools/fonts

Inputs of `tools/make_cn_font.py`, which rebuilds the Simplified Chinese bitmap font
(`Assets/ui/fonts/ChineseSimplified.txt` + `ChineseSimplified_0.png`) from the characters used in
`Assets/localization/zh-Hans.txt`. Run it after changing Chinese texts (Windows only, it renders with GDI):

```
../.venv/Scripts/python.exe tools/make_cn_font.py
```

| File | What it is |
| --- | --- |
| `DroidSansFallback.ttf` | Droid Sans Fallback, Android 4.4 (`android-4.4_r1`, `frameworks/base/data/fonts`). The font of the original Chinese atlas; rendering with it reproduces every original glyph pixel for pixel. |
| `ChineseSimplified_preserved/U+XXXX.png` | Glyphs copied as images instead of rendered: `@` silver and `^` gold currency icons, `£` and `б` (another font in the original). Metrics are in `PRESERVED_GLYPHS` of the script. |
| `ChineseSimplified_kerning.txt` | The original 372 kerning pairs (Latin, Cyrillic, quotes). GDI cannot read this font's split `kern` table. |

## License

Droid Sans Fallback (version 2.55b) is licensed under the Apache License, Version 2.0
(<https://www.apache.org/licenses/LICENSE-2.0>). From the font's name table: "Digitized data copyright Google
Corporation © 2006".
