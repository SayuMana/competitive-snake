using System.Collections.Generic;
using UnityEngine;

namespace CompetitiveSnake
{
    public static class RuntimeSpriteFactory
    {
        static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

        public static Sprite Rounded(int width, int height, float cornerRadius)
        {
            string key = width + "x" + height + "_" + cornerRadius;
            if (cache.TryGetValue(key, out Sprite cached) && cached != null) return cached;

            float radius = Mathf.Clamp(cornerRadius, 0f, Mathf.Min(width, height) * 0.5f);
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var pixels = new Color32[width * height];
            var solid = new Color32(255, 255, 255, 255);
            var clear = new Color32(255, 255, 255, 0);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool inside = true;

                    if (radius > 0f)
                    {
                        float cx = Mathf.Clamp(x + 0.5f, radius, width - radius);
                        float cy = Mathf.Clamp(y + 0.5f, radius, height - radius);
                        float dx = x + 0.5f - cx;
                        float dy = y + 0.5f - cy;
                        inside = dx * dx + dy * dy <= radius * radius;
                    }

                    pixels[y * width + x] = inside ? solid : clear;
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();

            var sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), width);
            cache[key] = sprite;
            return sprite;
        }
    }
}
