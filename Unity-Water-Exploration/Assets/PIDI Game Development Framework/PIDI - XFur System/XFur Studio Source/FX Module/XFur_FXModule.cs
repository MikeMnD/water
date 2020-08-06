/*
XFur Studio™ - XFur Generic Module
Copyright© 2018, Jorge Pinal Negrete. All Rights Reserved
*/

using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AnimatedValues;
#endif

namespace XFurStudio{
    [System.Serializable]
    public class XFur_FXModule:XFur_SystemModule {           
        
        [SerializeField]protected FurFXConfiguration snowSettings = new FurFXConfiguration( 0, true, Color.white, new Color( 0.3f,0.3f,0.3f,0.15f ), 10, true, true, null, 1 );
        [SerializeField]protected FurFXConfiguration bloodSettings = new FurFXConfiguration( 0, true, new Color(0.5f,0,0), new Color(0.15f,0,0,0f), 30, false, true, null, 1, 0.8f );
        [SerializeField]protected FurFXConfiguration waterSettings = new FurFXConfiguration( 0, true, new Color(0.5f,0.5f,0.5f), new Color( 0.3f,0.3f,0.3f,0.85f), 10, true, true, null, 1 );
        [SerializeField]protected FurFXConfiguration customFX0Settings;

        [SerializeField]protected bool forceUpdateNormals;
        [SerializeField] protected bool autoFixMeshTransform;
        [SerializeField]protected int maxIterations = 4;

        public bool affectedByLOD = true;

        private float timerNormals;
        private float timerFX;
        private float prevTime;
        private float secondCounter;
        private int currentIteration;

        private Texture2D fxTexture0 = null;
        private Mesh mesh;

        private int[] fVert;
        private int currentVert;
        private Color[] cols;
        private Vector3[] norms;
        private int skipFrame;
        [SerializeField]protected int vertexBuffer = 512;
        public float localWindStrength;
        private bool fxReady;
        

        public override void Module_Start( XFur_System owner ){
            systemOwner = owner;
            if ( !fxTexture0 ){
                fxTexture0 = systemOwner.XFur_CreateVertexData( "FUR_VERTICES" ); 
                fxTexture0.filterMode = FilterMode.Point;
                fxTexture0.wrapMode = TextureWrapMode.Clamp;
                cols = new Color[fxTexture0.width*fxTexture0.height];
                mesh = new Mesh();

                if ( systemOwner.GetComponent<SkinnedMeshRenderer>() ){
                    systemOwner.GetComponent<SkinnedMeshRenderer>().BakeMesh(mesh);
                }

                norms = systemOwner.GetComponent<SkinnedMeshRenderer>()?mesh.normals:systemOwner.Mesh.normals;

                if ( systemOwner.manualMeshIndex < 0)
                    fVert = systemOwner.database.meshData[systemOwner.database.XFur_ContainsMeshData(systemOwner.OriginalMesh)].furVertices;
                else
                    fVert = systemOwner.database.meshData[systemOwner.database.XFur_ContainsMeshData(systemOwner.manualMeshIndex)].furVertices;

            }

            timerFX = Time.timeSinceLevelLoad;
            systemOwner.XFur_UpdateFurMaterials();
        }

