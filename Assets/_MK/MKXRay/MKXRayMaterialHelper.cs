using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MK.XRay
{
    public static class MKXRayMaterialHelper
    {
        public static class PropertyNames
        {
            //Editor Properties
            public const string SHOW_MAIN_BEHAVIOR = "_MKEditorShowMainBehavior";
            public const string SHOW_XRAY_BEHAVIOR = "_MKEditorShowXRayBehavior";
            public const string SHOW_DISSOLVE_BEHAVIOR = "_MKEditorShowDissolveBehavior";
            public const string SHOW_NOISE_BEHAVIOR = "_MKEditorShowNoiseBehavior";

            //Main
            public const string MAIN_TEXTURE = "_MainTex";
            public const string MAIN_COLOR = "_Color";

            //XRay
            public const string XRAY_RIM_COLOR = "_XRayRimColor";
            public const string XRAY_RIM_SIZE = "_XRayRimSize";
            public const string XRAY_INSIDE = "_XRayInside";

            //Noise
            public const string USE_NOISE = "_UseNoise";
            public const string NOISE_ANIMATION_SPEED = "_NoiseAnimationSpeed";

            //Dissolve
            public const string USE_DISSOLVE = "_UseDissolve";
            public const string DISSOLVE_MAP = "_DissolveMap";
            public const string DISSOLVE_AMOUNT = "_DissolveAmount";
            public const string DISSOLVE_ANIMATION_DIRECTION = "_DissolveAnimationDirection";

            //Emission
            public const string EMISSION_COLOR = "_EmissionColor";
            public const string EMISSION_MAP = "_EmissionMap";
            public const string EMISSION = "_Emission";
        }

        //Main
        public static void SetMainTexture(Material material, Texture tex)
        {
            material.SetTexture(PropertyNames.MAIN_TEXTURE, tex);
        }
        public static Texture GetMainTexture(Material material)
        {
            return material.GetTexture(PropertyNames.MAIN_TEXTURE);
        }

        public static void SetMainColor(Material material, Color color)
        {
            material.SetColor(PropertyNames.MAIN_COLOR, color);
        }
        public static Color MainColor(Material material)
        {
            return material.GetColor(PropertyNames.MAIN_COLOR);
        }

        //XRay
        public static void SetXRayRimColor(Material material, Color color)
        {
            material.SetColor(PropertyNames.XRAY_RIM_COLOR, color);
        }
        public static Color GetXRayRimColor(Material material)
        {
            return material.GetColor(PropertyNames.XRAY_RIM_COLOR);
        }

        public static void SetXRayRimSize(Material material, float size)
        {
            material.SetFloat(PropertyNames.XRAY_RIM_SIZE, size);
        }
        public static float GetXRayRimSize(Material material)
        {
            return material.GetFloat(PropertyNames.XRAY_RIM_SIZE);
        }

        public static void SetXRayInside(Material material, float v)
        {
            material.SetFloat(PropertyNames.XRAY_INSIDE, v);
        }
        public static float GetXRayInside(Material material)
        {
            return material.GetFloat(PropertyNames.XRAY_INSIDE);
        }

        //Noise
        public static void SetNoiseAnimationSpeed(Material material, float v)
        {
            material.SetFloat(PropertyNames.NOISE_ANIMATION_SPEED, v);
        }
        public static float GetNoiseAnimationSpeed(Material material)
        {
            return material.GetFloat(PropertyNames.NOISE_ANIMATION_SPEED);
        }

        //Dissolve
        public static void SetDissolveMap(Material material, Texture tex)
        {
            material.SetTexture(PropertyNames.DISSOLVE_MAP, tex);
        }
        public static Texture GetDissolveMap(Material material)
        {
            return material.GetTexture(PropertyNames.DISSOLVE_MAP);
        }

        public static void SetDissolveAmount(Material material, float amount)
        {
            material.SetFloat(PropertyNames.DISSOLVE_AMOUNT, amount);
        }
        public static float GetDissolveAmount(Material material)
        {
            return material.GetFloat(PropertyNames.DISSOLVE_AMOUNT);
        }

        /// <summary>
        /// only x, y values used
        /// </summary>
        /// <param name="material"></param>
        /// <param name="v"></param>
        public static void SetDissolveAnimationDirection(Material material, Vector4 v)
        {
            material.SetVector(PropertyNames.DISSOLVE_ANIMATION_DIRECTION, v);
        }

        /// <summary>
        /// only x, y values used
        /// </summary>
        /// <param name="material"></param>
        /// <returns></returns>
        public static Vector4 GetDissolveAnimationDirection(Material material)
        {
            return material.GetVector(PropertyNames.DISSOLVE_ANIMATION_DIRECTION);
        }

        //Emission
        public static void SetEmissionMap(Material material, Texture tex)
        {
            material.SetTexture(PropertyNames.EMISSION_MAP, tex);
        }
        public static Texture GetEmissionMap(Material material)
        {
            return material.GetTexture(PropertyNames.EMISSION_MAP);
        }

        public static void SetEmissionColor(Material material, Color color)
        {
            material.SetColor(PropertyNames.EMISSION_COLOR, color);
        }
        public static Color GetEmissionColor(Material material)
        {
            return material.GetColor(PropertyNames.EMISSION_COLOR);
        }
    }
}