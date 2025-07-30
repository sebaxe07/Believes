using UnityEngine;
using UnityEngine.Serialization;

namespace Laser {
    [CreateAssetMenu( menuName = "Laser/Laser Render Settings")]
    public class LaserRenderSettings : ScriptableObject {
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        [SerializeField] public Color color;
        [SerializeField] public float width;
        [SerializeField] [Range(1f, 200f)] public float emissionAmount;

        [SerializeField] private Material laserMaterial;
        public void Apply(LineRenderer lineRenderer) {
            if (laserMaterial == null) {
                Debug.LogError("Laser material not assigned!");
                return;
            }
            lineRenderer.material = laserMaterial;
            lineRenderer.material.EnableKeyword("_EMISSION");
            lineRenderer.material.SetColor(EmissionColor, color * emissionAmount);
            lineRenderer.startWidth = width;
            lineRenderer.startColor = color;
        }

    }
}