        public override void Module_Execute(){
            var vBuffer = vertexBuffer;
            if ( State == XFurModuleState.Enabled  && currentIteration < maxIterations && prevTime+(1.0f/maxIterations*currentIteration)+Time.deltaTime*Random.Range(0,16)<Time.timeSinceLevelLoad ){
                if ( currentVert == 0 ){
                    //cols = fxTexture0.GetPixels();

                    if ( ( affectedByLOD && ( systemOwner.lodModule.XFur_LODLevel > 0 || systemOwner.lodModule.State != XFurModuleState.Enabled ) ) && systemOwner.GetComponent<SkinnedMeshRenderer>() && forceUpdateNormals ){
                        systemOwner.GetComponent<SkinnedMeshRenderer>().BakeMesh(mesh);
                        norms = systemOwner.GetComponent<SkinnedMeshRenderer>()? mesh.normals:systemOwner.Mesh.normals;
                    }

                    if ( affectedByLOD && systemOwner.lodModule.State == XFurModuleState.Enabled ){
                        if ( systemOwner.lodModule.XFur_LODLevel == 0 ){
                            vBuffer = vertexBuffer/4;
                        }
                    }
                    
                }
            
                
                    if ( State == XFurModuleState.Enabled ){
                       
                        var n = Vector3.zero;
                        var dot = 0.0f;
                        snowSettings.direction = systemOwner.transform.root.InverseTransformDirection( XFur_WeatherSystem.snowDirection );
                        waterSettings.direction = systemOwner.transform.root.InverseTransformDirection( XFur_WeatherSystem.rainDirection );
                        var snowDir = snowSettings.direction;
                        var rainDir = waterSettings.direction;

                        var fTime = (timerFX-prevTime);

                       for ( int c = currentVert; c < currentVert+vBuffer; c++ ){
                            if ( c < fVert.Length ){
                                n = autoFixMeshTransform?systemOwner.transform.TransformDirection(norms[fVert[c]]):norms[fVert[c]];
                                
                                if ( bloodSettings.fxMode == 0 ){
                                    cols[c].r = 0;
                                }

                                cols[c].r -= cols[c].r>0?bloodSettings.vanishOverTime?fTime/bloodSettings.vanishingTime:0:0;
                                dot = (snowDir.x * n.x) + (snowDir.y * n.y) + (snowDir.z * n.z);
                                
                                if ( snowSettings.fxMode != 0 && snowSettings.fxMode != 3 ){
                                    cols[c].g += cols[c].g<1? dot < -snowSettings.normalFalloff?fTime*XFur_WeatherSystem.snowStrength:0:0;
                                    
                                }
                                else if ( snowSettings.fxMode == 0 ){
                                    cols[c].g = 0;
                                }

                                cols[c].g -= cols[c].g>0?snowSettings.vanishOverTime?fTime/snowSettings.vanishingTime:0:0;
                                
                                dot = (rainDir.x * n.x) + (rainDir.y * n.y) + (rainDir.z * n.z);

                                if ( waterSettings.fxMode != 0 && waterSettings.fxMode != 3 ){
                                    cols[c].b += cols[c].b<1? dot < -waterSettings.normalFalloff?fTime*XFur_WeatherSystem.rainStrength*0.5f:0:0;
                                    
                                }
                                else if ( waterSettings.fxMode == 0 ){
                                    cols[c].b = 0;
                                }

                                cols[c].b -= cols[c].b>0?waterSettings.vanishOverTime?fTime/waterSettings.vanishingTime:0:0;
                                
                            }

                        }
                        currentVert += vBuffer;

                        if ( currentVert >= fVert.Length ){
                            fxTexture0.SetPixels(cols);
                            fxTexture0.Apply();
                            fxReady = true;
                            prevTime = timerFX;
                            currentIteration++;
                            

                            timerFX = Time.timeSinceLevelLoad;
                            currentVert = 0;
                        }
                    }                 
                
            }

            if ( Time.timeSinceLevelLoad > secondCounter ){
                secondCounter = Time.timeSinceLevelLoad+1;
                currentIteration=0;
            }
        }


        public override void Module_UpdateFurData( ref MaterialPropertyBlock m ){
                m.SetFloat("_LocalWindStrength", State==XFurModuleState.Enabled?localWindStrength:0);
                m.SetFloat( "_FXTexSize", State==XFurModuleState.Enabled&&fxTexture0!=null?fxTexture0.width:64 );
                m.SetTexture( "_FurFXMap", State==XFurModuleState.Enabled&&fxReady?fxTexture0:Texture2D.blackTexture );
                
                m.SetFloat("_FX0Penetration", State==XFurModuleState.Enabled?bloodSettings.furPenetration:0 );
                m.SetColor("_FXColor0", State==XFurModuleState.Enabled?bloodSettings.fxColor:Color.black );
                m.SetColor("_FXSpecSmooth0", State==XFurModuleState.Enabled?bloodSettings.fxSpecSmooth:Color.black );

                m.SetFloat("_FX1Penetration", State==XFurModuleState.Enabled?snowSettings.furPenetration:0 );
                m.SetColor("_FXColor1", State==XFurModuleState.Enabled?snowSettings.fxColor:Color.black );
                m.SetColor("_FXSpecSmooth1", State==XFurModuleState.Enabled?snowSettings.fxSpecSmooth:Color.black );

                m.SetFloat("_FX2Penetration", State==XFurModuleState.Enabled?waterSettings.furPenetration:0 );
                m.SetColor("_FXColor2", State==XFurModuleState.Enabled?waterSettings.fxColor:Color.black );
                m.SetColor("_FXSpecSmooth2", State==XFurModuleState.Enabled?waterSettings.fxSpecSmooth:Color.black );
        
        }

