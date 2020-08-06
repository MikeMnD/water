using UnityEngine;

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
#endif

namespace XFurStudio {

    [ExecuteInEditMode]
    public class XFur_WeatherSystem : MonoBehaviour {

        public bool wind;
        public bool snow;
        public bool rain;

        public Vector3 windDir = Vector3.forward;
        public float windIntensity = 0.2f;
        public float windTurbulence = 0.65f;
        public float snowIntensity;
        public float rainIntensity;

#if UNITY_EDITOR
        private Mesh arrowMesh;
#endif
        public static Vector3 snowDirection;
        public static Vector3 rainDirection;
        public static float snowStrength;
        public static float rainStrength;

        public void Update() {
            windDir = transform.forward;

            rainDirection = Vector3.Lerp(Vector3.down, windDir, windIntensity);
            rainStrength = rain ? rainIntensity : 0;

            snowDirection = Vector3.Lerp(Vector3.down, windDir, windIntensity / 2);
            snowStrength = snow ? snowIntensity : 0;

            Shader.SetGlobalFloat("_WindSpeed", windTurbulence);
            Shader.SetGlobalVector("_WindDirection", windDir * windIntensity);
        }


        void OnDrawGizmos() {
#if UNITY_EDITOR
            if (arrowMesh) {
                Gizmos.DrawWireMesh(arrowMesh, transform.position, transform.rotation, Vector3.one * windIntensity);
            }
            else {

                arrowMesh = UnityEditor.AssetDatabase.LoadAssetAtPath<Mesh>(UnityEditor.AssetDatabase.GUIDToAssetPath(UnityEditor.AssetDatabase.FindAssets("Arrow_Model")[0]));

            }
#endif
        }

    }



#if UNITY_EDITOR

    [CustomEditor(typeof(XFur_WeatherSystem))]
    public class XFur_WeatherEditor : Editor {

        public bool[] folds;
        private GUISkin pidiSkin2;
        private Texture2D logo;
        private string version = "1.9";

        private void OnEnable() {
            folds = new bool[3];
        }

