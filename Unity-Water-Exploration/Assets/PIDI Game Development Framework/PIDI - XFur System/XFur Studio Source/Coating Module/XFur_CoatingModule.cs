 /*
XFur Studio™ - XFur Generic Module
Copyright© 2018, Jorge Pinal Negrete. All Rights Reserved
*/

using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AnimatedValues;
#endif

namespace XFurStudio{
    [System.Serializable]
    public class XFur_CoatingModule:XFur_SystemModule {
        
        public XFur_CoatingSettings[] coatingSettings = new XFur_CoatingSettings[0];

        ///<summary>LEGACY, will disappear in XFur 2</summary>
        public bool profileVariation;

        /// <summary>If enabled, the module will select two profiles randomly and use them as limits for the variation of the parameters. Each fur parameter will take a value randomly selected between Profile A and profile B</summary>
        public bool randomizeAllParameters;
        


        public override void Module_Start( XFur_System owner ){
            
            systemOwner = owner;

            if ( !systemOwner || !systemOwner.database )
                return; 

            if ( coatingSettings.Length != systemOwner.materialProfiles.Length ){
                coatingSettings = new XFur_CoatingSettings[systemOwner.materialProfiles.Length];
                for ( int i = 0; i < coatingSettings.Length; i++ ){
                    coatingSettings[i] = new XFur_CoatingSettings();
                    systemOwner.materialProfiles[i].CopyTo( ref coatingSettings[i].originalP );
                }
            }

            for ( int i = 0; i < coatingSettings.Length; i++ ){
                if ( coatingSettings[i].originalP == null ){
                    coatingSettings[i].originalP = new XFur_MaterialProperties();
                }
                systemOwner.materialProfiles[i].CopyTo( ref coatingSettings[i].originalP );
                coatingSettings[i].originalP.originalMat = systemOwner.materialProfiles[i].originalMat;
            }            
            

            if ( Application.isPlaying && State == XFurModuleState.Enabled ){
                RandomizeFur();
            }
        
        }

        public override void Module_Execute(){

        }

        public override void Module_End(){

        }


        public void RandomizeFur(){
            for (int i = 0; i < coatingSettings.Length; i++) {
                if (systemOwner.materialProfiles[i].furmatType == 2 && coatingSettings[i].coatingProfiles.Length > 0) {
                    var rnd = Random.Range(0, coatingSettings[i].coatingProfiles.Length);
                    systemOwner.LoadXFurProfileAsset(coatingSettings[i].coatingProfiles[rnd], i);

                    if (randomizeAllParameters && coatingSettings[i].coatingProfiles.Length > 1) {
                        var a = coatingSettings[i].coatingProfiles[rnd];
                        var b = coatingSettings[i].coatingProfiles[Random.Range(0, coatingSettings[i].coatingProfiles.Length)];

                        var rndLerp = Random.Range(0f, 1.0f);

                        systemOwner.materialProfiles[i].furLength = Mathf.Lerp(a.profile.furLength, b.profile.furLength, rndLerp);
                        systemOwner.materialProfiles[i].furThickness = Mathf.Lerp(a.profile.furThickness, b.profile.furThickness, rndLerp);
                        systemOwner.materialProfiles[i].furGlossiness = Mathf.Lerp(a.profile.furGlossiness, b.profile.furGlossiness, rndLerp);
                        systemOwner.materialProfiles[i].furOcclusion = Mathf.Lerp(a.profile.furOcclusion, b.profile.furOcclusion, rndLerp);
                        systemOwner.materialProfiles[i].furAnisoOffset = Mathf.Lerp(a.profile.furAnisoOffset, b.profile.furAnisoOffset, rndLerp);
                        systemOwner.materialProfiles[i].furRimPower = Mathf.Lerp(a.profile.furRimPower, b.profile.furRimPower, rndLerp);

                        systemOwner.materialProfiles[i].furColorA = Color.Lerp(a.profile.furColorA, b.profile.furColorA, rndLerp);
                        systemOwner.materialProfiles[i].furColorB = Color.Lerp(a.profile.furColorB, b.profile.furColorB, rndLerp);
                        systemOwner.materialProfiles[i].furSpecColor = Color.Lerp(a.profile.furSpecColor, b.profile.furSpecColor, rndLerp);
                        systemOwner.materialProfiles[i].furRimColor = Color.Lerp(a.profile.furRimColor, b.profile.furRimColor, rndLerp);

                    }
                    if (profileVariation) {
                        systemOwner.materialProfiles[i].furColorA = Color.Lerp(coatingSettings[i].coatingProfiles[rnd].furColorA_Min, coatingSettings[i].coatingProfiles[rnd].furColorA_Max, Random.Range(0.0f, 1.1f));
                        systemOwner.materialProfiles[i].furColorB = Color.Lerp(coatingSettings[i].coatingProfiles[rnd].furColorB_Min, coatingSettings[i].coatingProfiles[rnd].furColorB_Max, Random.Range(0.0f, 1.1f));
                    }
                }
            }
        }

        #if UNITY_EDITOR

        public override void Module_StartUI( GUISkin editorSkin ){
            base.Module_StartUI( editorSkin );
            moduleName = "RANDOMIZATION MODULE 1.9";

        }


        public override void Module_UI(XFur_System owner){
            base.Module_UI(owner);
            GUILayout.Space(8);

            if ( State == XFurModuleState.Enabled ){

                for ( int i = 0; i < coatingSettings.Length; i++ ){

                    systemOwner.CopyFurProperties(systemOwner.materialProfiles[i], ref coatingSettings[i].originalP);

                    if (systemOwner.materialProfiles[i].furmatType == 2) {

                        SmallGroup(coatingSettings[i].originalP.originalMat.name);

                        GUILayout.Space(8);
                        randomizeAllParameters = EnableDisableToggle(new GUIContent("RANDOMIZED PARAMETERSN","Picks two randomly selected profiles from the profiles list and randomly lerps each fur parameter between them to produce more varied results"), randomizeAllParameters);
                        GUILayout.Space(8);

                        for (int c = 0; c < coatingSettings[i].coatingProfiles.Length; c++) {
                            GUILayout.BeginHorizontal();
                            coatingSettings[i].coatingProfiles[c] = ObjectField<XFur_CoatingProfile>(new GUIContent("FUR PROFILE " + c, "The fur profile to use as a random option for this module"), coatingSettings[i].coatingProfiles[c], false);

                            if (StandardButton("REMOVE", 100)) {
                                var l = new List<XFur_CoatingProfile>(coatingSettings[i].coatingProfiles);
                                l.RemoveAt(c);
                                coatingSettings[i].coatingProfiles = l.ToArray();
                            }
                            GUILayout.Space(8); GUILayout.EndHorizontal();
                        }

                        GUILayout.Space(8);
                        
                        if (CenteredButton("ADD NEW PROFILE", 200)) {
                            var l = new List<XFur_CoatingProfile>(coatingSettings[i].coatingProfiles);
                            l.Add(null);
                            coatingSettings[i].coatingProfiles = l.ToArray();
                        }


                        EndSmallGroup();

                        GUILayout.Space(12);

                    }
                }
            }
        }
        

        #endif



        
    }


    [System.Serializable]
    public class XFur_CoatingSettings{
        public XFur_CoatingProfile[] coatingProfiles = new XFur_CoatingProfile[0];
        public XFur_MaterialProperties originalP = new XFur_MaterialProperties();

        
    }
    
}