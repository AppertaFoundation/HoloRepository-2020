using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;

namespace HoloStorageConnector
{
    /// <summary>
    /// Class <c>HologramInstantiationSettings</c> is used set the transform settings before load the 3D object from server
    /// For example, the position, rotation and scale of the 3D object
    /// </summary>
    public class HologramInstantiationSettings
    {
        #region Properties
        /// <summary>
        /// Set a name for the loaded model
        /// </summary>
        public string Name { get; set; } = "LoadedModel";
        /// <summary>
        /// Set position for the loaded model, the parameter should be a Vector3 value
        /// </summary>
        public Vector3 Position { get; set; } = new Vector3(0f, 0f, 0f);
        /// <summary>
        /// Set rotation for the loaded model, the parameter should be a Vector3 value
        /// </summary>
        public Vector3 Rotation { get; set; } = new Vector3(0f, 0f, 0f);
        /// <summary>
        /// Real size in the scene, The longest side of the loaded object will be set to this value
        /// </summary>
        public float Size { get; set; } = 0.5f;
        /// <summary>
        /// Determine whether the object could be manipulated
        /// </summary>
        public bool Manipulable { get; set; } = true;
        /// <summary>
        /// Determine which scene you want to load the object
        /// </summary>
        public string SceneName { get; set; } = null;
        #endregion Properties

        /// <summary>
        /// Initialize transform settings of the gameobject
        /// </summary>
        /// <param name="gameobject">The loaded GameObject</param>
        public static void Initialize(GameObject gameobject1, HologramInstantiationSettings setting)
        {
            gameobject1.name = setting.Name;
            for (int i = 0;i < gameobject1.transform.childCount; i++)
            {
                GameObject gameobject = gameobject1.transform.GetChild(i).gameObject;
                Debug.Log(gameobject.name);

                foreach (MeshFilter meshFilter in gameobject.GetComponentsInChildren<MeshFilter>())
                {
                    Mesh mesh2 = meshFilter.sharedMesh;
                    mesh2.RecalculateNormals();
                }
                foreach (Renderer renderer in gameobject.GetComponentsInChildren<Renderer>())

                {
					
					//This code changes the render mode at runtime, to allow the shader to use the transparency information
                    //https://forum.unity.com/threads/change-standard-shader-render-mode-in-runtime.318815/

                    Material material = renderer.material;
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                }

                Mesh mesh = gameobject.GetComponentsInChildren<MeshFilter>()[0].sharedMesh;
                mesh.RecalculateNormals();
                float max = Math.Max(Math.Max(mesh.bounds.size.x, mesh.bounds.size.y), mesh.bounds.size.z);

                float scaleSize = setting.Size / max;
                gameobject.transform.localScale = new Vector3(scaleSize, scaleSize, scaleSize);

                Vector3 initialPosition = new Vector3(mesh.bounds.center.x, -mesh.bounds.center.y, mesh.bounds.center.z) * scaleSize;
                gameobject.transform.position = initialPosition + setting.Position;

                gameobject.transform.eulerAngles = setting.Rotation;

                if (setting.Manipulable)
                {
                    gameobject.AddComponent<BoundingBox>();
                    gameobject.AddComponent<ManipulationHandler>();
                }
            }
            if (setting.SceneName != null)
            {
                try
                {
                    Scene HologramDisplayScene = SceneManager.GetSceneByName(setting.SceneName);
                    SceneManager.MoveGameObjectToScene(gameobject1, HologramDisplayScene);
                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to move the object to specific scene! \n[Error message]: " + e.Message);
                }              
            }
        }
    }
}