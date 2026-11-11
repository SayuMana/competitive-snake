using System.Collections.Generic;
using UnityEngine;

namespace CompetitiveSnake
{
    public class BoardView : MonoBehaviour
    {
        [SerializeField] SnakeConfig config;

        readonly List<SpriteRenderer> playerParts = new List<SpriteRenderer>();
        readonly List<SpriteRenderer> botParts = new List<SpriteRenderer>();

        SpriteRenderer foodRenderer;
        int width;
        int height;

        public void Build(int gridWidth, int gridHeight)
        {
            width = gridWidth;
            height = gridHeight;

            BuildBoardSurface();
            BuildFood();
        }

        public Vector3 CellToWorld(Vector2Int cell)
        {
            return new Vector3(cell.x - width * 0.5f + 0.5f, cell.y - height * 0.5f + 0.5f, 0f);
        }

        public void Render(SnakeBody player, SnakeBody bot)
        {
            RenderSnake(player, playerParts, Palette.PlayerHead, Palette.PlayerBody);
            RenderSnake(bot, botParts, Palette.BotHead, Palette.BotBody);
        }

        public void RenderFood(Vector2Int cell)
        {
            if (foodRenderer == null) return;

            bool valid = cell.x >= 0 && cell.y >= 0;
            foodRenderer.gameObject.SetActive(valid);
            if (valid) foodRenderer.transform.position = CellToWorld(cell);
        }

        void BuildBoardSurface()
        {
            if (transform.Find("BoardSurface") != null) return;

            const int cellPixels = 8;
            int textureWidth = width * cellPixels;
            int textureHeight = height * cellPixels;

            var texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            var pixels = new Color32[textureWidth * textureHeight];

            for (int y = 0; y < textureHeight; y++)
            {
                for (int x = 0; x < textureWidth; x++)
                {
                    int cellX = x / cellPixels;
                    int cellY = y / cellPixels;
                    bool light = (cellX + cellY) % 2 == 0;
                    pixels[y * textureWidth + x] = light ? (Color32)Palette.BoardLight : (Color32)Palette.BoardDark;
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();

            var go = new GameObject("BoardSurface");
            go.transform.SetParent(transform, false);

            var sprite = Sprite.Create(texture, new Rect(0, 0, textureWidth, textureHeight), new Vector2(0.5f, 0.5f), cellPixels);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = -10;
        }

        void BuildFood()
        {
            if (foodRenderer != null) return;

            var go = new GameObject("Food");
            go.transform.SetParent(transform, false);

            foodRenderer = go.AddComponent<SpriteRenderer>();
            foodRenderer.sprite = RuntimeSpriteFactory.Rounded(16, 16, 8f);
            foodRenderer.color = Palette.Food;
            foodRenderer.sortingOrder = 30;
        }

        void RenderSnake(SnakeBody snake, List<SpriteRenderer> parts, Color headColor, Color bodyColor)
        {
            EnsureParts(parts, snake.Segments.Count);

            for (int i = 0; i < parts.Count; i++)
            {
                bool visible = i < snake.Segments.Count;
                parts[i].gameObject.SetActive(visible);
                if (!visible) continue;

                parts[i].transform.position = CellToWorld(snake.Segments[i]);
                parts[i].color = i == 0 ? headColor : bodyColor;
                parts[i].sortingOrder = 20 - i;
            }
        }

        void EnsureParts(List<SpriteRenderer> parts, int count)
        {
            while (parts.Count < count)
            {
                var go = new GameObject("Part");
                go.transform.SetParent(transform, false);

                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = RuntimeSpriteFactory.Rounded(16, 16, 4f);

                parts.Add(renderer);
            }
        }
    }
}
