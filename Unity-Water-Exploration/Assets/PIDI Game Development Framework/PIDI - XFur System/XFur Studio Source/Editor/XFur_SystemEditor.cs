/*
XFur Studio™ - XFur System Editor
Copyright© 2018, Jorge Pinal Negrete. All Rights Reserved
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AnimatedValues;
#endif


namespace XFurStudio {

    [CustomEditor(typeof(XFur_System), true)]



    public class XFur_SystemEditor : Editor {

        public static GUISkin pidiSkin;
        public GUISkin pidiSkin2;

        public Texture2D logo;

        public XFur_System xFur;


        void OnEnable() {


        }

        public override void OnInspectorGUI() {
            xFur = (XFur_System)target;
            


            if (xFur.coatingModule != null) {
                xFur.coatingModule.Module_StartUI(pidiSkin2);
            }


            if (xFur.lodModule != null) {
                xFur.lodModule.Module_StartUI(pidiSkin2);
            }


            if (xFur.physicsModule != null) {
                xFur.physicsModule.Module_StartUI(pidiSkin2);
            }


            if (xFur.fxModule != null) {
                xFur.fxModule.Module_StartUI(pidiSkin2);
            }

            Undo.RecordObject(xFur, "XFUR_" + xFur.GetInstanceID());

            

            var tSkin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
            GUI.skin = pidiSkin;



            var buffDatabase = xFur.database;

            GUILayout.BeginHorizontal(); GUILayout.BeginVertical(pidiSkin2.box);
            AssetLogoAndVersion();
            GUILayout.Space(8);
            GUILayout.BeginHorizontal();GUILayout.Space(12);GUILayout.BeginVertical();
            xFur.database = ObjectField<XFur_DatabaseModule>(new GUIContent("XFUR DATABASE ASSET", "The XFur Database asset, required for LOD management, mesh handling and most other internal features"), xFur.database, false);
            GUILayout.Space(8);

            if (buffDatabase != xFur.database && xFur.database != null) {
                xFur.XFur_UpdateMeshData();
                xFur.XFur_Start();
            }

            var tempTarget = xFur.manualMeshIndex;

            xFur.manualMeshIndex = EnableDisableToggle(new GUIContent("EXPLICIT MODEL INDEX", "Use an explicit index on the database to get the mesh data from instead of automatically linking XFur to the database through Mesh ID. In most cases, you should leave this toggle disabled"), xFur.manualMeshIndex!=-1 )?Mathf.Max(xFur.manualMeshIndex,0):-1; 
        
            if ( xFur.manualMeshIndex != -1) {
                if (xFur.database.meshData.Length < 1) {
                    xFur.manualMeshIndex = -1;
                }
                xFur.manualMeshIndex = PopupField(new GUIContent("EXPLICIT DATABASE INDEX"), xFur.manualMeshIndex, xFur.database.GetMeshNames());
                xFur.manualMeshIndex = Mathf.Clamp(xFur.manualMeshIndex, 0, xFur.database.meshData.Length-1);
            }

            if ( tempTarget != xFur.manualMeshIndex) {
                xFur.XFur_UpdateMeshData();
            }

            if (!xFur.database) {
                GUILayout.BeginHorizontal(); GUILayout.Space(12);
                EditorGUILayout.HelpBox("Please assign the XFur Database Asset to this component. It is required for internal functions and its absence can make this software unstable", MessageType.Error);
                GUILayout.Space(12); GUILayout.EndHorizontal();
                GUILayout.Space(8);

                GUILayout.EndVertical(); GUILayout.Space(12);
                GUILayout.EndHorizontal();
            }
            else {

                if (xFur.database.XFur_ContainsMeshData(xFur.OriginalMesh) != -1) {
                    if (xFur.database.meshData[xFur.database.XFur_ContainsMeshData(xFur.OriginalMesh)].XFurVersion != xFur.Version) {
                        GUILayout.BeginHorizontal(); GUILayout.Space(12);
                        EditorGUILayout.HelpBox("The current XFur version you are using does not match the version of the data stored in the database asset. You need to rebuild this data", MessageType.Error);
                        GUILayout.Space(12); GUILayout.EndHorizontal();
                        GUILayout.Space(8);
                        
                        if (CenteredButton("REBUILD DATA", 200)) {
                            xFur.database.XFur_DeleteMeshData(xFur.database.XFur_ContainsMeshData(xFur.OriginalMesh));
                            xFur.XFur_UpdateMeshData();
                        }

                        GUILayout.Space(8);
                    }
                    else {
                        GUILayout.Space(8);
                        if (CenteredButton("REBUILD DATA", 200)) {
                            xFur.database.XFur_DeleteMeshData(xFur.database.XFur_ContainsMeshData(xFur.OriginalMesh));
                            xFur.XFur_UpdateMeshData();
                        }

                        GUILayout.Space(8);
                    }
                }

                GUILayout.EndVertical();GUILayout.Space(12);
                GUILayout.EndHorizontal();

                if (BeginCenteredGroup("XFUR - MAIN SETTINGS", ref xFur.folds[0])) {

                    GUILayout.Space(16);

                    xFur.updateDynamically = EnableDisableToggle(new GUIContent("DYNAMIC UPDATES", "Enable this if you plan on making dynamic changes to the fur at runtime. If you are using the FX or Physics module, disable it for better performance."), xFur.updateDynamically);

                    xFur.materialProfileIndex = PopupField(new GUIContent("MATERIAL SLOT", "The material we are currently editing"), xFur.materialProfileIndex, xFur.FurMatGUIS);

                    GUILayout.Space(8);
                    xFur.materialProfiles[xFur.materialProfileIndex].originalMat = ObjectField<Material>(new GUIContent("BASE FUR MATERIAL", "The fur material that will be used as a reference by this instance"), xFur.materialProfiles[xFur.materialProfileIndex].originalMat, false);
                    GUILayout.Space(8);

                    if (xFur.materialProfiles[xFur.materialProfileIndex].buffOriginalMat != xFur.materialProfiles[xFur.materialProfileIndex].originalMat || xFur.GetComponent<Renderer>().sharedMaterials[xFur.materialProfileIndex] == null || !xFur.GetComponent<Renderer>().sharedMaterials[xFur.materialProfileIndex].name.Contains(xFur.materialProfiles[xFur.materialProfileIndex].originalMat.name) || !xFur.GetComponent<Renderer>().sharedMaterials[xFur.materialProfileIndex].shader.name.Contains("XFUR")) {
                        xFur.UpdateSharedData(xFur.materialProfiles[xFur.materialProfileIndex]);
                        xFur.materialProfiles[xFur.materialProfileIndex].SynchToOriginalMat();
                        xFur.XFur_UpdateMeshData();
                    }

                    GUILayout.Space(4);
                    if (xFur.materialProfiles[xFur.materialProfileIndex].originalMat && xFur.materialProfiles[xFur.materialProfileIndex].furmatType == 2) {

                        if ((!Application.isPlaying || !xFur.instancedMode) && xFur.materialProfiles[xFur.materialProfileIndex].furmatShadowsMode == 0) {
                            if (XFur_System.materialReferences.ContainsKey(xFur.materialProfiles[xFur.materialProfileIndex].originalMat)) {
                                var samples = new List<GUIContent>();


                                for (int i = 0; i < xFur.database.highQualityShaders.Length; i++) {
                                    var shaderName = XFur_System.materialReferences[xFur.materialProfiles[xFur.materialProfileIndex].originalMat][i].shader.name.ToUpper();
                                    samples.Add(new GUIContent(shaderName.Split("/"[0])[shaderName.Split("/"[0]).Length - 1]));
                                }

                                if (xFur.lodModule.State == XFurModuleState.Enabled) {
                                    samples.Clear();
                                    samples.Add(new GUIContent("LOD DRIVEN"));
                                    var n = 0;
                                    n = PopupField(new GUIContent("FUR SAMPLES", "The amount of samples to use on this shader"), n, new GUIContent[] { new GUIContent("LOD Driven") });

                                    if (!Application.isPlaying)
                                        xFur.materialProfiles[xFur.materialProfileIndex].furmatSamples = 2;
                                }
                                else {
                                    xFur.materialProfiles[xFur.materialProfileIndex].furmatSamples = PopupField(new GUIContent("FUR SAMPLES", "The amount of samples to use on this shader"), xFur.materialProfiles[xFur.materialProfileIndex].furmatSamples, samples.ToArray());
                                }

                            }
                        }
                        else {
                            if ((xFur.lodModule == null || (xFur.lodModule != null && xFur.lodModule.State != XFurModuleState.Enabled)) && xFur.runMaterialReferences.ContainsKey(xFur.materialProfiles[xFur.materialProfileIndex].originalMat)) {
                                var samples = new List<GUIContent>();


                                if (xFur.lodModule.State == XFurModuleState.Enabled) {
                                    samples.Clear();
                                    samples.Add(new GUIContent("LOD DRIVEN"));
                                }

                                xFur.materialProfiles[xFur.materialProfileIndex].furmatSamples = PopupField(new GUIContent("FUR SAMPLES", "The amount of samples to use on this shader"), xFur.materialProfiles[xFur.materialProfileIndex].furmatSamples, samples.ToArray());
                            }
                            else {
                                var n = 0;
                                n = PopupField(new GUIContent("FUR SAMPLES", "The amount of samples to use on this shader"), n, new GUIContent[] { new GUIContent("LOD Driven") });
                            }
                        }

                        var s = xFur.materialProfiles[xFur.materialProfileIndex].furmatShadowsMode;

                        xFur.materialProfiles[xFur.materialProfileIndex].furmatShadowsMode = PopupField(new GUIContent("SHADOWS MODE", "Switches between the different shadow modes for the fur"), xFur.materialProfiles[xFur.materialProfileIndex].furmatShadowsMode, new GUIContent[] { new GUIContent("STANDARD SHADOWS", "Simple shadow casting on forward and deferred with full shadow reception enabled only for deferred rendering"), new GUIContent("(BETA) FULL SHADOWS", "Expensive method of full shadowing in forward and deferred that adds accurate shadows in casting and receiving mode based on each fur strand and layer") });

                        if (s == 1) {
                            GUILayout.BeginHorizontal(); GUILayout.Space(12);
                            EditorGUILayout.HelpBox("Full shadows generate additional geometry and sub renderers, making it VERY expensive to compute. DO NOT use them on scenes with more than a couple characters nor in models with more than 6-10k polygons", MessageType.Warning);
                            GUILayout.Space(12); GUILayout.EndHorizontal();
                            GUILayout.Space(8);

                            var tempF = (int)xFur.materialProfiles[xFur.materialProfileIndex].fullShadowLayers;
                            tempF = IntSliderField(new GUIContent("FUR SAMPLES"), tempF, 6, 32);
                            xFur.materialProfiles[xFur.materialProfileIndex].fullShadowLayers = tempF;



                        }

                        if (s != xFur.materialProfiles[xFur.materialProfileIndex].furmatShadowsMode && xFur.materialProfiles[xFur.materialProfileIndex].furmatShadowsMode == 1) {
                            xFur.XFur_GenerateShadowMesh(xFur.materialProfiles[xFur.materialProfileIndex]);
                        }


                        if (xFur.FurMaterials == 1
#if UNITY_2018_1_OR_NEWER
                    || true
#endif
                ) {

                            GUILayout.Space(8);

                            //EnableDisableToggle( new GUIContent("Static Material", "If this fur material will not change its length, thickness, color, etc. at runtime it is recommended to toggle this value to handle the material as a static instance for better performance" ), ref xFur.instancedMode );
                            xFur.materialProfiles[xFur.materialProfileIndex].furmatCollision = EnableDisableToggle(new GUIContent("BASIC SELF-COLLISION", "Performs a basic self-collision algorithm on the shader to avoid (with a low precision) the fur from going inside the mesh"), xFur.materialProfiles[xFur.materialProfileIndex].furmatCollision == 1) ? 1 : 0;
                            xFur.materialProfiles[xFur.materialProfileIndex].furmatRenderSkin = EnableDisableToggle(new GUIContent("BASE SKIN PASS", "Render the skin layer under the fur for this material"), xFur.materialProfiles[xFur.materialProfileIndex].furmatRenderSkin == 1) ? 1 : 0;
                            xFur.materialProfiles[xFur.materialProfileIndex].furmatTriplanar = EnableDisableToggle(new GUIContent("TRIPLANAR MODE", "Render fur using triplanar coordinates generated at runtime instead of the secondary UV channel of this mesh"), xFur.materialProfiles[xFur.materialProfileIndex].furmatTriplanar == 1) ? 1 : 0;
                            xFur.materialProfiles[xFur.materialProfileIndex].furmatForceUV2Grooming = EnableDisableToggle(new GUIContent("GROOM MAP ON UV2", "Forces triplanar coordinates to be used for fur projection, using the secondary UV map as coordinates for grooming instead"), xFur.materialProfiles[xFur.materialProfileIndex].furmatForceUV2Grooming == 1) ? 1 : 0;

                            if (xFur.materialProfiles[xFur.materialProfileIndex].furmatForceUV2Grooming == 1) {
                                xFur.materialProfiles[xFur.materialProfileIndex].furmatTriplanar = 1;
                            }

                            GUILayout.Space(8);

                            if (xFur.materialProfiles[xFur.materialProfileIndex].furmatTriplanar == 1) {
                                xFur.materialProfiles[xFur.materialProfileIndex].furmatTriplanarScale = SliderField(new GUIContent("TRIPLANAR SCALE", "The scale used for the triplanar projection of the fur, multiplied by the fur UV1 and UV2 channels' sizes"), xFur.materialProfiles[xFur.materialProfileIndex].furmatTriplanarScale, 0.0f, 1.0f);
                            }


                            GUILayout.Space(8);

                            xFur.materialProfiles[xFur.materialProfileIndex].furmatReadBaseSkin = PopupField(new GUIContent("BASE SKIN MODE", "The mode in which the skin color and specularity are controlled for this instance"), xFur.materialProfiles[xFur.materialProfileIndex].furmatReadBaseSkin, new GUIContent[] { new GUIContent("FROM MATERIAL"), new GUIContent("FROM INSTANCE") });
                            xFur.materialProfiles[xFur.materialProfileIndex].furmatReadBaseFur = PopupField(new GUIContent("FUR SETTINGS MODE", "The mode in which the fur parameters are controlled for this instance"), xFur.materialProfiles[xFur.materialProfileIndex].furmatReadBaseFur, new GUIContent[] { new GUIContent("FROM MATERIAL"), new GUIContent("FROM INSTANCE") });
                            xFur.materialProfiles[xFur.materialProfileIndex].furmatReadFurNoise = PopupField(new GUIContent("FUR GEN MAP", "The mode in which the fur noise map is controlled for this instance"), xFur.materialProfiles[xFur.materialProfileIndex].furmatReadFurNoise, new GUIContent[] { new GUIContent("FROM MATERIAL"), new GUIContent("FROM INSTANCE") });

                            GUILayout.Space(8);

                            XFur_CoatingProfile t = null;
                            t = ObjectField<XFur_CoatingProfile>(new GUIContent("LOAD FUR PROFILE", "Allows you to assign a pre-made fur profile to easily load existing settings, colors, etc"), t, false);

                            if (t != null) {
                                xFur.LoadXFurProfileAsset(t, xFur.materialProfileIndex);
                            }

                            GUILayout.Space(16);
                            if (CenteredButton("EXPORT FUR SETTINGS", 200)) {
                                var k = xFur.XFur_ExportMaterialProfile(xFur.materialProfiles[xFur.materialProfileIndex]);
                                if (k) {
                                    var path = EditorUtility.SaveFilePanelInProject("Save Fur Profile", "New Fur Profile", "asset", "");
                                    AssetDatabase.CreateAsset(k, path);
                                    AssetDatabase.SaveAssets();
                                    AssetDatabase.Refresh();
                                }
                            }
                            GUILayout.Space(16);

                            if (xFur.materialProfiles[xFur.materialProfileIndex].furmatReadBaseSkin != 0) {
                                GUILayout.Space(4);
                                SmallGroup("BASE SKIN");
                                xFur.materialProfiles[xFur.materialProfileIndex].furmatBaseColor = ColorField(new GUIContent("MAIN COLOR", "Final tint to be applied to the skin under the fur"), xFur.materialProfiles[xFur.materialProfileIndex].furmatBaseColor);

                                if (!xFur.materialProfiles[xFur.materialProfileIndex].furmatGlossSpecular) {
                                    xFur.materialProfiles[xFur.materialProfileIndex].furmatSpecular = ColorField(new GUIContent("SPECULAR COLOR", "Specular color to be applied to the skin under the fur"), xFur.materialProfiles[xFur.materialProfileIndex].furmatSpecular);
                                }

                                xFur.materialProfiles[xFur.materialProfileIndex].furmatBaseTex = ObjectField<Texture2D>(new GUIContent("MAIN TEXTURE", "Texture to be applied to the mesh under the fur"), xFur.materialProfiles[xFur.materialProfileIndex].furmatBaseTex, false);

                                xFur.materialProfiles[xFur.materialProfileIndex].furmatGlossSpecular = ObjectField<Texture2D>(new GUIContent("SPECULAR MAP", "Base Specular (RGB) and Smoothness (A) map to be used under the fur"), xFur.materialProfiles[xFur.materialProfileIndex].furmatGlossSpecular, false);
                                GUILayout.Space(4);

                                if (!xFur.materialProfiles[xFur.materialProfileIndex].furmatGlossSpecular) {
                                    xFur.materialProfiles[xFur.materialProfileIndex].furmatSmoothness = SliderField(new GUIContent("SMOOTHNESS", "Smoothness to be applied to the skin under the fur"), xFur.materialProfiles[xFur.materialProfileIndex].furmatSmoothness, 0.0f, 1.0f);
                                }

                                GUILayout.Space(4);

                                xFur.materialProfiles[xFur.materialProfileIndex].furmatNormalmap = ObjectField<Texture2D>(new GUIContent("NORMALMAP", "The normalmap to be applied to the skin under the fur"), xFur.materialProfiles[xFur.materialProfileIndex].furmatNormalmap, false);
                                xFur.materialProfiles[xFur.materialProfileIndex].furmatOcclusion = ObjectField<Texture2D>(new GUIContent("OCCLUSION MAP", "The occlusion map to be applied to the skin under the fur"), xFur.materialProfiles[xFur.materialProfileIndex].furmatOcclusion, false);

                                GUILayout.Space(8);
                                EndSmallGroup();
                            }

                            GUILayout.Space(16);

                            if (xFur.materialProfiles[xFur.materialProfileIndex].furmatReadBaseFur != 0) {
                                GUILayout.Space(4);
                                SmallGroup("FUR SETTINGS");
                                xFur.materialProfiles[xFur.materialProfileIndex].furmatFurColorA = ColorField(new GUIContent("FUR COLOR A", "Main tint to be applied to the fur"), xFur.materialProfiles[xFur.materialProfileIndex].furmatFurColorA);
                                xFur.materialProfiles[xFur.materialProfileIndex].furmatFurColorB = ColorField(new GUIContent("FUR COLOR B", "Main tint to be applied to the fur"), xFur.materialProfiles[xFur.materialProfileIndex].furmatFurColorB);
                                xFur.materialProfiles[xFur.materialProfileIndex].furmatFurRim = ColorField(new GUIContent("FUR RIM COLOR", "Main tint to be applied to the fur's rim"), xFur.materialProfiles[xFur.materialProfileIndex].furmatFurRim);

                                xFur.materialProfiles[xFur.materialProfileIndex].furmatFurSpecular = ColorField(new GUIContent("SPECULAR COLOR", "Specular color to be applied to the fur"), xFur.materialProfiles[xFur.materialProfileIndex].furmatFurSpecular);

                                xFur.materialProfiles[xFur.materialProfileIndex].furmatFurColorMap = ObjectField<Texture2D>(new GUIContent("FUR COLOR MAP", "Texture to be applied to the fur"), xFur.materialProfiles[xFur.materialProfileIndex].furmatFurColorMap, false);
                                xFur.materialProfiles[xFur.materialProfileIndex].furmatData0 = ObjectField<Texture2D>(new GUIContent("FUR DATA 0", "Main fur Data map (fur mask, length, thickness and occlusion)"), xFur.materialProfiles[xFur.materialProfileIndex].furmatData0, false);
                                xFur.materialProfiles[xFur.materialProfileIndex].furmatData1 = ObjectField<Texture2D>(new GUIContent("FUR DATA 1", "Secondary fur Data map (grooming and stiffness)"), xFur.materialProfiles[xFur.materialProfileIndex].furmatData1, false);

                                if (xFur.materialProfiles[xFur.materialProfileIndex].furmatReadFurNoise > 0)
                                    xFur.materialProfiles[xFur.materialProfileIndex].furmatFurNoiseMap = ObjectField<Texture2D>(new GUIContent("FUR NOISE GEN", "Multi-layer noise map used as reference to generate the fur"), xFur.materialProfiles[xFur.materialProfileIndex].furmatFurNoiseMap, false);
                                GUILayout.Space(4);

                                xFur.materialProfiles[xFur.materialProfileIndex].furmatFurOcclusion = SliderField( new GUIContent("FUR OCCLUSION", "Shadowing and Occlusion to be applied to the fur"), xFur.materialProfiles[xFur.materialProfileIndex].furmatFurOcclusion, 0.0f, 1.0f);
                                xFur.materialProfiles[xFur.materialProfileIndex].furmatFurSmoothness = SliderField(new GUIContent("FUR SMOOTHNESS", "Smoothness to be applied to the fur"), xFur.materialProfiles[xFur.materialProfileIndex].furmatFurSmoothness, 0.0f, 1.0f);
                                xFur.materialProfiles[xFur.materialProfileIndex].furmatFurLength = SliderField(new GUIContent("FUR LENGTH", "Length of the fur"), xFur.materialProfiles[xFur.materialProfileIndex].furmatFurLength, 0.0f, 4.0f);
                                xFur.materialProfiles[xFur.materialProfileIndex].furmatFurThickness = SliderField(new GUIContent("FUR THICKNESS", "Thickness of the fur"), xFur.materialProfiles[xFur.materialProfileIndex].furmatFurThickness, 0.0f, 1.0f);
                                xFur.materialProfiles[xFur.materialProfileIndex].furmatFurRimPower = SliderField(new GUIContent("FUR RIM POWER", "The power of the rim lighting effect"), xFur.materialProfiles[xFur.materialProfileIndex].furmatFurRimPower, 0.0f, 1.0f);
                                GUILayout.Space(4);

                                xFur.materialProfiles[xFur.materialProfileIndex].furmatFurUV1 = FloatField(new GUIContent("FUR UV 0 SCALE", "Scale of the first fur specific UV channel"), xFur.materialProfiles[xFur.materialProfileIndex].furmatFurUV1);
                                xFur.materialProfiles[xFur.materialProfileIndex].furmatFurUV2 = FloatField(new GUIContent("FUR UV 1 SCALE", "Scale of the second fur specific UV channel"), xFur.materialProfiles[xFur.materialProfileIndex].furmatFurUV2);

                                Vector3 furDir = xFur.materialProfiles[xFur.materialProfileIndex].furmatDirection;
                                int groomAlgorithm = (int)xFur.materialProfiles[xFur.materialProfileIndex].furmatDirection.w;

                                groomAlgorithm = PopupField(new GUIContent("GROOMING ALGORITHM", "The grooming algorithm to use when adding fur direction to this model"), groomAlgorithm, new GUIContent[] { new GUIContent("Original", "The original grooming algorithm adds a bit of length to the fur as you groom allowing more creativity. Please use small fur direction values for best results"), new GUIContent("Accurate", "The new algorithm for grooming is more accurate, bending the fur without adding any length. It allows for a more controlled, predictable grooming. Please make sure to use high fur direction values for best results") });
                                furDir = Vector3Field(new GUIContent("FUR DIRECTION"), furDir);

                                xFur.materialProfiles[xFur.materialProfileIndex].furmatDirection = new Vector4(furDir.x, furDir.y, furDir.z, groomAlgorithm);



                                GUILayout.Space(8);
                                EndSmallGroup();

                                GUILayout.Space(8);
                            }

                        }
                        else {
                            GUILayout.Space(6);

                            GUILayout.BeginHorizontal(); GUILayout.Space(12);
                            EditorGUILayout.HelpBox("Per Instance parameters are not supported on models with more than 1 fur based material on Unity versions older than Unity 2018.1", MessageType.Warning);
                            GUILayout.Space(12); GUILayout.EndHorizontal();

                            GUILayout.Space(6);
                        }

                    }
                    else {
                        GUILayout.BeginHorizontal(); GUILayout.Space(12);
                        EditorGUILayout.HelpBox("This material is not a fur enabled material, no settings will be available for it.", MessageType.Warning);
                        GUILayout.Space(12); GUILayout.EndHorizontal();
                    }
                    GUILayout.Space(16);
                }
                EndCenteredGroup();


                if (BeginCenteredGroup(xFur.coatingModule != null ? "XFUR - " + xFur.coatingModule.ModuleName : "XFur - Coating Module (ERROR)", ref xFur.folds[1])) {
                    GUILayout.Space(16);
                    xFur.coatingModule.Module_UI(xFur);
                    GUILayout.Space(16);
                }
                EndCenteredGroup();


                if (BeginCenteredGroup(xFur.lodModule != null ? "XFUR - " + xFur.lodModule.ModuleName : "XFur - Lod Module (ERROR)", ref xFur.folds[2])) {
                    GUILayout.Space(16);
                    xFur.lodModule.Module_UI(xFur);
                    GUILayout.Space(16);
                }
                EndCenteredGroup();


                if (BeginCenteredGroup(xFur.physicsModule != null ? "XFUR - " + xFur.physicsModule.ModuleName : "XFur - Physics Module (ERROR)", ref xFur.folds[3])) {
                    GUILayout.Space(16);
                    xFur.physicsModule.Module_UI(xFur);
                    GUILayout.Space(16);
                }
                EndCenteredGroup();


                if (BeginCenteredGroup(xFur.fxModule != null ? "XFUR - " + xFur.fxModule.ModuleName : "XFur - FX Module (ERROR)", ref xFur.folds[4])) {
                    GUILayout.Space(16);
                    xFur.fxModule.Module_UI(xFur);
                    GUILayout.Space(16);
                }
                EndCenteredGroup();

                if (BeginCenteredGroup("HELP & SUPPORT", ref xFur.folds[6])) {

                    GUILayout.Space(16);
                    CenteredLabel("SUPPORT AND ASSISTANCE");
                    GUILayout.Space(10);

                    EditorGUILayout.HelpBox("Please make sure to include the following information with your request :\n - Invoice number\n - Screenshots of the XFur_System component and its settings\n - Steps to reproduce the issue.\n\nOur support service usually takes 2-4 business days to reply, so please be patient. We always reply to all emails.", MessageType.Info);

                    GUILayout.Space(8);
                    GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();
                    GUILayout.Label("For support, contact us at : support@irreverent-software.com", pidiSkin2.label);
                    GUILayout.FlexibleSpace(); GUILayout.EndHorizontal();

                    GUILayout.Space(8);

                    GUILayout.Space(16);
                    CenteredLabel("ONLINE TUTORIALS");
                    GUILayout.Space(10);
                    if (CenteredButton("QUICK START GUIDE", 200)) {
                        Help.BrowseURL("https://pidiwiki.irreverent-software.com/wiki/doku.php?id=xfur_studio_legacy#quick_start_guide");
                    }
                    if (CenteredButton("PREPARING YOUR 3D MODELS", 200)) {
                        Help.BrowseURL("https://pidiwiki.irreverent-software.com/wiki/doku.php?id=xfur_studio_legacy#preparing_your_3d_models");
                    }
                    if (CenteredButton("XFUR PAINTER", 200)) {
                        Help.BrowseURL("https://pidiwiki.irreverent-software.com/wiki/doku.php?id=xfur_studio_legacy#xfur_painter");
                    }
                    if (CenteredButton("XFUR SYSTEM COMPONENTS", 200)) {
                        Help.BrowseURL("https://pidiwiki.irreverent-software.com/wiki/doku.php?id=xfur_studio_legacy#xfur_studio_-_components");
                    }
                    if (CenteredButton("XFUR SYSTEM MODULES", 200)) {
                        Help.BrowseURL("https://pidiwiki.irreverent-software.com/wiki/doku.php?id=xfur_studio_legacy#xfur_studio_-_system_modules");
                    }

                    if (CenteredButton("XFUR STUDIO API", 200)) {
                        Help.BrowseURL("https://pidiwiki.irreverent-software.com/wiki/doku.php?id=xfur_studio_legacy#xfur_studio_api");
                    }

                    GUILayout.Space(24);
                    CenteredLabel("ABOUT PIDI : XFUR STUDIO™");
                    GUILayout.Space(12);

                    EditorGUILayout.HelpBox("PIDI : XFur Studio™ has been integrated in dozens of projects by hundreds of users.\nYour use and support to this tool is what keeps it growing, evolving and adapting to better suit your needs and keep providing you with the best quality fur for Unity.\n\nIf this tool has been useful for your project, please consider taking a minute to rate and review it, to help us to continue its development for a long time.", MessageType.Info);

                    GUILayout.Space(8);
                    if (CenteredButton("REVIEW XFUR STUDIO™", 200)) {
                        Help.BrowseURL("https://assetstore.unity.com/packages/vfx/shaders/pidi-xfur-studio-113361");
                    }
                    GUILayout.Space(8);
                    if (CenteredButton("ABOUT THIS VERSION", 200)) {
                        Help.BrowseURL("https://assetstore.unity.com/packages/vfx/shaders/pidi-xfur-studio-113361");
                    }
                    GUILayout.Space(8);
                }
                EndCenteredGroup();

            }

            GUILayout.Space(16);

            var tempStyle = new GUIStyle();
            tempStyle.normal.textColor = new Color(0.75f, 0.75f, 0.75f);
            tempStyle.fontSize = 9;
            tempStyle.fontStyle = FontStyle.Italic;
            GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();
            GUILayout.Label("Copyright© 2018-2019, Jorge Pinal N.", tempStyle);
            GUILayout.FlexibleSpace(); GUILayout.EndHorizontal();

            GUILayout.Space(16);
            GUILayout.EndVertical(); GUILayout.Space(8); GUILayout.EndHorizontal();


            GUI.skin = tSkin;
        }


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
        /// Draws the asset's logo and its current version
        /// </summary>
        public void AssetLogoAndVersion() {

            GUILayout.BeginVertical(logo, pidiSkin2 ? pidiSkin2.customStyles[1] : null);
            GUILayout.Space(45);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(xFur.Version, pidiSkin2.customStyles[2]);
            GUILayout.Space(6);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
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



    }

}