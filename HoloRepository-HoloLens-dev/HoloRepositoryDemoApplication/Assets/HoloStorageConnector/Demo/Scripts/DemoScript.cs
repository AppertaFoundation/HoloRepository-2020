﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloStorageConnector;
using TMPro;
using System;

public class DemoScript : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI SinglePatientInfo = null;
    [SerializeField]
    private TextMeshProUGUI buttonText = null;
    [SerializeField]
    private GameObject buttonTemplates = null;
    [SerializeField]
    private GameObject All = null;
    [SerializeField]
    private GameObject Single = null;

    public void GetAllPatients()
    {
        StartCoroutine(getAllPatients());
    }

    IEnumerator getAllPatients()
    {
        List<Patient> patientList = new List<Patient>();
        yield return HoloStorageClient.GetMultiplePatients(patientList, "p-101,p-102,p-103");
        All.SetActive(true);
        foreach (Patient patient in patientList)
        {
            GameObject button = Instantiate(buttonTemplates) as GameObject;
            button.SetActive(true);
            button.GetComponent<DemoScript>().SetText(patient.name.full);
            button.transform.SetParent(buttonTemplates.transform.parent, false);
        }
    }

    public void GetPatientByID()
    {
        StartCoroutine(getPatientByID());
    }

    IEnumerator getPatientByID()
    {
        Patient patient = new Patient();
        yield return HoloStorageClient.GetPatient(patient, "p-101");
        Single.SetActive(true);
        try
        {
            SinglePatientInfo.text = $"Patient name: \n{patient.name.full}\nGender: {patient.gender}\nDate of Birth: \n{patient.birthDate.Substring(0, 10)}";
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to set the text! \n[Error message]" + e.Message);
        }    
    }

    public void LoadModel()
    {
        HologramInstantiationSettings setting = new HologramInstantiationSettings
        {
            Manipulable = true,
            Rotation = new Vector3(0, 180, 0),
            Position = new Vector3(0.22f, -0.2f, 0.8f),
            Size = 0.12f
        };
        HoloStorageClient.LoadHologram("ef051fa3-ccfd-4a3e-8bf6-3cd4c1c8dc23", setting);
    }

    public void SetText(string name)
    {
        buttonText.text = name;
    }
}