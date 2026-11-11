# Competitive Snake

Dua ular di satu arena, satu dikendalikan pemain, satu bot. Makanan muncul di posisi acak, ular bertambah panjang setiap makan. Match berjalan 60 detik.

## Aturan menang

1. Ular sendiri mati lebih dulu: kalah
2. Ular lawan mati: menang
3. Dua-duanya mati: dibandingkan panjangnya
4. Waktu habis: dibandingkan panjangnya
5. Panjang sama: seri

Kepala yang menabrak badan lawan mati. Dua kepala bertabrakan langsung, dua-duanya mati.

## Main

Buka `docs/index.html` lewat link GitHub Pages.

- Keyboard: WASD atau tombol panah
- Layar sentuh: tombol D-pad di bawah

## Kontrol teknis

- Unity 2022.3.62f3, WebGL, Built-in Render Pipeline
- Grid 20x20, tick tetap 8 kali per detik, kedua ular bergerak di tick yang sama
- Bot memakai BFS untuk jalur ke makanan, lalu flood fill untuk memastikan langkah itu tidak memenjarakan dirinya
- Semua visual digambar runtime lewat `Texture2D`, tidak ada file aset
- Build Brotli dengan decompression fallback, ukuran 4.25 MB

## Struktur

- `Assets/Scripts/Core` state match dan data
- `Assets/Scripts/Grid` peta dan occupancy
- `Assets/Scripts/Snake` badan ular, input pemain, bot, keyboard
- `Assets/Scripts/Match` resolver tick, evaluator menang
- `Assets/Scripts/View` render papan, kamera
- `Assets/Scripts/UI` HUD dan tombol D-pad
- `Assets/Scripts/Editor/SnakeRuleChecks.cs` sembilan pengecekan aturan tabrakan
- `docs/` hasil build untuk GitHub Pages
