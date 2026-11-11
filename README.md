# Competitive Snake

1v1 snake dengan bot. Makanan muncul secara random, ular bertambah panjang setiap 1x makan. Match berjalan 60 detik.

## Aturan menang

1. Ular sendiri mati lebih dulu: kalah
2. Ular lawan mati: menang
3. Dua-duanya mati: comparing panjangnya
4. Waktu habis: comparing panjangnya
5. Panjang sama: draw

Kepala yang menabrak badan lawan mati. Dua kepala bertabrakan langsung, dua-duanya mati.

## Main

Buka `docs/index.html` lewat link GitHub Pages.

- Keyboard: WASD atau Arrow Key
- Layar sentuh: tombol D-pad

## Detail

- Unity 2022.3.62f3, WebGL, Built-in Render Pipeline
- Grid 20x20, tick tetap 8 kali per detik, kedua ular bergerak di tick yang sama
- Bot memakai BFS untuk jalur ke makanan, lalu flood fill untuk memastikan langkah itu tidak membuat dirinya stuck
- Semua visual digambar runtime lewat `Texture2D`
- Build Brotli dengan decompression fallback, ukuran 4.25 MB

## Struktur

- `Assets/Scripts/Core` state match dan data
- `Assets/Scripts/Grid` peta dan occupancy
- `Assets/Scripts/Snake` badan ular, input pemain, bot, keyboard
- `Assets/Scripts/Match` resolver tick, evaluator menang
- `Assets/Scripts/View` render papan, kamera
- `Assets/Scripts/UI` HUD & tombol D-pad
- `Assets/Scripts/Editor/SnakeRuleChecks.cs` collision checker
- `docs/` hasil build untuk GitHub Pages
