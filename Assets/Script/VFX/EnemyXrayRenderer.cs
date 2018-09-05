using UnityEngine;
using VFXManager = Game.GameManagement.VFXManager;
using EnemyBehavior = Game.Enemy.EnemyBehavior;

namespace Game.VFX
{
    public class EnemyXrayRenderer : MonoBehaviour
    {

        #region Material Properties

        //Editor Properties
        public static string SHOW_MAIN_BEHAVIOR = "_MKEditorShowMainBehavior";
        public static string SHOW_XRAY_BEHAVIOR = "_MKEditorShowXRayBehavior";
        public static string SHOW_DISSOLVE_BEHAVIOR = "_MKEditorShowDissolveBehavior";
        public static string SHOW_NOISE_BEHAVIOR = "_MKEditorShowNoiseBehavior";

        //Main
        public static string MAIN_TEXTURE = "_MainTex";
        public static string MAIN_COLOR = "_Color";

        //XRay
        public static string XRAY_RIM_COLOR = "_XRayRimColor";
        public static string XRAY_RIM_SIZE = "_XRayRimSize";
        public static string XRAY_INSIDE = "_XRayInside";

        //Noise
        public static string USE_NOISE = "_UseNoise";
        public static string NOISE_ANIMATION_SPEED = "_NoiseAnimationSpeed";

        //Dissolve
        public static string USE_DISSOLVE = "_UseDissolve";
        public static string DISSOLVE_MAP = "_DissolveMap";
        public static string DISSOLVE_AMOUNT = "_DissolveAmount";
        public static string DISSOLVE_ANIMATION_DIRECTION = "_DissolveAnimationDirection";

        //Emission
        public static string EMISSION_COLOR = "_EmissionColor";
        public static string EMISSION_MAP = "_EmissionMap";
        public static string EMISSION = "_Emission";

        #endregion

        VFXManager _vfxManager;

        Material _material;
        Renderer _mesh;

        [ColorUsage(true, true)]
        public Color idle;

        [ColorUsage(true, true)]
        public Color watch;

        [ColorUsage(true, true)]
        public Color search;

        [ColorUsage(true, true)]
        public Color chase;

        [ColorUsage(true, true)]
        public Color dead;

        private void Awake()
        {
            _material = new Material(Shader.Find("MK/XRay"));
            _mesh = transform.GetComponent<Renderer>();

            _mesh.material = _material;
            SetMainColor(idle);
            SetXRayInside(0f);
            SetXRayRimSize(0f);
            SetXRayRimColor(idle);
        }

        private void Start()
        {
            _vfxManager = VFXManager.Instance;
        }

        private void Update()
        {
            SetXRayRimSize(_vfxManager.GetPingPongSin());
        }

        public void SetXRayToBehavior(EnemyBehavior behavior)
        {
            switch (behavior)
            {
                case EnemyBehavior.Idle:
                    SetXRayRimColor(idle);
                    return;
                case EnemyBehavior.Watch:
                    SetXRayRimColor(watch);
                    return;
                case EnemyBehavior.Search:
                    SetXRayRimColor(search);
                    return;
                case EnemyBehavior.Chase:
                    SetXRayRimColor(chase);
                    return;
                case EnemyBehavior.Dead:
                    SetXRayRimColor(dead);
                    return;
            }
        }
    
        //Main
        private void SetMainColor(Color color)
        {
            _material.SetColor(MAIN_COLOR, color);
        }

        //XRay
        private void SetXRayRimColor(Color color)
        {
            _material.SetColor(XRAY_RIM_COLOR, color);
        }

        private void SetXRayRimSize(float size)
        {
            _material.SetFloat(XRAY_RIM_SIZE, size);
        }

        private void SetXRayInside(float v)
        {
            _material.SetFloat(XRAY_INSIDE, v);
        }

        //Emission
        private void SetEmissionColor(Color color)
        {
            _material.SetColor(EMISSION_COLOR, color);
        }
    }
}