        public override void Module_InstancedFurData( Material mat ){
                mat.SetFloat("_LocalWindStrength", State==XFurModuleState.Enabled?localWindStrength:0);
                
                mat.SetFloat( "_FXTexSize", State==XFurModuleState.Enabled&&fxTexture0!=null?fxTexture0.width:64 );
                mat.SetTexture( "_FurFXMap", State==XFurModuleState.Enabled&&fxTexture0!=null?fxTexture0:Texture2D.blackTexture );
                
                mat.SetFloat("_FX0Penetration", State==XFurModuleState.Enabled?bloodSettings.furPenetration:0 );
                mat.SetColor("_FXColor0", State==XFurModuleState.Enabled?bloodSettings.fxColor:Color.black );
                mat.SetColor("_FXSpecSmooth0", State==XFurModuleState.Enabled?bloodSettings.fxSpecSmooth:Color.black );            
                
                mat.SetFloat("_FX1Penetration", State==XFurModuleState.Enabled?snowSettings.furPenetration:0 );
                mat.SetColor("_FXColor1", State==XFurModuleState.Enabled?snowSettings.fxColor:Color.black );
                mat.SetColor("_FXSpecSmooth1", State==XFurModuleState.Enabled?snowSettings.fxSpecSmooth:Color.black );
        }

        public override void Module_End(){
            if ( Application.isPlaying ){
                
            }
        }



        #if UNITY_EDITOR

        public override void Module_StartUI( GUISkin editorSkin ){
            base.Module_StartUI( editorSkin );
            moduleName = "FX MODULE v1.9";
        }

