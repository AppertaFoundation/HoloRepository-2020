using Siccity.GLTFUtility;
using System.Collections.Generic;
using System.IO;
using UniGLTF;
using UnityEngine;
using UnityEngine.UI;




public class ModelLoader : MonoBehaviour
{
    // Start is called before the first frame update
    string[] args;
    public int offsetX;
    public int offsetY;
    public int rotationX;
    public int rotationY;
    public int rotationZ;
    public GameObject offsetDisplayX;
    public GameObject offsetDisplayY;
    public GameObject rotationDisplayX;
    public GameObject rotationDisplayY;
    public GameObject rotationDisplayZ;
    public bool modelLoaded;
    public bool enableModel;
    public GameObject organObject;
    public Dropdown dropdownListOrgan;
    public Dropdown dropdownListModel;
    public List<string> organOptions;
    public SaveManager manager;
    public List<string> fileNames;
    public List<FileInfo> fileInfoList;
    public ImporterContext context;
    public GameObject BodyPartsMissing;
    public Shader gltfTansparencyMaterial;
    public GameObject popup;




    public void SetOffsetX(string s) { int res; if (int.TryParse(s, out res)) { offsetX = res; }; }
    public void SetOffsetY(string s) { int res; if (int.TryParse(s, out res)) { offsetY = res; }; }

    public void SetRotationX(string s) { int res; if (int.TryParse(s, out res)) { rotationX = res % 360; }; rotationDisplayX.GetComponent<InputField>().text = rotationX.ToString(); }

    public void SetRotationY(string s) { int res; if (int.TryParse(s, out res)) { rotationY = res % 360; }; rotationDisplayY.GetComponent<InputField>().text = rotationY.ToString(); }

    public void SetRotationZ(string s) { int res; if (int.TryParse(s, out res)) { rotationZ = res % 360; }; rotationDisplayZ.GetComponent<InputField>().text = rotationZ.ToString(); }

    public void OpenPopup() { popup.SetActive(true); }

    public void ClosePopup() { popup.SetActive(false); }
    public void SetModelEnabled(bool enabled) { this.enableModel = enabled; UpdateModelRenderer(); }


    private void Awake()
    {
        modelLoaded = false;
        //Initialise SaveManager
        manager = new SaveManager();
        manager.LoadConfigData();
        //Update UI elements
        UpdateDropdownOptions();
        CheckForModels();
        //Get any command line args
        args = System.Environment.GetCommandLineArgs();

    }
    void Start()
    {
        offsetX = 0;
        offsetY = 0;
        rotationX = 0;
        rotationY = 0;
        rotationZ = 0;
        if (args.Length == 5)
        {
            LoadModelFromCommandLine();
        }
        


    }

    public void Update()
    {
        //Debug.Log(offsetY.ToString());
        //Debug.Log(enableModel);
    }

    public void OffsetIncrementX()
    {
        offsetX = ++offsetX;
        offsetDisplayX.GetComponent<InputField>().text = offsetX.ToString();
    }

    public void OffsetDecrementX()
    {
        offsetX = --offsetX;
        offsetDisplayX.GetComponent<InputField>().text = offsetX.ToString();
    }

    public void OffsetIncrementY()
    {
        offsetY = ++offsetY;
        offsetDisplayY.GetComponent<InputField>().text = offsetY.ToString();
    }

    public void OffsetDecrementY()
    {
        offsetY = --offsetY;
        offsetDisplayY.GetComponent<InputField>().text = offsetY.ToString();
    }

    public void RotationIncrementX()
    {
        rotationX = (rotationX + 90) % 360;
        rotationDisplayX.GetComponent<InputField>().text = rotationX.ToString();
    }

    public void RotationDecrementX()
    {
        rotationX = (rotationX - 90) % 360;
        rotationDisplayX.GetComponent<InputField>().text = rotationX.ToString();
    }

    public void RotationIncrementY()
    {
        rotationY = (rotationY + 90) % 360;
        rotationDisplayY.GetComponent<InputField>().text = rotationY.ToString();
    }

    public void RotationDecrementY()
    {
        rotationY = (rotationY - 90) % 360;
        rotationDisplayY.GetComponent<InputField>().text = rotationY.ToString();
    }

    public void RotationIncrementZ()
    {
        rotationZ = (rotationZ + 90) % 360;
        rotationDisplayZ.GetComponent<InputField>().text = rotationZ.ToString();
    }

    public void RotationDecrementZ()
    {
        rotationZ = (rotationZ - 90) % 360;
        rotationDisplayZ.GetComponent<InputField>().text = rotationZ.ToString();
    }

