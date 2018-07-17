using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using UnityEditor.Utils;
using UnityEditorInternal;

#if UNITY_EDITOR
namespace MK.XRay
{
    public class MKXRayEditor : ShaderGUI
    {
        public static class GuiStyles
        {
            public static GUIStyle header = new GUIStyle("ShurikenModuleTitle")
            {
                font = (new GUIStyle("Label")).font,
                border = new RectOffset(15, 7, 4, 4),
                fixedHeight = 22,
                contentOffset = new Vector2(20f, -2f),
            };

            public static GUIStyle headerCheckbox = new GUIStyle("ShurikenCheckMark");
            public static GUIStyle headerCheckboxMixed = new GUIStyle("ShurikenCheckMarkMixed");
        }

        private static class GUIContentCollection
        {
            public static GUIContent mainColor = new GUIContent("Inside Color", "Basic color tint");
            public static GUIContent xRayRimSize = new GUIContent("Rim Size", "Amount of highlighted areas by rim");
            public static GUIContent xRayInside = new GUIContent("Inside", "Intensity of the inside color");
            public static GUIContent xRayRimColor = new GUIContent("Rim Color", "Color of the rim highlight");
            public static GUIContent emission = new GUIContent("Emission", "Emission Map (RGB)");
            public static GUIContent dissolveMap = new GUIContent("Dissolve", "Dissolve (R)");
            public static GUIContent dissolveAnimationDirection = new GUIContent("Direction", "Direction: X, Y, Scale: Z, W");
            public static GUIContent mainTex = new GUIContent("Albedo", "Albedo (RGB)");
            public static GUIContent useNoise = new GUIContent("Noise", "Add useNoise");
            public static GUIContent noiseAnimationSpeed = new GUIContent("Noise Speed", "Animation speed of the noise");
        }

        #region const
        internal const string EMISSION_DEFAULT = "_MK_EMISSION_DEFAULT";
        internal const string EMISSION_MAP = "_MK_EMISSION_MAP";
        internal const string DISSOLVE_DEFAULT = "_MK_DISSOLVE_DEFAULT";
        internal const string NOISE = "_MK_NOISE";
        internal const string ALBEDO_MAP = "_MK_ALBEDO_MAP";
        #endregion

        #region pe
        private enum KeywordsToManage
        {
            BUMP,
            COLOR_SOURCE,
            EMISSION,
            DISSOLVE,
            NOISE,
            ALL
        };
        #endregion

        //hdr config
        private ColorPickerHDRConfig colorPickerHDRConfig = new ColorPickerHDRConfig(0f, 99f, 1 / 99f, 3f);

        //Editor Properties
        private MaterialProperty showMainBehavior = null;
        private MaterialProperty showXRayBehavior = null;
        private MaterialProperty showNoiseBehavior = null;
        private MaterialProperty showDissolveBehavior = null;

        //Main
        private MaterialProperty mainColor = null;
        private MaterialProperty mainTex = null;

        //XRay
        private MaterialProperty xRayRimColor = null;
        private MaterialProperty xRayRimSize = null;
        private MaterialProperty xRayInside = null;

        //Noise
        private MaterialProperty useNoise = null;
        private MaterialProperty noiseAnimationSpeed = null;

        //Dissolve
        private MaterialProperty useDissolve = null;
        private MaterialProperty dissolveMap = null;
        private MaterialProperty dissolveAmount = null;
        private MaterialProperty dissolveAnimationDirection = null;

        //Emission
        private MaterialProperty emissionColor = null;
        private MaterialProperty emissionTex = null;

        private bool showGIField = false;

