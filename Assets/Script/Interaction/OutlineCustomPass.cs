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

        private static readonly HashSet<Renderer> s_activeSet = new();
        private static readonly List<Renderer> s_activeList = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ClearOnLoad()
        {
            s_activeSet.Clear();
            s_activeList.Clear();
        }

        /// <summary>Registers renderers to receive outline rendering this frame.</summary>
        public static void RegisterRenderers(Renderer[] renderers)
        {
            foreach (Renderer r in renderers)
            {
                if (r != null && s_activeSet.Add(r))
                    s_activeList.Add(r);
            }
        }

        /// <summary>Unregisters renderers from outline rendering.</summary>
        public static void UnregisterRenderers(Renderer[] renderers)
        {
            foreach (Renderer r in renderers)
            {
                if (r == null) continue;
                if (s_activeSet.Remove(r))
                    s_activeList.Remove(r);
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
                filterMode: FilterMode.Bilinear,
                name: "OutlineMaskBuffer"
            );
        }

        protected override void Execute(CustomPassContext ctx)
        {
            if (_outlineMaterial == null || _maskBuffer == null) return;

            CoreUtils.SetRenderTarget(ctx.cmd, _maskBuffer, ClearFlag.Color, Color.black);

            if (_maskMaterial != null && s_activeList.Count > 0)
            {
                foreach (Renderer renderer in s_activeList)
                {
                    if (renderer == null || !renderer.gameObject.activeInHierarchy || !renderer.enabled)
                        continue;

                    for (int subMeshIdx = 0; subMeshIdx < renderer.sharedMaterials.Length; subMeshIdx++)
                    {
                        ctx.cmd.DrawRenderer(renderer, _maskMaterial, subMeshIdx, 0);
                    }
                }
            }

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
            s_activeSet.Clear();
            s_activeList.Clear();
        }
    }
}
