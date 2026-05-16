using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Experimental.Rendering;
using System.Collections.Generic;

namespace Game.Interaction
{
    [System.Serializable]
    public sealed class OutlineCustomPass : CustomPass
    {
        [Header("Outline Settings")]
        public Color outlineColor = new Color(1f, 0.8f, 0.2f, 1f);
        [Range(0.5f, 5f)]
        public float outlineThickness = 1.2f;

        [Header("Shader")]
        public Shader outlineShader;

        private Material _outlineMaterial;
        private Material _maskMaterial; 
        private RTHandle _maskBuffer;

        private static readonly int _OutlineColorID = Shader.PropertyToID("_OutlineColor");
        private static readonly int _OutlineThicknessID = Shader.PropertyToID("_OutlineThickness");
        private static readonly int _OutlineBufferID = Shader.PropertyToID("_OutlineBuffer");

        // List global untuk menampung renderer objek yang sedang di-hover
        private static readonly HashSet<Renderer> ActiveRenderers = new HashSet<Renderer>();

        public static void RegisterRenderers(Renderer[] renderers)
        {
            foreach (var r in renderers)
            {
                if (r != null) ActiveRenderers.Add(r);
            }
        }

        public static void UnregisterRenderers(Renderer[] renderers)
        {
            foreach (var r in renderers)
            {
                if (r != null) ActiveRenderers.Remove(r);
            }
        }

        protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
        {
            if (outlineShader == null)
            {
                Debug.LogError("[OutlineCustomPass] Outline shader tidak di-assign!");
                return;
            }

            _outlineMaterial = CoreUtils.CreateEngineMaterial(outlineShader);
            
            Shader unlitShader = Shader.Find("Hidden/Internal-Colored");
            if (unlitShader != null)
                _maskMaterial = CoreUtils.CreateEngineMaterial(unlitShader);

            _maskBuffer = RTHandles.Alloc(
                Vector2.one,
                TextureXR.slices,
                dimension: TextureDimension.Tex2DArray,
                colorFormat: GraphicsFormat.R8_UNorm,
                useDynamicScale: true,
                filterMode: FilterMode.Bilinear, // Interprolasi linear murni untuk meredam jitter piksel
                name: "OutlineMaskBuffer"
            );
        }

        protected override void Execute(CustomPassContext ctx)
        {
            if (_outlineMaterial == null || _maskBuffer == null) return;

            // 1. Bersihkan Mask Buffer menjadi hitam sebelum digambar
            CoreUtils.SetRenderTarget(ctx.cmd, _maskBuffer, ClearFlag.Color, Color.black);

            // 2. Gambar semua renderer yang terdaftar ke mask buffer secara manual
            if (_maskMaterial != null && ActiveRenderers.Count > 0)
            {
                foreach (var renderer in ActiveRenderers)
                {
                    if (renderer == null || !renderer.gameObject.activeInHierarchy || !renderer.enabled) 
                        continue;

                    for (int subMeshIdx = 0; subMeshIdx < renderer.sharedMaterials.Length; subMeshIdx++)
                    {
                        // Menghapus nama argumen shaderPassId demi kecocokan overload API CommandBuffer
                        ctx.cmd.DrawRenderer(renderer, _maskMaterial, subMeshIdx, 0);
                    }
                }
            }

            // 3. Render full screen outline shader menggunakan Sobel 9-Tap terfilter
            _outlineMaterial.SetColor(_OutlineColorID, outlineColor);
            _outlineMaterial.SetFloat(_OutlineThicknessID, outlineThickness);
            _outlineMaterial.SetTexture(_OutlineBufferID, _maskBuffer);

            CoreUtils.SetRenderTarget(ctx.cmd, ctx.cameraColorBuffer, ctx.cameraDepthBuffer);
            CoreUtils.DrawFullScreen(ctx.cmd, _outlineMaterial, shaderPassId: 0);
        }

        protected override void Cleanup()
        {
            CoreUtils.Destroy(_outlineMaterial);
            CoreUtils.Destroy(_maskMaterial);
            _maskBuffer?.Release();
            _maskBuffer = null;
            ActiveRenderers.Clear();
        }
    }
}