/*
XFur Studio™ v 1.2

You cannot sell, redistribute, share nor make public this code, even modified, through any means on any platform.
Modifications are allowed only for your own use and to make this product suit better your project's needs.
These modifications may not be redistributed, sold or shared in any way.

For more information, contact us at contact@irreverent-software.com

Copyright© 2018, Jorge Pinal Negrete. All Rights Reserved.
*/

using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using XFurStudio;
using System;

public class XFur_ShaderEditor : ShaderGUI{

    private Material targetMat;
    public bool[] folds;

    public GUISkin pidiSkin2;
    public Texture2D logo;

    private string version = "1.9";

    void EnableGUI(){
        folds = new bool[24];
        
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties){
        if (materialEditor == null || materialEditor.target == null || materialEditor.targets == null )
            return;

        if ( !targetMat ){
            targetMat = materialEditor.target as Material;
            if (!targetMat)
                return;
            EnableGUI();
        }

        pidiSkin2 = AssetDatabase.LoadAssetAtPath<GUISkin>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("l:XFurGUI")[0]));
        logo = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("XFur Studio Logo")[0]));


        if ( targetMat && targetMat.name.Contains(" Samples") || targetMat.name.Contains("Pass")) {
            GUILayout.BeginHorizontal();GUILayout.Space(12);
            EditorGUILayout.HelpBox("This material is a managed instanced material used internally by XFur. Its properties cannot be edited directly. Please either select the original material or edit the per instance properties to modify this material's appearance", MessageType.Info );
            GUILayout.Space(12);
            GUILayout.EndHorizontal();
            return;
        }
        else{
            if ( !targetMat) {
                return;
            }

            Undo.RecordObject(targetMat, "XFUR SHADER EDITOR ID"+targetMat.GetInstanceID());

            
            
            GUILayout.BeginHorizontal();GUILayout.BeginVertical(pidiSkin2.box);
            AssetLogoAndVersion();
            GUILayout.Space(8);


            if (BeginCenteredGroup("BASE PROPERTIES", ref folds[0])) {
                GUILayout.Space(16);
                targetMat.SetColor("_BaseColor", ColorField(new GUIContent("MAIN COLOR", "The final tint of the mesh under the fur"), targetMat.GetColor("_BaseColor")));
                if (targetMat.GetFloat("_HasGlossMap") == 0)
                    targetMat.SetColor("_BaseSpecular", ColorField(new GUIContent("SPECULAR COLOR", "The specular color of the mesh under the fur"), targetMat.GetColor("_BaseSpecular")));

                targetMat.SetTexture("_BaseTex", ObjectField<Texture2D>(new GUIContent("MAIN TEXTURE", "The texture that will be applied to the mesh under the fur"), (Texture2D)targetMat.GetTexture("_BaseTex"), false));
                targetMat.SetTexture("_GlossSpecular", ObjectField<Texture2D>(new GUIContent("MAIN SPECULAR MAP", "The texture that will control the Specular Color (RGB) and glossiness (A) of the mesh under the fur"), (Texture2D)targetMat.GetTexture("_GlossSpecular"), false));

                if (targetMat.GetFloat("_HasGlossMap") == 0)
                    targetMat.SetFloat("_BaseSmoothness", SliderField(new GUIContent("SMOOTHNESS", "The glossiness of the mesh under the fur"), targetMat.GetFloat("_BaseSmoothness"), 0, 1));

                targetMat.SetFloat("_HasGlossMap", targetMat.GetTexture("_GlossSpecular") ? 1 : 0);
                targetMat.SetTexture("_Normalmap", ObjectField<Texture2D>(new GUIContent("NORMALMAP", "The normalmap that will be applied to the mesh under the fur"), (Texture2D)targetMat.GetTexture("_Normalmap"), false));
                targetMat.SetTexture("_OcclusionMap", ObjectField<Texture2D>(new GUIContent("OCCLUSION MAP", "The occlusion map that will be applied to the mesh under the fur"), (Texture2D)targetMat.GetTexture("_OcclusionMap"), false));

                GUILayout.Space(4);

                targetMat.SetFloat("_UV0Scale1", FloatField(new GUIContent("UV 0 SCALE", "The scale of the main UV coordinates channel"), targetMat.GetFloat("_UV0Scale1")));

                GUILayout.Space(16);

            }

            EndCenteredGroup();

            if ( BeginCenteredGroup("FUR PROPERTIES", ref folds[1]) ){
                GUILayout.Space(16);
                if (!targetMat) {
                    EndCenteredGroup();
                    return;
                }

                targetMat.SetFloat("_Cull", EnableDisableToggle( new GUIContent("DOUBLE SIDED FUR"), targetMat.GetFloat("_Cull")==0 )?0:2);

                var t = targetMat.IsKeywordEnabled("ANISOTROPIC_ON");
                t = EnableDisableToggle( new GUIContent("ANISOTROPIC SPECULAR"), t );
                
                if ( t ){
                    targetMat.EnableKeyword( "ANISOTROPIC_ON" );
                    targetMat.SetFloat("_AnisotropicOffset", SliderField( new GUIContent("ANISOTROPIC OFFSET"), targetMat.GetFloat("_AnisotropicOffset"), -1.0f, 1.0f ) );
                }
                else{
                    targetMat.DisableKeyword("ANISOTROPIC_ON");
                }

                targetMat.SetFloat("_TriplanarMode", EnableDisableToggle(new GUIContent("TRIPLANAR MODE"), targetMat.GetFloat("_TriplanarMode")==1)?1:0);
                if ( targetMat.GetFloat("_TriplanarMode") == 1 ){
                    targetMat.SetFloat("_TriplanarScale", FloatField(new GUIContent("TRIPLANAR SCALE"), targetMat.GetFloat("_TriplanarScale") ) );
                }
                targetMat.SetFloat("_LocalWindStrength", SliderField(new GUIContent("WIND STRENGTH","The influence the wind will have over this material"), targetMat.GetFloat("_LocalWindStrength"), 0, 64 ) );
                GUILayout.Space(4);
                targetMat.SetTexture( "_FurColorMap", ObjectField<Texture2D>( new GUIContent("FUR COLOR MAP", "The diffuse texture to be used by the fur"), (Texture2D)targetMat.GetTexture("_FurColorMap"), false ) );
                targetMat.SetTexture( "_FurData0", ObjectField<Texture2D>( new GUIContent("FUR DATA 0", "The fur data map and mask"), (Texture2D)targetMat.GetTexture("_FurData0"), false ) );
                targetMat.SetTexture( "_FurData1", ObjectField<Texture2D>( new GUIContent("FUR DATA 1", "The Directional/Stiffness map for this material"), (Texture2D)targetMat.GetTexture("_FurData1"), false ) );
                targetMat.SetTexture( "_FurNoiseMap", ObjectField<Texture2D>( new GUIContent("FUR GEN MAP", "The noise based multi-layer texture used for fur generation"), (Texture2D)targetMat.GetTexture("_FurNoiseMap"), false ) );
                
                GUILayout.Space(4);
                targetMat.SetColor( "_FurColorA", ColorField( new GUIContent("MAIN FUR COLOR", "The main final tint of the fur"), targetMat.GetColor("_FurColorA") ) );
                targetMat.SetColor( "_FurColorB", ColorField( new GUIContent("SECONDARY FUR COLOR", "The secondary final tint of the fur"), targetMat.GetColor("_FurColorB") ) );
                targetMat.SetColor( "_FurSpecular", ColorField( new GUIContent("FUR SPECULAR", "The specular color of the fur"), targetMat.GetColor("_FurSpecular") ) );
                GUILayout.Space(4);

                targetMat.SetColor( "_RimColor", ColorField(new GUIContent("FUR RIM COLOR"), targetMat.GetColor("_RimColor")));
                targetMat.SetFloat("_FurRimStrength",SliderField(new GUIContent("RIM POWER"), targetMat.GetFloat("_FurRimStrength"), 0, 1 ) );

                if ( targetMat.GetTexture("_FurNoiseMap") ){
                    targetMat.SetFloat("_FurSmoothness", SliderField( new GUIContent("FUR SMOOTHNESS","The smoothness to be applied to the generated fur"), targetMat.GetFloat("_FurSmoothness"), 0, 1 ) );
                    targetMat.SetFloat("_FurCutoff", SliderField( new GUIContent("FUR CUTOFF","The alpha cutoff to be applied to the generated fur"), targetMat.GetFloat("_FurCutoff"), 0, 1 ) );
                    targetMat.SetFloat("_FurOcclusion", SliderField( new GUIContent("FUR OCCLUSION", "The occlusion of the fur"), targetMat.GetFloat("_FurOcclusion"), 0, 1 ));
                    targetMat.SetFloat("_FurLength", SliderField( new GUIContent("FUR LENGTH","The length of the fur"), targetMat.GetFloat("_FurLength"), 0, 4 ) );
                    targetMat.SetFloat("_FurThin", SliderField( new GUIContent("FUR THICKNESS","Make each fur strand look finer"), targetMat.GetFloat("_FurThin"), 0, 1 ) );
                }

                targetMat.SetVector("_FurDirection", Vector4Field( new GUIContent("FUR DIRECTION"), targetMat.GetVector("_FurDirection") ) );

                GUILayout.Space(4);

                GUILayout.Space(4);
                
                
                
                targetMat.SetFloat("_UV1Scale1", FloatField( new GUIContent("FUR UV SCALE 1", "The scale of the first fur UV coordinates channel"), targetMat.GetFloat("_UV1Scale1") ) );
                targetMat.SetFloat("_UV1Scale2", FloatField( new GUIContent("FUR UV SCALE 2", "The scale of the second fur UV coordinates channel"), targetMat.GetFloat("_UV1Scale2") ) );
                
                
                GUILayout.Space(16);
            }
            EndCenteredGroup();
            
            if ( BeginCenteredGroup("ADDITIONAL SETTINGS", ref folds[2])){
                GUILayout.Space(16);
                targetMat.SetTexture("_FurFXNoise", ObjectField<Texture2D>( new GUIContent("FX PARAM. NOISE","The parametric noise texture used to generate and control fx over the fur"), (Texture2D)targetMat.GetTexture("_FurFXNoise"), false ) );
                targetMat.SetFloat("_UV0Scale2", FloatField( new GUIContent("FX UV SCALE"), targetMat.GetFloat("_UV0Scale2")));
                GUILayout.Space(16);
            }
            EndCenteredGroup();
            
            GUILayout.Space(8);

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