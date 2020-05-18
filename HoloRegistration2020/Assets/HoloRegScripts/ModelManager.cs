using OpenPose;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UniGLTF;

public class ModelManager : MonoBehaviour
{
    private OPDatum datum;
    private GameObject organ;
    private string organString = "lung";
    SaveManager manager;
    Camera m_MainCamera;
    Vector3[] lastCoord;
    private bool modelAttached;
    private bool organSelected;
    private int offsetX;
    private int offsetY;
    private int rotationX;
    private int rotationY;
    private int rotationZ;
    List<float> coordsX;
    List<float> coordsY;
    List<float> weightsY;
    List<float> weightsX;
    List<float> boundingBox;

    private string[] bodyPartList = new string[] {"Nose", "Neck", "RShoulder", "RElbow", "RWrist", "Lshoulder", "LElbow", "LWrist", "MidHip", "RHip", "RKnee", "RAnkle",
                                                    "LHip", "LKnee", "LAnkle", "REye", "LEye", "REar", "LEar", "LBigToe", "LSmallToe", "LHeel", "RBigToe", "RSmallToe", "RHeel"};



    // Start is called before the first frame update
    void Start()
    {
        //Initi
        organ = this.gameObject;
        modelAttached = false;
        lastCoord = new Vector3[2];
        m_MainCamera = Camera.main;
        manager = transform.parent.GetComponent<ModelLoader>().manager;
        manager.LoadConfigData();
        coordsX = new List<float>();
        coordsY = new List<float>();
        weightsX = new List<float>();
        weightsY = new List<float>();
        boundingBox = new List<float>();
    }

    public void Update()
    {

    }
    //Extract relveant information from the datum
    public void ParseDatum(OPDatum datumX, float scale)
    {
        datum = datumX;
        float[] organBodyParts;
        ExtractBodyCoords(0);
        //If not all body parts are present, update UI
        if (coordsX.Contains(0F))
        {
            MissingPartDisplay();
        }
        else
        {
            ExtractBodyCoords(1);
            ExtractWeights(0);
            ExtractWeights(1);
            ExtractBox();
            OrganDisplay(scale);

        }
    }


    public void MissingPartDisplay()
    {
        float[] organBodyParts;
        manager.organBodyParts.TryGetValue(organString, out organBodyParts);
        //Create binary list of missing values
        float[] zeroIndexes = coordsX.Select(b => b == 0F ? 1F : 0F).ToArray();
        
        Debug.Log(coordsX[0].ToString() + " " + coordsX[1].ToString() + " " + coordsX[2].ToString());
        Debug.Log(zeroIndexes[0].ToString() + zeroIndexes[1].ToString() + zeroIndexes[2].ToString());

        //Identufy body part index
        zeroIndexes = zeroIndexes.Zip(organBodyParts, (zeroInd, part) => zeroInd == 1F ? part : -1F).Where(c => c >= 0).ToArray();
        string errorBodyPart = "Missing body parts: ";
        //create string and display on UI
        foreach (float part in zeroIndexes)
        {
            errorBodyPart += bodyPartList[(int)part] + " ";
        }
        Debug.Log(errorBodyPart);


        this.transform.parent.GetComponent<ModelLoader>().BodyPartsMissing.GetComponent<Text>().text = errorBodyPart;
        if(modelAttached == true)
        {
            this.transform.GetChild(0).gameObject.SetActive(false);
        }
        
    }

    public void OrganDisplay(float scale)
    {
        this.transform.GetChild(0).gameObject.SetActive(true);
        ExtractBodyCoords(1);
        ExtractWeights(0);
        ExtractWeights(1);
        ExtractBox();

        //Get UI transformations
        offsetX = this.transform.parent.GetComponent<ModelLoader>().offsetX;
        offsetY = this.transform.parent.GetComponent<ModelLoader>().offsetY;
        rotationX = this.transform.parent.GetComponent<ModelLoader>().rotationX;
        rotationY = this.transform.parent.GetComponent<ModelLoader>().rotationY;
        rotationZ = this.transform.parent.GetComponent<ModelLoader>().rotationZ;

        //calculate position using weights from config
        Vector3 avgPosWeighted = new Vector3(coordsX.Zip(weightsX, (coord, weight) => coord * weight).Where(c => c != 0).Average() + offsetX, coordsY.Zip(weightsY, (coord, weight) => coord * weight).Where(c => c != 0).Average() + offsetY, -100F);
        
        //Use a rollnig average for the position to prevent jittering
        gameObject.transform.localPosition = (avgPosWeighted + lastCoord[0] + lastCoord[1]) / 3;
        lastCoord[1] = lastCoord[0];
        lastCoord[0] = avgPosWeighted;
        
        //Set rotation from UI
        gameObject.transform.eulerAngles = new Vector3(rotationX, rotationY, rotationZ);

        //calculate world space distances
        Vector3 witdh1 = new Vector3(boundingBox[0], 0F, 500F);
        Vector3 width2 = new Vector3(boundingBox[1], 0F, 500F);
        Vector3 width = Vector3.Scale((m_MainCamera.ScreenToWorldPoint(witdh1) - m_MainCamera.ScreenToWorldPoint(width2)), (new Vector3(boundingBox[2], boundingBox[2], boundingBox[2])));

        Vector3 height1 = new Vector3(0, boundingBox[3], 500F);
        Vector3 height2 = new Vector3(0F, boundingBox[4], 500F);
        Vector3 height = Vector3.Scale((m_MainCamera.ScreenToWorldPoint(height1) - m_MainCamera.ScreenToWorldPoint(height2)), (new Vector3(boundingBox[5], boundingBox[5], boundingBox[5])));

        /*
        Debug.Log("canvas coords " + boundingBox[0] + " " + boundingBox[1]);
        Debug.Log("screentwp " + m_MainCamera.ScreenToWorldPoint(witdh1) + " " + m_MainCamera.ScreenToWorldPoint(width2));
        Debug.Log("width " + ((m_MainCamera.ScreenToWorldPoint(witdh1) - m_MainCamera.ScreenToWorldPoint(width2))) + " widthScal " + width);
        Debug.Log("scale " + scale);
        */

        //Calculate scale
        Bounds organModelBounds = GetMeshBounds();

        float widthX = (width.x / 2) * gameObject.transform.localScale.x / organModelBounds.extents.x;
        float heightY = (height.y / 2) * gameObject.transform.localScale.y / organModelBounds.extents.y;
        float avgScale = (widthX + heightY) / 2;
        



        gameObject.transform.localScale = new Vector3(avgScale * scale, avgScale * scale, avgScale * scale);
        Bounds organModelBoundsNew = GetMeshBounds();
        Debug.Log(organModelBounds + " " + organModelBoundsNew + " " + widthX + "  " + heightY);

        this.transform.parent.GetComponent<ModelLoader>().BodyPartsMissing.GetComponent<Text>().text = "All body parts present";
    }

   
    private Bounds getBounds()
    {
        Bounds totalBounds = organ.transform.GetChild(0).GetComponentInChildren<Renderer>().bounds;
        
        return totalBounds;
    }
    
