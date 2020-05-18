using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class OrganParamsCollection
{
    public OrganParams[] organParams;
    public string collectionName;

    public override string ToString()
    {
        string result = "Organ Params:";

        foreach (var param in organParams)
        {
            //result += string.Format("organ: {0}, bodyparts: {1}, weights: {2}", param.organName, param.bodyPart, param.weights);
        }
        return result;
    }
}
