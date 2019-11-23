using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XFurStudio;

public class RuntimeXFurBuilder : MonoBehaviour {

    public Mesh referenceMesh;
    public Material[] referenceMats;
    public bool generateProceduralCopy;
    public XFur_DatabaseModule database;
    public XFur_CoatingProfile dynamicFurProfile;
    public GameObject prefab;

    public IEnumerator Start() {


        yield return new WaitForSeconds(3);
        CreateRuntime();
        

    }


    public void CreateRuntime() {
        var m = gameObject.AddComponent<MeshFilter>();
        var r = gameObject.AddComponent<MeshRenderer>();


        if (generateProceduralCopy) {
            m.sharedMesh = (Mesh)Instantiate(referenceMesh);
        }
        else {
            m.sharedMesh = referenceMesh;
        }

        r.sharedMaterials = referenceMats;

        var x = gameObject.AddComponent<XFur_System>();

        x.database = database;

        x.manualMeshIndex = 1;

        //Before modifying any of the fur based materials, the XFurStart function must be called. Otherwise, the materials array won't be initialized.
        x.XFur_Start();

        x.materialProfiles[0].furmatReadBaseFur = 1;
        x.materialProfiles[0].furmatTriplanar = 1;
        x.materialProfiles[0].furmatFurSmoothness = 0.05f;
        x.materialProfiles[0].furmatTriplanarScale = 0.35f;
        x.materialProfiles[0].furmatFurColorA = new Color(0.1f, 0.1f, 0.1f, 1);
        x.updateDynamically = true;

        if (dynamicFurProfile) {
            x.XFur_LoadFurProfile(dynamicFurProfile, 0);
        }



        x.XFur_UpdateFurMaterials();
    }

}
