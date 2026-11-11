using UnityEngine;

namespace CompetitiveSnake
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class SnakeCameraFitter : MonoBehaviour
    {
        [SerializeField] SnakeConfig config;

        UnityEngine.Camera cam;

        void Awake()
        {
            cam = GetComponent<UnityEngine.Camera>();
            Apply();
        }

        void Update()
        {
            if (!Mathf.Approximately(cam.orthographicSize, TargetSize())) Apply();
        }

        float TargetSize()
        {
            float freeBand = Mathf.Clamp(1f - config.bottomUiFraction - config.topUiFraction, 0.2f, 1f);
            float byHeight = config.gridHeight / (2f * freeBand);
            float byWidth = config.gridWidth / (2f * Mathf.Max(0.1f, cam.aspect));
            return Mathf.Max(byHeight, byWidth);
        }

        void Apply()
        {
            float size = TargetSize();
            cam.orthographicSize = size;
            cam.transform.position = new Vector3(0f, -0.21f * size, -10f);
        }
    }
}