        public void FindProperties(MaterialProperty[] props, Material mat)
        {
            //Editor Properties
            showMainBehavior = FindProperty(MKXRayMaterialHelper.PropertyNames.SHOW_MAIN_BEHAVIOR, props);
            showXRayBehavior = FindProperty(MKXRayMaterialHelper.PropertyNames.SHOW_XRAY_BEHAVIOR, props);
            showDissolveBehavior = FindProperty(MKXRayMaterialHelper.PropertyNames.SHOW_DISSOLVE_BEHAVIOR, props);
            showNoiseBehavior = FindProperty(MKXRayMaterialHelper.PropertyNames.SHOW_NOISE_BEHAVIOR, props);

            //Main
            mainColor = FindProperty(MKXRayMaterialHelper.PropertyNames.MAIN_COLOR, props);
            mainTex = FindProperty(MKXRayMaterialHelper.PropertyNames.MAIN_TEXTURE, props);

            //XRay
            xRayRimColor = FindProperty(MKXRayMaterialHelper.PropertyNames.XRAY_RIM_COLOR, props);
            xRayRimSize = FindProperty(MKXRayMaterialHelper.PropertyNames.XRAY_RIM_SIZE, props);
            xRayInside = FindProperty(MKXRayMaterialHelper.PropertyNames.XRAY_INSIDE, props);

            //Noise
            useNoise = FindProperty(MKXRayMaterialHelper.PropertyNames.USE_NOISE, props);
            noiseAnimationSpeed = FindProperty(MKXRayMaterialHelper.PropertyNames.NOISE_ANIMATION_SPEED, props);

            //Emission
            emissionColor = FindProperty(MKXRayMaterialHelper.PropertyNames.EMISSION_COLOR, props);
            emissionTex = FindProperty(MKXRayMaterialHelper.PropertyNames.EMISSION_MAP, props);

            //Dissolve
            useDissolve = FindProperty(MKXRayMaterialHelper.PropertyNames.USE_DISSOLVE, props);
            dissolveMap = FindProperty(MKXRayMaterialHelper.PropertyNames.DISSOLVE_MAP, props);
            dissolveAmount = FindProperty(MKXRayMaterialHelper.PropertyNames.DISSOLVE_AMOUNT, props);
            dissolveAnimationDirection = FindProperty(MKXRayMaterialHelper.PropertyNames.DISSOLVE_ANIMATION_DIRECTION, props);
        }

        //Colorfield
        private void ColorProperty(MaterialProperty prop, bool showAlpha, bool hdrEnabled, GUIContent label)
        {
            EditorGUI.showMixedValue = prop.hasMixedValue;
            EditorGUI.BeginChangeCheck();
            Color c = EditorGUILayout.ColorField(label, prop.colorValue, false, showAlpha, hdrEnabled, colorPickerHDRConfig);
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
                prop.colorValue = c;
        }

        //Setup GI emission
        private void SetGIFlags()
        {
            foreach (Material obj in emissionColor.targets)
            {
                bool emissive = true;
                if (MKXRayMaterialHelper.GetEmissionColor(obj) == Color.black)
                {
                    emissive = false;
                }
                MaterialGlobalIlluminationFlags flags = obj.globalIlluminationFlags;
                if ((flags & (MaterialGlobalIlluminationFlags.BakedEmissive | MaterialGlobalIlluminationFlags.RealtimeEmissive)) != 0)
                {
                    flags &= ~MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                    if (!emissive)
                        flags |= MaterialGlobalIlluminationFlags.EmissiveIsBlack;

                    obj.globalIlluminationFlags = flags;
                }
            }
        }

        //BoldToggle
        private void ToggleBold(MaterialEditor materialEditor, MaterialProperty prop)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(prop.displayName, EditorStyles.boldLabel, GUILayout.Width(100));
            materialEditor.ShaderProperty(prop, "");
            EditorGUILayout.EndHorizontal();
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            if (material.HasProperty(MKXRayMaterialHelper.PropertyNames.EMISSION))
            {
                MKXRayMaterialHelper.SetEmissionColor(material, material.GetColor(MKXRayMaterialHelper.PropertyNames.EMISSION));
            }
            base.AssignNewShaderToMaterial(material, oldShader, newShader);

            MaterialProperty[] properties = MaterialEditor.GetMaterialProperties(new Material[] { material });
            FindProperties(properties, material);

            UpdateKeywords(KeywordsToManage.ALL);
            SetGIFlags();
        }

