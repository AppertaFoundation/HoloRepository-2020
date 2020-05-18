using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    //public static SaveManager Instance { get; private set; }

    public Dictionary<string, float[]> organBodyParts;
    public Dictionary<string, float[]> organWeightsPosX;
    public Dictionary<string, float[]> organWeightsPosY;
    public Dictionary<string, float[]> organWidth;
    public Dictionary<string, float[]> organHeight;
 
    public static SaveManager _instance;


    private OrganParamsCollection paramsCollection;
    private void Awake()
    {
        //Enforce Singleton
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }


    public static SaveManager Instance
    {
        get
        {
            return _instance;
        }
    }
    public void SaveConfigData()
    {
        string path = Application.dataPath + "/../config" + "/organConfig.json";
        Debug.Log(path);
        if (!System.IO.File.Exists(path))
        {
            //If config doesn't, create this base config
            Debug.Log("doesn't exist");
            var folder = Directory.CreateDirectory(Application.dataPath + "/../config");
            organBodyParts = new Dictionary<string, float[]>()
            {
                {"lung", new float[] { 2F, 5F,8F } },
                {"kidney", new float[] { 1F, 9F,12F } },
                {"abdomen", new float[] { 1F, 9F,12F } }
            };
            organWeightsPosX = new Dictionary<string, float[]>()
            {
                {"lung", new float[] { 0F, 0F, 1F } },
                {"kidney", new float[] { 1F, 1F,1F } },
                {"abdomen", new float[] { 1F, 1F, 1F } }
            };
            organWeightsPosY = new Dictionary<string, float[]>()
            {
                {"lung", new float[] { 1F, 1F, 0.75F } },
                {"kidney", new float[] { 1.2F, 1F,0F } },
                {"abdomen", new float[] { 1F, 1F,0F } }
            };
            organWidth = new Dictionary<string, float[]>()
            {
                {"lung", new float[] { 5F, 2F, 0.85F } },
                {"kidney", new float[] { 12F, 9F, 0.85F } },
                {"abdomen", new float[] { 5F, 2F, 1F } },
            };
            organHeight = new Dictionary<string, float[]>()
            {
                {"lung", new float[] { 8F, 1F,  0.7F } },
                {"kidney", new float[] { 8F, 1F, 0.3F } },
                {"abdomen", new float[] { 8F, 1F, 0.5F } }
            };



            GenerateParamFromDict();
            using (StreamWriter stream = new StreamWriter(path))
            {
                string json = JsonUtility.ToJson(paramsCollection);
                stream.Write(json);
            }

        }
        else
        {
            //Only used if config needs saving inernally, currently not used
            if (!(organBodyParts == null || organWeightsPosX == null || organWeightsPosY == null || organWidth == null || organHeight == null))
            {
                GenerateParamFromDict();
                using (StreamWriter stream = new StreamWriter(path))
                {
                    string json = JsonUtility.ToJson(paramsCollection);
                    stream.Write(json);
                }
            }
        }
    }

    private void GenerateParamFromDict()
    {
        //converting dictionary to a collection of objects
        OrganParams[] paramss = new OrganParams[organBodyParts.Count];
        int i = 0;
        
        foreach (KeyValuePair<string, float[]> pair in organBodyParts)
        {
            paramss[i] = new OrganParams() { organName = pair.Key , bodyPart = pair.Value, weightsPosX = organWeightsPosX[pair.Key], weightsPosY = organWeightsPosY[pair.Key], width = organWidth[pair.Key],
            height = organHeight[pair.Key]};
            i++;
            
        }
        paramsCollection = new OrganParamsCollection() { organParams = paramss, collectionName = "organs" };
        
    }

    public void LoadConfigData()
    {
        //Coverting as JSON representing a collection of objects to dictionaries
        string path = Application.dataPath + "/../config" + "/organConfig.json";
        
        if (!System.IO.File.Exists(path))
        {
            SaveConfigData();
        }
        using (StreamReader stream = new StreamReader(path))
        {
            string json = stream.ReadToEnd();
            paramsCollection = JsonUtility.FromJson<OrganParamsCollection>(json);
        }

        
        organBodyParts = new Dictionary<string, float[]>();
        organWeightsPosX = new Dictionary<string, float[]>();
        organWeightsPosY = new Dictionary<string, float[]>();
        organWidth = new Dictionary<string, float[]>();
        organHeight = new Dictionary<string, float[]>();

        foreach (OrganParams param in paramsCollection.organParams)
        {
            organBodyParts.Add(param.organName, param.bodyPart);
            organWeightsPosX.Add(param.organName, param.weightsPosX);
            organWeightsPosY.Add(param.organName, param.weightsPosY);
            organWidth.Add(param.organName, param.width);
            organHeight.Add(param.organName, param.height);
            Debug.Log("param.organName");
        }
    }
}