    //Turn the model on or off
    public void UpdateModelRenderer()
    {
        
        organObject = this.transform.GetChild(0).gameObject;
        organObject.SetActive(enableModel);
    }

    //Update list of body parts
    private void UpdateDropdownOptions()
    {
        organOptions = new List<string>(manager.organBodyParts.Keys);
        dropdownListOrgan.ClearOptions();
        dropdownListOrgan.AddOptions(organOptions);


    }

    public void OrganStringChange(int index)
    {
        GameObject pivot = GameObject.Find("Pivot");
        ModelManager scriptmove = (ModelManager)pivot.GetComponent(typeof(ModelManager));
        scriptmove.UpdateOrganString(organOptions[index]);
    }
    public void OrganStringChange(string name)
    {
        GameObject pivot = GameObject.Find("Pivot");
        ModelManager scriptmove = (ModelManager)pivot.GetComponent(typeof(ModelManager));
        scriptmove.UpdateOrganString(name);
    }

    //Update list of models in the model folder
    public void CheckForModels()
    {
        string modelFolderPath = Application.dataPath + "/../models";
        if (!Directory.Exists(modelFolderPath))
        {
            var folder = Directory.CreateDirectory(Application.dataPath + "/../models");
        }
        var info = new DirectoryInfo(modelFolderPath);
        FileInfo[] fileInfo = info.GetFiles();
        fileNames = new List<string>();
        fileInfoList = new List<FileInfo>();


        fileNames.Add("Select Model");
        foreach (var file in fileInfo)
        {
            Debug.Log(file.Extension);
            if (file.Extension == ".glb" || file.Extension == ".gltf")
            {
                fileNames.Add(file.Name);
                fileInfoList.Add(file);
            }



        }
        dropdownListModel.ClearOptions();
        dropdownListModel.AddOptions(fileNames);
    }

    public Vector3 GetMeshCenter(GameObject root)
    {
        MeshFilter[] filters = root.GetComponentsInChildren<MeshFilter>();

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
    //Load model from dropdpown list
    public void LoadSelectedModel(int index)
    {
        if (index == 0 && modelLoaded == true)
        {
            Destroy(context.Root);
            CheckForModels();
            return;
        }

        if (modelLoaded == true)
        {
            Destroy(context.Root);
        }
        string path = fileInfoList[index - 1].FullName;
        context = new ImporterContext();
        context.Load(path);
        context.ShowMeshes();



        MeshRenderer[] meshes = context.Root.GetComponentsInChildren<MeshRenderer>();
        //set transparency of object
        foreach (MeshRenderer mesh in meshes)
        {
            mesh.material.shader = gltfTansparencyMaterial;
            mesh.material.SetColor("_SpecColor", new Color(0, 0, 0, 255));
        }



        //context.EnableUpdateWhenOffscreen();
        GameObject pivot = GameObject.Find("Pivot");
        Vector3 meshCenter = GetMeshCenter(context.Root);
        Debug.Log(meshCenter);






        context.Root.transform.position = pivot.transform.position - meshCenter;
        context.Root.transform.parent = pivot.transform;
        context.Root.SetActive(false);
        ModelManager scriptmove = (ModelManager)pivot.GetComponent(typeof(ModelManager));
        scriptmove.AttachModel();
        modelLoaded = true;
        CheckForModels();
        
    }
    //load model from command line args
    public void LoadModelFromCommandLine()
    {
        string modelFilePath = "";
        string organtype = "";

        for (int i = 0; i < args.Length; i++)
        {
            Debug.Log("ARG " + i + ": " + args[i]);
            if (args[i] == "-file")
            {
                modelFilePath = args[i + 1];
            }
            if (args[i] == "-organ")
            {
                organtype = args[i + 1];
            }
        }

        OrganStringChange(organtype);

        context = new ImporterContext();
        context.Load(modelFilePath);
        context.ShowMeshes();



        MeshRenderer[] meshes = context.Root.GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer mesh in meshes)
        {
            mesh.material.shader = gltfTansparencyMaterial;
            mesh.material.SetColor("_SpecColor", new Color(0, 0, 0, 255));
        }



        //context.EnableUpdateWhenOffscreen();
        GameObject pivot = GameObject.Find("Pivot");
        Vector3 meshCenter = GetMeshCenter(context.Root);
        Debug.Log(meshCenter);






        context.Root.transform.position = pivot.transform.position - meshCenter;
        context.Root.transform.parent = pivot.transform;
        context.Root.SetActive(false);
        ModelManager scriptmove = (ModelManager)pivot.GetComponent(typeof(ModelManager));
        scriptmove.AttachModel();
        modelLoaded = true;
        CheckForModels();
    }
}