        private bool HandleBehavior(string title, MaterialProperty behavior, MaterialEditor materialEditor)
        {
            EditorGUI.showMixedValue = behavior.hasMixedValue;
            var rect = GUILayoutUtility.GetRect(16f, 22f, GuiStyles.header);
            rect.x -= 10;
            rect.width += 10;
            var e = Event.current;

            GUI.Box(rect, title, GuiStyles.header);

            var foldoutRect = new Rect(EditorGUIUtility.currentViewWidth * 0.5f, rect.y + 2, 13f, 13f);
            if (behavior.hasMixedValue)
            {
                foldoutRect.x -= 13;
                foldoutRect.y -= 2;
            }

            EditorGUI.BeginChangeCheck();
            if (e.type == EventType.MouseDown)
            {
                if (rect.Contains(e.mousePosition))
                {
                    if (behavior.hasMixedValue)
                        behavior.floatValue = 0.0f;
                    else
                        behavior.floatValue = Convert.ToSingle(!Convert.ToBoolean(behavior.floatValue));
                    e.Use();
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                if (Convert.ToBoolean(behavior.floatValue))
                    materialEditor.RegisterPropertyChangeUndo(behavior.displayName + " Show");
                else
                    materialEditor.RegisterPropertyChangeUndo(behavior.displayName + " Hide");
            }

            EditorGUI.showMixedValue = false;


                if (e.type == EventType.Repaint && behavior.hasMixedValue)
                    EditorStyles.radioButton.Draw(foldoutRect, "", false, false, true, false);
                else
                    EditorGUI.Foldout(foldoutRect, Convert.ToBoolean(behavior.floatValue), "");

            if (behavior.hasMixedValue)
                return true;
            else
                return Convert.ToBoolean(behavior.floatValue);
        }

        private bool HandleBehavior(string title, MaterialProperty behavior, ref MaterialProperty feature, MaterialEditor materialEditor, string featureName)
        {
            var rect = GUILayoutUtility.GetRect(16f, 22f, GuiStyles.header);
            rect.x -= 10;
            rect.width += 10;
            var e = Event.current;

            GUI.Box(rect, title, GuiStyles.header);

            var foldoutRect = new Rect(EditorGUIUtility.currentViewWidth * 0.5f, rect.y + 2, 13f, 13f);
            if (behavior.hasMixedValue)
            {
                foldoutRect.x -= 13;
                foldoutRect.y -= 2;
            }

            EditorGUI.showMixedValue = feature.hasMixedValue;
            var toggleRect = new Rect(rect.x + 4f, rect.y + ((feature.hasMixedValue) ? 0.0f : 4.0f), 13f, 13f);
            bool fn = Convert.ToBoolean(feature.floatValue);
            EditorGUI.BeginChangeCheck();

            fn = EditorGUI.Toggle(toggleRect, "", fn, GuiStyles.headerCheckbox);

            if (EditorGUI.EndChangeCheck())
            {
                feature.floatValue = Convert.ToSingle(fn);
                if (Convert.ToBoolean(feature.floatValue))
                    materialEditor.RegisterPropertyChangeUndo(feature.displayName + " enabled");
                else
                    materialEditor.RegisterPropertyChangeUndo(feature.displayName + " disabled");
                foreach (Material mat in feature.targets)
                {
                    mat.SetFloat(featureName, feature.floatValue);
                }
            }
            EditorGUI.showMixedValue = false;

            EditorGUI.showMixedValue = behavior.hasMixedValue;
            EditorGUI.BeginChangeCheck();
            if (e.type == EventType.MouseDown)
            {
                if (rect.Contains(e.mousePosition))
                {
                    if (behavior.hasMixedValue)
                        behavior.floatValue = 0.0f;
                    else
                        behavior.floatValue = Convert.ToSingle(!Convert.ToBoolean(behavior.floatValue));
                    e.Use();
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                if (Convert.ToBoolean(behavior.floatValue))
                    materialEditor.RegisterPropertyChangeUndo(behavior.displayName + " show");
                else
                    materialEditor.RegisterPropertyChangeUndo(behavior.displayName + " hide");
            }

            EditorGUI.showMixedValue = false;

            if (e.type == EventType.Repaint)
            {
                if (behavior.hasMixedValue)
                    EditorStyles.radioButton.Draw(foldoutRect, "", false, false, true, false);
                else
                    EditorGUI.Foldout(foldoutRect, Convert.ToBoolean(behavior.floatValue), "");
            }

            if (behavior.hasMixedValue)
                return true;
            else
                return Convert.ToBoolean(behavior.floatValue);
        }

        private void Vector2Field(ref MaterialProperty prop, GUIContent label)
        {
            EditorGUI.showMixedValue = prop.hasMixedValue;
            EditorGUI.BeginChangeCheck();
            Vector2 v = EditorGUILayout.Vector2Field(label, new Vector2(prop.vectorValue.x, prop.vectorValue.y));
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
                prop.vectorValue = new Vector4(v.x, v.y, 0, 0);
        }

        override public void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            Material targetMat = materialEditor.target as Material;
            //get properties
            FindProperties(properties, targetMat);

            if (emissionColor.colorValue != Color.black)
                showGIField = true;
            else
                showGIField = false;

            EditorGUI.BeginChangeCheck();
            //main settings
            if (HandleBehavior("Main", showMainBehavior, materialEditor))
            {
                EditorGUI.BeginChangeCheck();

                EditorGUI.BeginChangeCheck();
                materialEditor.TexturePropertySingleLine(GUIContentCollection.mainTex, mainTex);
                if (EditorGUI.EndChangeCheck())
                {
                    UpdateKeywords(KeywordsToManage.COLOR_SOURCE);
 
                }

                EditorGUI.BeginChangeCheck();
                materialEditor.TexturePropertyWithHDRColor(GUIContentCollection.emission, emissionTex, emissionColor, colorPickerHDRConfig, false);
                if (showGIField)
                    materialEditor.LightmapEmissionProperty(MaterialEditor.kMiniTextureFieldLabelIndentLevel + 1);
                if (EditorGUI.EndChangeCheck())
                {
                    UpdateKeywords(KeywordsToManage.EMISSION);
                    SetGIFlags();
                }
                materialEditor.TextureScaleOffsetProperty(mainTex);
#if UNITY_5_6_OR_NEWER
                materialEditor.EnableInstancingField();
#endif
            }

            //XRay settings
            if (HandleBehavior("XRay", showXRayBehavior, materialEditor))
            {
                ColorProperty(mainColor, false, false, GUIContentCollection.mainColor);
                materialEditor.ShaderProperty(xRayInside, GUIContentCollection.xRayInside);
                ColorProperty(xRayRimColor, false, true, GUIContentCollection.xRayRimColor);
                materialEditor.ShaderProperty(xRayRimSize, GUIContentCollection.xRayRimSize);
            }

            //Noise
            EditorGUI.BeginChangeCheck();
            if (HandleBehavior("Noise", showNoiseBehavior, ref useNoise, materialEditor, MKXRayMaterialHelper.PropertyNames.USE_NOISE))
            {
                materialEditor.ShaderProperty(noiseAnimationSpeed, GUIContentCollection.noiseAnimationSpeed);
            }
            if (EditorGUI.EndChangeCheck())
            {
                UpdateKeywords(KeywordsToManage.NOISE);
            }

            //Dissolve settings
            EditorGUI.BeginChangeCheck();
            if (HandleBehavior("Dissolve", showDissolveBehavior, ref useDissolve, materialEditor, MKXRayMaterialHelper.PropertyNames.USE_DISSOLVE))
            {
                if (dissolveMap.textureValue == null)
                    materialEditor.TexturePropertySingleLine(GUIContentCollection.dissolveMap, dissolveMap);
                else
                    materialEditor.TexturePropertySingleLine(GUIContentCollection.dissolveMap, dissolveMap, dissolveAmount);

                if (dissolveMap.textureValue != null)
                {
                    Vector2Field(ref dissolveAnimationDirection, GUIContentCollection.dissolveAnimationDirection);
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                UpdateKeywords(KeywordsToManage.DISSOLVE);
            }
            EditorGUI.EndChangeCheck();
        }

        private void ManageKeywordsColorSource()
        {
            //Colorsource
            foreach (Material mat in mainTex.targets)
            {
                SetKeyword(MKXRayMaterialHelper.GetMainTexture(mat), ALBEDO_MAP, mat);
            }
        }

        private void ManageKeywordsEmission()
        {
            //emission
            foreach (Material mat in emissionColor.targets)
            {
                SetKeyword(MKXRayMaterialHelper.GetEmissionMap(mat) != null && MKXRayMaterialHelper.GetEmissionColor(mat) != Color.black, EMISSION_MAP, mat);
            }
            foreach (Material mat in emissionColor.targets)
            {
                SetKeyword(MKXRayMaterialHelper.GetEmissionMap(mat) == null && MKXRayMaterialHelper.GetEmissionColor(mat) != Color.black, EMISSION_DEFAULT, mat);
            }
        }

        private void ManageKeywordsDissolve()
        {
            //Dissolve
            foreach (Material mat in useDissolve.targets)
            {
                SetKeyword(MKXRayMaterialHelper.GetDissolveMap(mat) != null && mat.GetFloat(MKXRayMaterialHelper.PropertyNames.USE_DISSOLVE) == 1.0f, DISSOLVE_DEFAULT, mat);
            }
        }

        private void ManageKeywordsNoise()
        {
            //Dissolve
            foreach (Material mat in useNoise.targets)
            {
                SetKeyword(mat.GetFloat(MKXRayMaterialHelper.PropertyNames.USE_NOISE) == 1.0f, NOISE, mat);
            }
        }

        private void UpdateKeywords(KeywordsToManage kw)
        {
            switch (kw)
            {
                case KeywordsToManage.ALL:
                    ManageKeywordsColorSource();
                    ManageKeywordsEmission();
                    ManageKeywordsDissolve();
                    break;
                case KeywordsToManage.EMISSION:
                    ManageKeywordsEmission();
                    break;
                case KeywordsToManage.DISSOLVE:
                    ManageKeywordsDissolve();
                    break;
                case KeywordsToManage.NOISE:
                    ManageKeywordsNoise();
                    break;
                case KeywordsToManage.COLOR_SOURCE:
                    ManageKeywordsColorSource();
                    break;
            }
        }

        private static void SetKeyword(bool enable, string keyword, Material mat)
        {
            if (enable)
            {
                mat.EnableKeyword(keyword);
            }
            else
            {
                mat.DisableKeyword(keyword);
            }
        }

        private void Divider()
        {
            GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) });
        }
    }
}
#endif