var template = {
    "contained": {
        "address": "https://holoblob.blob.core.windows.net/mock-pacs/normal-chest-lung.zip",
        "connectionType": {
            "code": "direct-project",
            "system": "http://terminology.hl7.org/CodeSystem/endpoint-connection-type"
        },
        "id": "imagingEndpointId",
        "payloadType": [
            {
                "coding": [
                    {
                        "code": "ImagingStudy",
                        "system": "http://hl7.org/fhir/resource-types"
                    }
                ]
            }
        ],
        "resourceType": "Endpoint",
        "status": "active"
    },
    "endpoint": [
        {
            "reference": "#imagingEndpointId"
        }
    ],
    "id": "i100",
    "identifier": [
        {
            "system": "urn:ietf:rfc:3986",
            "use": "official",
            "value": "urn:oid:1.2.840.99999999.15185267.1563805279896"
        }
    ],
    "numberOfInstances": 273,
    "numberOfSeries": 1,
    "resourceType": "ImagingStudy",
    "series": [
        {
            "bodySite": {
                "code": "40983000",
                "display": "chest",
                "system": "http://snomed.info/sct"
            },
            "modality": {
                "code": "DX",
                "display": "Digital Radiography",
                "system": "http://dicom.nema.org/resources/ontology/DCM"
            },
            "number": 1,
            "numberOfInstances": 273,
            "started": "2013-09-25T22:45:29+01:00",
            "uid": "1.2.840.99999999.1.71566160.1563805279896"
        }
    ],
    "started": "2013-09-25T22:45:29+01:00",
    "status": "available",
    "subject": {
        "reference": "Patient/p100"
    }
}

module.exports = {template}; 