/*
XFur Studio™ - XFur Generic Module
Copyright© 2018, Jorge Pinal Negrete. All Rights Reserved
*/

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AnimatedValues;
#endif

namespace XFurStudio{
    
    [System.Serializable]
    public class XFur_SystemModule {
        
        [SerializeField]protected XFurModuleState moduleState = XFurModuleState.AssetMode;
        [SerializeField]protected string moduleName;
        public XFur_System systemOwner;
        protected GUISkin pidiSkin2;
        
        
        public virtual void Module_Start(XFur_System owner){
            systemOwner = owner;
        }

        public virtual void Module_Execute(){

        }

        public virtual void Module_OnRender(){
            
        }

        public virtual void Module_UpdateFurData( ref MaterialPropertyBlock m ){

        }

        public virtual void Module_InstancedFurData( Material mat ){
            
        }

        public virtual void Module_End(){

        }

        #if UNITY_EDITOR
        
        public virtual void Module_StartUI( GUISkin editorSkin ){
           
            pidiSkin2 = editorSkin;           

        }
        
        public virtual void Module_UI(XFur_System owner){
            moduleState = EnableDisableToggle( new GUIContent("ENABLE MODULE FUNCTIONS"), moduleState==XFurModuleState.Enabled?true:false )?XFurModuleState.Enabled:XFurModuleState.Disabled;
            systemOwner = owner;
        }

        public virtual void Module_UI( SerializedObject serialized ){

        }
        #endif
        

        #region Access Interfaces

        public string ModuleName{
            get{
                return moduleName;
            }
        }

        public XFurModuleState State{
            get{
                return moduleState;
            }
        }


        public bool Enabled{
            get{
                return moduleState==XFurModuleState.Enabled;
            }
        }

        #endregion

#if UNITY_EDITOR

        #region PIDI 2020 EDITOR


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
        /// Draws a standard button in the standard PIDI 2020 editor style
        /// </summary>
        /// <param name="label"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public bool StandardButton(string label, float width) {
            GUILayout.Space(2);
            var tempBool = GUILayout.Button(label, pidiSkin2.customStyles[0], GUILayout.MaxWidth(width));
            GUILayout.Space(2);
            return tempBool;
        }



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


        public Vector2 Vector2Field(GUIContent label, Vector2 currentValue) {

            GUILayout.Space(2);
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, pidiSkin2.label, GUILayout.Width(EditorGUIUtility.labelWidth));
            currentValue.x = EditorGUILayout.FloatField(currentValue.x, pidiSkin2.customStyles[4]);
            GUILayout.Space(8);
            currentValue.y = EditorGUILayout.FloatField(currentValue.y, pidiSkin2.customStyles[4]);
            GUILayout.EndHorizontal();
            GUILayout.Space(2);

            return currentValue;

        }


        public Vector3 Vector3Field(GUIContent label, Vector3 currentValue) {

            GUILayout.Space(2);
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, pidiSkin2.label, GUILayout.Width(EditorGUIUtility.labelWidth));
            currentValue.x = EditorGUILayout.FloatField(currentValue.x, pidiSkin2.customStyles[4]);
            GUILayout.Space(8);
            currentValue.y = EditorGUILayout.FloatField(currentValue.y, pidiSkin2.customStyles[4]);
            GUILayout.Space(8);
            currentValue.z = EditorGUILayout.FloatField(currentValue.z, pidiSkin2.customStyles[4]);
            GUILayout.EndHorizontal();
            GUILayout.Space(2);

            return currentValue;

        }



        /// <summary>
        /// Custom int slider using the PIDI 2020 editor skin and adding a custom suffix to the float display
        /// </summary>
        /// <param name="label"></param>
        /// <param name="currentValue"></param>
        /// <param name="minSlider"></param>
        /// <param name="maxSlider"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public int IntSliderField(GUIContent label, int currentValue, int minSlider = 0, int maxSlider = 10) {

            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, pidiSkin2.label, GUILayout.Width(EditorGUIUtility.labelWidth));
            currentValue = (int)GUILayout.HorizontalSlider(currentValue, minSlider, maxSlider, pidiSkin2.horizontalSlider, pidiSkin2.horizontalSliderThumb);
            GUILayout.Space(12);
            currentValue = Mathf.Clamp(EditorGUILayout.IntField(currentValue, pidiSkin2.customStyles[4], GUILayout.MaxWidth(40)), minSlider, maxSlider);
            GUILayout.EndHorizontal();
            GUILayout.Space(4);

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
            currentValue = GUILayout.HorizontalSlider(currentValue, minSlider, maxSlider, pidiSkin2.horizontalSlider, pidiSkin2.horizontalSliderThumb);
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


            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, pidiSkin2.label, GUILayout.Width(EditorGUIUtility.labelWidth));
            selected = EditorGUILayout.Popup(selected, options, pidiSkin2.customStyles[0]);
            GUILayout.EndHorizontal();
            GUILayout.Space(4);
            return selected;
        }


        /// <summary>
        /// Draw a custom popup field in the PIDI 2020 style
        /// </summary>
        /// <param name="label"></param>
        /// <param name="toggleValue"></param>
        /// <returns></returns>
        public int PopupField(GUIContent label, int selected, GUIContent[] options) {


            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, pidiSkin2.label, GUILayout.Width(EditorGUIUtility.labelWidth));
            selected = EditorGUILayout.Popup(selected, options, pidiSkin2.customStyles[0]);
            GUILayout.EndHorizontal();
            GUILayout.Space(4);
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

            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, pidiSkin2.label, GUILayout.Width(EditorGUIUtility.labelWidth));
            option = EditorGUILayout.Popup(option, new string[] { "DISABLED", "ENABLED" }, pidiSkin2.customStyles[0]);
            GUILayout.EndHorizontal();
            GUILayout.Space(4);
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


        /// <summary>
        /// Draw a layer mask field in the PIDI 2020 style
        /// </summary>
        /// <param name="label"></param>
        /// <param name="selected"></param>
        public LayerMask LayerMaskField(GUIContent label, LayerMask selected) {

            List<string> layers = null;
            string[] layerNames = null;

            if (layers == null) {
                layers = new List<string>();
                layerNames = new string[4];
            }
            else {
                layers.Clear();
            }

            int emptyLayers = 0;
            for (int i = 0; i < 32; i++) {
                string layerName = LayerMask.LayerToName(i);

                if (layerName != "") {

                    for (; emptyLayers > 0; emptyLayers--) layers.Add("Layer " + (i - emptyLayers));
                    layers.Add(layerName);
                }
                else {
                    emptyLayers++;
                }
            }

            if (layerNames.Length != layers.Count) {
                layerNames = new string[layers.Count];
            }
            for (int i = 0; i < layerNames.Length; i++) layerNames[i] = layers[i];

            GUILayout.BeginHorizontal();
            GUILayout.Label(label, pidiSkin2.label, GUILayout.Width(EditorGUIUtility.labelWidth));

            selected.value = EditorGUILayout.MaskField(selected.value, layerNames, pidiSkin2.customStyles[0]);

            GUILayout.EndHorizontal();

            return selected;
        }


        #endregion

#endif

    }

}