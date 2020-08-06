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
    public class XFur_LODModule:XFur_SystemModule {
        
        public enum XFur_LODMode{ Quality,  Performance };
        
        [SerializeField]protected XFur_LODMode lodMode;
        private float timer;

        public float maxDistance = 32.0f;

        public int XFur_LODLevel;

        private int lodStep = 24;

        private bool loadedR;

        private int[] furRenders = new int[32];

        public override void Module_Start(XFur_System owner){
            systemOwner = owner;


            for ( int i = 0; i < owner.materialProfiles.Length; i++) {
                furRenders[i] = (int)owner.materialProfiles[i].fullShadowLayers;
            }
        }

        
        public override void Module_End(){

        }

        #if UNITY_EDITOR

        public override void Module_StartUI( GUISkin editorSkin ){
            base.Module_StartUI( editorSkin );
            moduleName = "LOD MODULE 1.9";
            
        }

        public override void Module_UI(XFur_System owner){

            base.Module_UI(owner);

            if ( State==XFurModuleState.Enabled ){
                lodMode = (XFur_LODMode)UpperCaseEnumField( new GUIContent("LOD MODE", "The way the lod of this object will be managed and calculated. Quality switches to lower sample counts at a further distance (around 70% of the max distance) while Performance mode uses lower sample counts unless in a heavy close up (around 10-20% of the max. distance)"), lodMode );
                maxDistance = SliderField( new GUIContent("MAX. DISTANCE","As the character approaches the max distance it will start reducing the amount of samples of the fur and to use simpler algorithms for physics, effects, etc."), maxDistance, 0.01f, 250.0f );
            }
        }

        #endif

        public override void Module_Execute(){
                       
            lodStep = 24;
            if ( State != XFurModuleState.Enabled ){
                XFur_LODLevel = 99;
                
                if ( !loadedR )
                foreach( XFur_MaterialProperties fMat in systemOwner.materialProfiles ){
                    foreach( Renderer r in fMat.subRenders ){
                        if ( r )
                            r.enabled = true;
                    }
                }
                loadedR = true;
                return;
            }

            loadedR = false;

            if ( Time.timeSinceLevelLoad < timer ){
                return;
            }
            
            timer = Time.timeSinceLevelLoad+Random.Range(0.25f,0.6f);


            var size = Vector3.Distance(Camera.main.transform.position, systemOwner.transform.position )*100/maxDistance;

            size = 100-size;

            if ( !systemOwner.rend.isVisible ){
                size = 0;
            }

            if ( lodMode==XFur_LODMode.Quality ){
                var levels = systemOwner.database.highQualityShaders.Length;
                var lodLevel = 80.0f/levels;
                var index = -1;

                
                for ( int i = 0; i < levels; i++ ){
                    if ( size < lodLevel*(i+1) ){
                        index = i;
                        break;
                    }
                }

               

                if ( index == -1 ){
                    foreach( XFur_MaterialProperties fMat in systemOwner.materialProfiles ){
                        fMat.furmatSamples = levels-1;
                        foreach( Renderer r in fMat.subRenders ){
                            if ( r )
                                r.enabled = true;
                        }
                        lodStep = 24;
                        index = levels;
                        if (XFur_LODLevel != index)
                            systemOwner.XFur_SwitchMaterialSamples(fMat);
                    }
                }
                else{
                    foreach( XFur_MaterialProperties fMat in systemOwner.materialProfiles ){
                        fMat.furmatSamples = index;

                        if ( size < 8 ){
                            fMat.fullShadowLayers = Mathf.Clamp( furRenders[fMat.furmatIndex] * 0.1f, 6, 32 );
                            lodStep = 3;
                        }
                        else if ( size < 16 ){
                            fMat.fullShadowLayers = Mathf.Clamp(furRenders[fMat.furmatIndex] * 0.25f, 6, 32);
                            lodStep = 6;
                        }
                        else if ( size < 25 ){
                            fMat.fullShadowLayers = Mathf.Clamp(furRenders[fMat.furmatIndex] * 0.3f, 6, 32);
                            lodStep = 9;
                        }
                        else if ( size < 35 ){
                            fMat.fullShadowLayers = Mathf.Clamp(furRenders[fMat.furmatIndex] * 0.45f, 6, 32);
                            lodStep = 12;
                        }
                        else if ( size < 50 ){
                            fMat.fullShadowLayers = Mathf.Clamp(furRenders[fMat.furmatIndex] * 0.6f, 6, 32);
                            lodStep = 15;
                        }
                        else if ( size < 65 ){
                            fMat.fullShadowLayers = Mathf.Clamp(furRenders[fMat.furmatIndex] * 0.75f, 6, 32);
                            lodStep = 18;
                        }
                        else if ( size < 75 ){
                            fMat.fullShadowLayers = Mathf.Clamp(furRenders[fMat.furmatIndex] * 0.85f, 6, 32);
                            lodStep = 21;
                        }
                        else{
                            fMat.fullShadowLayers = furRenders[fMat.furmatIndex];
                            lodStep = 24;
                        }
                        
                        if ( XFur_LODLevel != index )
                            systemOwner.XFur_SwitchMaterialSamples(fMat);
                    }
                }


                if ( XFur_LODLevel != index ){
                    XFur_LODLevel = index;
                    //systemOwner.XFur_UpdateFurMaterials();
                }

            }


            if ( lodMode==XFur_LODMode.Performance ){
                var levels = systemOwner.database.highQualityShaders.Length;
                var lodLevel = 120.0f/levels;
                var index = -1;

                
                for ( int i = 0; i < levels; i++ ){
                    if ( size < lodLevel*(i+1)+lodLevel*0.2f ){
                        index = i;
                        break;
                    }
                }


                if ( index == -1 ){
                    foreach( XFur_MaterialProperties fMat in systemOwner.materialProfiles ){
                        fMat.furmatSamples = levels-1;
                        index = levels;
                        foreach( Renderer r in fMat.subRenders ){
                            if ( r )
                                r.enabled = true;
                        }
                        lodStep = 24;
                        systemOwner.XFur_SwitchMaterialSamples(fMat);
                    }
                }
                else{
                    foreach( XFur_MaterialProperties fMat in systemOwner.materialProfiles ){
                        fMat.furmatSamples = index;
                        if ( size < 8 ){
                            for ( int i = 0; i < fMat.subRenders.Length; i++ ){
                                if ( fMat.subRenders[i] )
                                    fMat.subRenders[i].enabled = i < 1;
                            }
                            lodStep = 3;
                        }
                        else if ( size < 16 ){
                            for ( int i = 0; i < fMat.subRenders.Length; i++ ){
                                if ( fMat.subRenders[i] )
                                    fMat.subRenders[i].enabled = i < 2;
                            }
                            lodStep = 6;
                        }
                        else if ( size < 25 ){
                            for ( int i = 0; i < fMat.subRenders.Length; i++ ){
                                if ( fMat.subRenders[i] )
                                    fMat.subRenders[i].enabled = i < 3;
                            }
                            lodStep = 9;
                        }
                        else if ( size < 35 ){
                            for ( int i = 0; i < fMat.subRenders.Length; i++ ){
                                if ( fMat.subRenders[i] )
                                    fMat.subRenders[i].enabled = i < 4;
                            }
                            lodStep = 12;
                        }
                        else if ( size < 60 ){
                            for ( int i = 0; i < fMat.subRenders.Length; i++ ){
                                if ( fMat.subRenders[i] )
                                    fMat.subRenders[i].enabled = i < 5;
                            }
                            lodStep = 15;
                        }
                        else if ( size < 80 ){
                            for ( int i = 0; i < fMat.subRenders.Length; i++ ){
                                if ( fMat.subRenders[i] )
                                    fMat.subRenders[i].enabled = i < 6;
                            }
                            lodStep = 18;
                        }
                        else if ( size < 90 ){
                            for ( int i = 0; i < fMat.subRenders.Length; i++ ){
                                if ( fMat.subRenders[i] )
                                    fMat.subRenders[i].enabled = i < 7;
                            }
                            lodStep = 21;
                        }
                        else{
                            for ( int i = 0; i < fMat.subRenders.Length; i++ ){
                                if ( fMat.subRenders[i] )
                                    fMat.subRenders[i].enabled = true;
                            }
                            lodStep = 24;
                        }
                        systemOwner.XFur_SwitchMaterialSamples(fMat);
                    }
                }

                 if ( XFur_LODLevel != index ){
                    XFur_LODLevel = index;
                    //systemOwner.XFur_UpdateFurMaterials();
                }

            }

        }


        public override void Module_UpdateFurData(ref MaterialPropertyBlock m){
            //m.AddFloat("_FurStep", lodStep );
            m.SetFloat("_FurStep", lodStep );
        }

        

        private float GetScreenSizePercentage( Camera cam ){
            
                float pixelSize = ( systemOwner.rend.bounds.extents.magnitude*Mathf.Rad2Deg ) / ( Vector3.Distance( systemOwner.transform.position, cam.transform.position ) / 60 );
                float screenPercent = pixelSize/Screen.width*10;
                return screenPercent;

        }
        
    }



    
}