        public override void Module_UI(XFur_System owner){

            //Undo.RecordObject(this,moduleName+"_"+GetInstanceID());
            base.Module_UI(owner);

            if ( Enabled ){
                GUILayout.Space(16);

                affectedByLOD = EnableDisableToggle(new GUIContent("LOD INFLUENCED MODE", "If enabled, this module's functionality will be affected by the LOD module. At further distances, the functionality of this module will be adjusted for better performance"), affectedByLOD);
                GUILayout.Space(4);
                autoFixMeshTransform = EnableDisableToggle(new GUIContent("TRANSFORM AUTO-FIX", "Automatically Fix transform issues on your mesh (rotation is not 0,0,0. Fixes most cases of snow accumulating on wrong places/directions but it has a performance cost. It is recommended to fix the rotation on your modelling app and just use this option if you cannot modify the original mesh"), autoFixMeshTransform);
                forceUpdateNormals =  EnableDisableToggle( new GUIContent("DYNAMIC NORMALS UPDATE","If enabled, the normals of the mesh are updated as it moves (on animated characters) to calculate the snow/rain coverage in a much more accurate way. Impacts performance"), forceUpdateNormals );
                GUILayout.Space(4);
                localWindStrength = SliderField( new GUIContent("LOCAL WIND STRENGTH", "A local multiplier for the global wind strength"), localWindStrength, 0.0f, 64.0f );
                maxIterations = IntSliderField( new GUIContent("MAX. ITERATIONS","Maximum amount of updates per second that the module makes to the effects before sending them to the mesh. Impacts performance"), maxIterations, 1, 10 );
                vertexBuffer = IntSliderField( new GUIContent("VERTICES BUFFER","Maximum amount of vertex operations per frame that the module will perform. Lower values give better performance but reduce the smoothness of the effects' fading and melting"), vertexBuffer, 32, 2048 );

                GUILayout.Space(16);

                SmallGroup("BLOOD FX SETTINGS");
                
                if ( bloodSettings.fxMode != 0 ){
                    bloodSettings.fxMode = 3;
                }

                bloodSettings.fxMode = PopupField( new GUIContent("BLOOD MODE", "The mode in which this effect will be calculated and applied"), bloodSettings.fxMode, new GUIContent[]{ new GUIContent("Disabled"), new GUIContent("Global & On Demand", "Blood is added through both a global direction for the whole scene and upon demand by specific objects/function calls"), new GUIContent("Global Direction","The blood comes from a global direction and is applied to every fur system that has it enabled"), new GUIContent("On Demand","The blood is only applied by specific objects upon contact / function call") } );
                
                if ( bloodSettings.fxMode> 0 ){
                    GUILayout.Space(8);
                    
                    bloodSettings.fxColor = ColorField( new GUIContent("FX COLOR"), bloodSettings.fxColor );
                    bloodSettings.normalFalloff = SliderField( new GUIContent("EFFECT FALLOFF","How will the blood effect falloff on the slopes of the model?"), bloodSettings.normalFalloff, 0.15f, 0.9f );
                    bloodSettings.vanishOverTime = EnableDisableToggle( new GUIContent("FADE OVER TIME", "Should the blood dry and fade over a certain period of time?"), bloodSettings.vanishOverTime );
                    
                    GUILayout.Space(8);
                    if ( bloodSettings.vanishOverTime )
                        bloodSettings.vanishingTime = FloatField( new GUIContent("FADING TIME (SECS)", "The amount of time it takes for the blood to dry and fade" ), bloodSettings.vanishingTime );
                    
                    bloodSettings.furPenetration = SliderField( new GUIContent( "FX FUR PENETRATION","How much the effect will penetrate the fur, from tips to roots"), bloodSettings.furPenetration, 0.0f, 1.0f );
                    bloodSettings.fxSpecSmooth = ColorField( new GUIContent( "FX SPEC/SMOOTHNESS","The specular RGB and smoothness modifier of this effect"), bloodSettings.fxSpecSmooth );
                }
                GUILayout.Space(8);
                EndSmallGroup();

                GUILayout.Space(16);

                SmallGroup("SNOW FX SETTINGS");

                snowSettings.fxMode = PopupField( new GUIContent("SNOW MODE", "The mode in which this effect will be calculated and applied"), snowSettings.fxMode, new GUIContent[]{ new GUIContent("Disabled"), new GUIContent("Global & On Demand", "Snow is added through both a global direction for the whole scene and upon demand by specific objects/function calls"), new GUIContent("Global Direction","The snow comes from a global direction and is applied to every fur system that has it enabled"), new GUIContent("On Demand","The snow is only applied by specific objects upon contact / function call") } );
                
                if ( snowSettings.fxMode > 0 ){
                    GUILayout.Space(8);
                    snowSettings.fxColor = ColorField( new GUIContent("FX COLOR"), snowSettings.fxColor );
                    snowSettings.normalFalloff = SliderField( new GUIContent("EFFECT FALLOFF","How will the snow effect falloff on the slopes of the model?"), snowSettings.normalFalloff, 0.15f, 0.9f );
                    snowSettings.vanishOverTime = EnableDisableToggle( new GUIContent("FADE OVER TIME", "Should the snow melt over a certain period of time?"), snowSettings.vanishOverTime );
                    GUILayout.Space(8);

                    if ( snowSettings.vanishOverTime )    
                        snowSettings.vanishingTime = FloatField( new GUIContent("MELTING TIME (SECS)", "The amount of time it takes for the snow to melt" ), snowSettings.vanishingTime );
                        snowSettings.furPenetration = SliderField( new GUIContent( "FX FUR PENETRATION","How much the effect will penetrate the fur, from tips to roots"), snowSettings.furPenetration, 0.0f, 1.0f );
                        snowSettings.fxSpecSmooth = ColorField( new GUIContent( "FX SPEC/SMOOTHNESS","The specular RGB and smoothness modifier of this effect"), snowSettings.fxSpecSmooth );
                        GUILayout.Space(8);
                    }

                EndSmallGroup();
                
                GUILayout.Space(12);

                SmallGroup("WATER FX SETTINGS");
                
                waterSettings.fxMode = PopupField( new GUIContent("WATER MODE", "The mode in which this effect will be calculated and applied"), waterSettings.fxMode, new GUIContent[]{ new GUIContent("Disabled"), new GUIContent("Global & On Demand", "Snow is added through both a global direction for the whole scene and upon demand by specific objects/function calls"), new GUIContent("Global Direction","The water comes from a global direction and is applied to every fur system that has it enabled"), new GUIContent("On Demand","The water is only applied by specific objects upon contact / function call") } );
                
                if ( waterSettings.fxMode> 0 ){
                    GUILayout.Space(8);
                    waterSettings.fxColor = ColorField( new GUIContent("FX Color"), waterSettings.fxColor );
                    waterSettings.normalFalloff = SliderField( new GUIContent("EFFECT FALLOFF","How will the water effect falloff on the slopes of the model?"), waterSettings.normalFalloff, 0.15f, 0.9f );
                    waterSettings.vanishOverTime = EnableDisableToggle( new GUIContent("FADE OVER TIME", "Should the water dry off over a certain period of time?"), waterSettings.vanishOverTime );

                    GUILayout.Space(8);

                    if ( waterSettings.vanishOverTime )    
                        waterSettings.vanishingTime = FloatField( new GUIContent("MELTING TIME (SECS)", "The amount of time it takes for the snow to melt" ), waterSettings.vanishingTime );
                        waterSettings.furPenetration = SliderField( new GUIContent( "FX FUR PENETRATION","How much the effect will penetrate the fur, from tips to roots"), waterSettings.furPenetration, 0.0f, 1.0f );
                        waterSettings.fxSpecSmooth = ColorField( new GUIContent( "FX SPEC/SMOOTHNESS","The specular RGB and smoothness modifier of this effect"), waterSettings.fxSpecSmooth );
                        GUILayout.Space(8);
                }
                GUILayout.Space(8);
                EndSmallGroup();

                GUILayout.Space(12);

            }

            GUILayout.Space(12);
        }