    private void ExtractBodyCoords(int index)
    {
        List<float> bodyCoords;
        float[] organBodyParts;

        if (index == 0)
        {
            bodyCoords = coordsX;
        }
        else
        {
            bodyCoords = coordsY;
        }

        bodyCoords.Clear();
        


        if(manager.organBodyParts.TryGetValue(organString, out organBodyParts)){
            foreach(float partNo in organBodyParts)
            {
                bodyCoords.Add(datum.poseKeypoints.Get(0, (int) partNo, index));
            }

            
        }
        else
        {
            Debug.Log("Body part not found");
        }

        return;
    }

    private void ExtractWeights(int x)
    {
        //List<float> weights; 
        float[] organWeights;

        
        if (manager.organWeightsPosX.TryGetValue(organString, out organWeights))
        {
            weightsX = new List<float>(organWeights);
            
        }
        else
        {
            Debug.Log("Body part not found");
        }
        
       
        if (manager.organWeightsPosY.TryGetValue(organString, out organWeights))
        {
            weightsY = new List<float>(organWeights);
            
        }
        else
        {
            Debug.Log("Body part not found");
        }
        
        return;
    }

    private void ExtractBox()
    {
        List<float> box = boundingBox;
        float[] organWidth;
        float[] organHeight;

        boundingBox.Clear();
        if (manager.organWidth.TryGetValue(organString, out organWidth))
        {
            Debug.Log((int)organWidth[0]);
            box.Add(datum.poseKeypoints.Get(0, (int)organWidth[0], 0));
            box.Add(datum.poseKeypoints.Get(0, (int)organWidth[1], 0));
            box.Add(organWidth[2]);

            
        }
        else
        {
            Debug.Log("Body part not found");
        }

        if (manager.organHeight.TryGetValue(organString, out organHeight))
        {

            box.Add(datum.poseKeypoints.Get(0, (int)organHeight[0], 1));
            box.Add(datum.poseKeypoints.Get(0, (int)organHeight[1], 1));
            box.Add(organHeight[2]);

         
        }
        else
        {
            Debug.Log("Body part not found");
        }



        return;
    }

   
    public Vector3 GetMeshCenter()
    {
        MeshFilter[] filters = this.gameObject.GetComponentsInChildren<MeshFilter>();

        if (filters.Length > 0)
        {
            Bounds bounds = filters[0].mesh.bounds;
            for (int i = 1, ni = filters.Length; i < ni; i++)
            {
                bounds.Encapsulate(filters[i].mesh.bounds);
            }
            return bounds.center;
        }
        else
        {
            return new Vector3();
        };
    }
    public Bounds GetMeshBoundsF()
    {
        MeshFilter[] filters = this.gameObject.GetComponentsInChildren<MeshFilter>();

        if (filters.Length > 0)
        {
            Bounds bounds = filters[0].mesh.bounds;
            for (int i = 1, ni = filters.Length; i < ni; i++)
            {
                bounds.Encapsulate(filters[i].mesh.bounds);
            }
            return bounds;
        }
        else
        {
            return new Bounds();
        };
    }
    public Bounds GetMeshBounds()
    {
        Renderer[] renderers = this.gameObject.GetComponentsInChildren<Renderer>();
        //Debug.Log(renderers.Length);
        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1, ni = renderers.Length; i < ni; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
            return bounds;
        }
        else
        {
            return new Bounds();
        }
    }

    public void AttachModel()
    {

        Vector3 meshCenter = GetMeshCenter();
        modelAttached = true;
    }

    public void UpdateOrganString(string name)
    {
        organString = name;
    }
}