        public override void OnInspectorGUI() {

            var mTarget = (XFur_WeatherSystem)target;

            Undo.RecordObject(mTarget, "WeatherSystem" + mTarget.GetInstanceID());

            if (!pidiSkin2) {
                pidiSkin2 = AssetDatabase.LoadAssetAtPath<GUISkin>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("PIDI_EditorSkin_XFurStudio")[0]));
                logo = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("XFur Studio Logo")[0]));
            }


            GUILayout.BeginHorizontal(); GUILayout.BeginVertical(pidiSkin2.box);

            AssetLogoAndVersion();

            GUILayout.Space(8);



            if (BeginCenteredGroup("WEATHER SETTINGS", ref folds[0])) {

                GUILayout.Space(16);

                mTarget.snow = EnableDisableToggle(new GUIContent("SNOW SIMULATION", "Enables snow on this scene"), mTarget.snow);
                mTarget.rain = EnableDisableToggle(new GUIContent("RAIN SIMULATION", "Enables rain on this scene"), mTarget.rain);

                GUILayout.Space(12);
                SmallGroup("WIND SETTINGS");
                mTarget.windTurbulence = SliderField(new GUIContent("WIND TURBULENCE", "The global turbulence of the wind for this scene"), mTarget.windTurbulence, 0.0f, 1.0f);
                mTarget.windIntensity = SliderField(new GUIContent("WIND STRENGTH", "The global strength of the wind for this scene"), mTarget.windIntensity, 0.0f, 10.0f);
                GUILayout.Space(8);
                EndSmallGroup();

                GUILayout.Space(12);

                if (mTarget.snow) {
                    SmallGroup("SNOW SETTINGS");
                    mTarget.snowIntensity = SliderField(new GUIContent("SNOW INTENSITY", "The global intensity of the snow for this scene"), mTarget.snowIntensity, 0.0f, 1.0f);
                    GUILayout.Space(8);
                    EndSmallGroup();
                    GUILayout.Space(12);
                }

                if (mTarget.rain) {
                    SmallGroup("RAIN SETTINGS");
                    mTarget.rainIntensity = SliderField(new GUIContent("RAIN INTENSITY", "The global intensity of the rain for this scene"), mTarget.rainIntensity, 0.0f, 1.0f);
                    GUILayout.Space(8);
                    EndSmallGroup();
                    GUILayout.Space(12);
                }

            }
            EndCenteredGroup();

            GUILayout.Space(16);

            GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();
            var lStyle = new GUIStyle();
            lStyle.fontStyle = FontStyle.Italic;
            lStyle.normal.textColor = Color.white;
            lStyle.fontSize = 8;

            GUILayout.Label("Copyright© 2017-2019,   Jorge Pinal N.", lStyle);

            GUILayout.FlexibleSpace(); GUILayout.EndHorizontal();

            GUILayout.Space(24);
            GUILayout.EndVertical(); GUILayout.Space(8); GUILayout.EndHorizontal();


        }



        #region PIDI 2020 EDITOR

        /// <summary>
        /// Draws the asset's logo and its current version
        /// </summary>
        public void AssetLogoAndVersion() {

            GUILayout.BeginVertical(logo, pidiSkin2 ? pidiSkin2.customStyles[1] : null);
            GUILayout.Space(45);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(version, pidiSkin2.customStyles[2]);
            GUILayout.Space(6);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }


        public Color ColorField(GUIContent label, Color currentValue) {

            GUILayout.Space(2);
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, pidiSkin2.label, GUILayout.Width(EditorGUIUtility.labelWidth));
            currentValue = EditorGUILayout.ColorField(currentValue);
            GUILayout.EndHorizontal();
            GUILayout.Space(2);

            return currentValue;

        }



        /// <summary>
        /// Draws a standard object field in the PIDI 2020 style
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="label"></param>
        /// <param name="inputObject"></param>
        /// <param name="allowSceneObjects"></param>
        /// <returns></returns>
        public T ObjectField<T>(GUIContent label, T inputObject, bool allowSceneObjects = true) where T : UnityEngine.Object {

            GUILayout.Space(2);
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, pidiSkin2.label, GUILayout.Width(EditorGUIUtility.labelWidth));
            GUI.color = Color.gray;
            inputObject = (T)EditorGUILayout.ObjectField(inputObject, typeof(T), allowSceneObjects);
            GUI.color = Color.white;
            GUILayout.EndHorizontal();
            GUILayout.Space(2);
            return inputObject;
        }

        public void SmallGroup(string label) {

            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUI.color = new Color(0.7f, 0.75f, 0.85f, 1);
            GUILayout.BeginVertical(pidiSkin2.customStyles[0]);
            GUI.color = Color.white;
            GUILayout.Space(8);
            CenteredLabel(label);
            GUILayout.Space(16);
            GUILayout.BeginHorizontal(); GUILayout.Space(12);
            GUILayout.BeginVertical();

        }


        public void EndSmallGroup() {
            GUILayout.Space(16);
            GUILayout.EndVertical();
            GUILayout.Space(12);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.Space(12);
            GUILayout.EndHorizontal();

        }


        /// <summary>
        /// Draws a centered button in the standard PIDI 2020 editor style
        /// </summary>
        /// <param name="label"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public bool CenteredButton(string label, float width) {
            GUILayout.Space(2);
            GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();
            var tempBool = GUILayout.Button(label, pidiSkin2.customStyles[0], GUILayout.MaxWidth(width));
            GUILayout.FlexibleSpace(); GUILayout.EndHorizontal();
            GUILayout.Space(2);
            return tempBool;
        }


        /// <summary>
        /// Draws the asset's logo and its current version
        /// </summary>


        /// <summary>
        /// Draws a label centered in the Editor window
        /// </summary>
        /// <param name="label"></param>
        public void CenteredLabel(string label) {

            GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();
            GUILayout.Label(label, pidiSkin2.label);
            GUILayout.FlexibleSpace(); GUILayout.EndHorizontal();

        }

        /// <summary>
        /// Begins a custom centered group similar to a foldout that can be expanded with a button
        /// </summary>
        /// <param name="label"></param>
        /// <param name="groupFoldState"></param>
        /// <returns></returns>
        public bool BeginCenteredGroup(string label, ref bool groupFoldState) {

            if (GUILayout.Button(label, pidiSkin2.customStyles[0])) {
                groupFoldState = !groupFoldState;
            }
            GUILayout.BeginHorizontal(); GUILayout.Space(12);
            GUILayout.BeginVertical();
            return groupFoldState;
        }


        /// <summary>
        /// Finishes a centered group
        /// </summary>
        public void EndCenteredGroup() {
            GUILayout.EndVertical();
            GUILayout.Space(12);
            GUILayout.EndHorizontal();
        }



        /// <summary>
        /// Custom integer field following the PIDI 2020 editor skin
        /// </summary>
        /// <param name="label"></param>
        /// <param name="currentValue"></param>
        /// <returns></returns>
        public int IntField(GUIContent label, int currentValue) {

            GUILayout.Space(2);
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, pidiSkin2.label, GUILayout.Width(EditorGUIUtility.labelWidth));
            currentValue = EditorGUILayout.IntField(currentValue, pidiSkin2.customStyles[4]);
            GUILayout.EndHorizontal();
            GUILayout.Space(2);

            return currentValue;
        }



        /// <summary>
        /// Custom float field following the PIDI 2020 editor skin
        /// </summary>
        /// <param name="label"></param>
        /// <param name="currentValue"></param>
        /// <returns></returns>
        public float FloatField(GUIContent label, float currentValue) {

            GUILayout.Space(2);
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, pidiSkin2.label, GUILayout.Width(EditorGUIUtility.labelWidth));
            currentValue = EditorGUILayout.FloatField(currentValue, pidiSkin2.customStyles[4]);
            GUILayout.EndHorizontal();
            GUILayout.Space(2);

            return currentValue;
        }

        public Vector4 Vector4Field(GUIContent label, Vector4 currentValue) {

            GUILayout.Space(2);
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, pidiSkin2.label, GUILayout.Width(EditorGUIUtility.labelWidth));
            currentValue.x = EditorGUILayout.FloatField(currentValue.x, pidiSkin2.customStyles[4]);
            GUILayout.Space(8);
            currentValue.y = EditorGUILayout.FloatField(currentValue.y, pidiSkin2.customStyles[4]);
            GUILayout.Space(8);
            currentValue.z = EditorGUILayout.FloatField(currentValue.z, pidiSkin2.customStyles[4]);
            GUILayout.Space(8);
            currentValue.w = EditorGUILayout.FloatField(currentValue.w, pidiSkin2.customStyles[4]);
            GUILayout.EndHorizontal();
            GUILayout.Space(2);

            return currentValue;

        }


        /// <summary>
        /// Custom slider using the PIDI 2020 editor skin and adding a custom suffix to the float display
        /// </summary>
        /// <param name="label"></param>
        /// <param name="currentValue"></param>
        /// <param name="minSlider"></param>
        /// <param name="maxSlider"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public float SliderField(GUIContent label, float currentValue, float minSlider = 0.0f, float maxSlider = 1.0f, string suffix = "") {

            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, pidiSkin2.label, GUILayout.Width(EditorGUIUtility.labelWidth));
            GUI.color = Color.gray;
            currentValue = GUILayout.HorizontalSlider(currentValue, minSlider, maxSlider, GUI.skin.horizontalSlider, GUI.skin.horizontalSliderThumb);
            GUI.color = Color.white;
            GUILayout.Space(12);
            currentValue = Mathf.Clamp(EditorGUILayout.FloatField(float.Parse(currentValue.ToString("n2")), pidiSkin2.customStyles[4], GUILayout.MaxWidth(40)), minSlider, maxSlider);
            GUILayout.EndHorizontal();
            GUILayout.Space(4);

            return currentValue;
        }


        /// <summary>
        /// Draw a custom popup field in the PIDI 2020 style
        /// </summary>
        /// <param name="label"></param>
        /// <param name="toggleValue"></param>
        /// <returns></returns>
        public int PopupField(GUIContent label, int selected, string[] options) {


            GUILayout.Space(2);
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, pidiSkin2.label, GUILayout.Width(EditorGUIUtility.labelWidth));
            selected = EditorGUILayout.Popup(selected, options, pidiSkin2.customStyles[0]);
            GUILayout.EndHorizontal();
            GUILayout.Space(2);
            return selected;
        }



        /// <summary>
        /// Draw a custom toggle that instead of using a check box uses an Enable/Disable drop down menu
        /// </summary>
        /// <param name="label"></param>
        /// <param name="toggleValue"></param>
        /// <returns></returns>
        public bool EnableDisableToggle(GUIContent label, bool toggleValue) {

            int option = toggleValue ? 1 : 0;

            GUILayout.Space(2);
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, pidiSkin2.label, GUILayout.Width(EditorGUIUtility.labelWidth));
            option = EditorGUILayout.Popup(option, new string[] { "DISABLED", "ENABLED" }, pidiSkin2.customStyles[0]);
            GUILayout.EndHorizontal();
            GUILayout.Space(2);
            return option == 1;
        }


        /// <summary>
        /// Draw an enum field but changing the labels and names of the enum to Upper Case fields
        /// </summary>
        /// <param name="label"></param>
        /// <param name="userEnum"></param>
        /// <returns></returns>
        public int UpperCaseEnumField(GUIContent label, System.Enum userEnum) {

            var names = System.Enum.GetNames(userEnum.GetType());

            for (int i = 0; i < names.Length; i++) {
                names[i] = System.Text.RegularExpressions.Regex.Replace(names[i], "(\\B[A-Z])", " $1").ToUpper();
            }

            GUILayout.Space(2);
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, pidiSkin2.label, GUILayout.Width(EditorGUIUtility.labelWidth));
            var result = EditorGUILayout.Popup(System.Convert.ToInt32(userEnum), names, pidiSkin2.customStyles[0]);
            GUILayout.EndHorizontal();
            GUILayout.Space(2);
            return result;
        }




        #endregion


    }

#endif

}