        #endif




        /// <summary>
        /// Apply an effect to this mesh by directly adding a certain amount of coverage.
        /// </summary>
        /// <param name="effectIndex" ></param>The index of the effect to apply : 0=blood, 1=snow, 2=water
        /// <param name="vertexIndices"></param>The indices of the vertices that will receive the effect
        /// <param name="intensity"></param>The intensity (as a float between 0 and 1) of the effect
        public void ApplyEffect( int effectIndex, int[] vertexIndices, float intensity = 1 ){

            switch( effectIndex ){
                case 0:
                if ( bloodSettings.fxMode == 0 || bloodSettings.fxMode == 1 ){
                    return;
                }
                for ( int i = 0; i < vertexIndices.Length; i++ ){
                    cols[vertexIndices[i]].r = intensity;
                }
                break;

                case 1:
                if ( snowSettings.fxMode == 0 || bloodSettings.fxMode == 1 ){
                    return;
                }
                for ( int i = 0; i < vertexIndices.Length; i++ ){
                    cols[vertexIndices[i]].g = intensity;
                }
                break;

                case 2:
                if ( waterSettings.fxMode == 0 || bloodSettings.fxMode == 1 ){
                    return;
                }
                for ( int i = 0; i < vertexIndices.Length; i++ ){
                    cols[vertexIndices[i]].b = intensity;
                }
                break;

                case 3:
                if ( customFX0Settings.fxMode == 0 || bloodSettings.fxMode == 1 ){
                    return;
                }
                for ( int i = 0; i < vertexIndices.Length; i++ ){
                    cols[vertexIndices[i]].a = intensity;
                }
                break;
            }

        }
        
    }

    [System.Serializable]
        public struct FurFXConfiguration{
            public int fxMode;
            public bool vanishOverTime;
            public bool windAffected;
            public bool clumpFur;
            public Texture2D globalTexture;
            public Texture2D furNormals;
            public Vector3 direction;
            public float fxIntensity;
            public float vanishingTime;
            public Color fxSpecSmooth;
            public float furPenetration;
            public Color fxColor;
            public float normalFalloff;


            public FurFXConfiguration( int mode, bool vanishMode, Color mainColor, Color specSmooth, float vanishTime = 10, bool wind = true, bool clumps = true, Texture2D tex = null, float intensity = 1, float furP = 0.6f ){
                fxMode = mode;
                vanishOverTime = vanishMode;
                vanishingTime = vanishTime;
                windAffected = wind;
                clumpFur = clumps;
                globalTexture = tex;
                direction = Vector3.down;
                fxIntensity = intensity;
                fxSpecSmooth = specSmooth;
                furPenetration = furP;
                furNormals = null;
                fxColor = mainColor;
                normalFalloff = 0.45f;
            }

        }
    
}