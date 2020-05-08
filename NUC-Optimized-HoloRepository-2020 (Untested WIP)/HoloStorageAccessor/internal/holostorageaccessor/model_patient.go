/*
 * HoloStorage Accessor API
 *
 * API to access holograms and metadata from HoloStorage
 *
 * API version: 1.0.0
 */

package holostorageaccessor

// Patient - Metadata of a single patient
type Patient struct {
	Pid       string      `json:"pid,omitempty"`
	Gender    string      `json:"gender,omitempty"`
	BirthDate string      `json:"birthDate,omitempty"`
	Name      *PersonName `json:"name,omitempty"`
}

// PatientFHIR - Components of the relevant Patient FHIR resource
type PatientFHIR struct {
	ResourceType string          `json:"resourceType"`
	ID           string          `json:"id"`
	Name         []HumanNameFHIR `json:"name,omitempty"`
	Gender       string          `json:"gender,omitempty"`
	BirthDate    string          `json:"birthDate,omitempty"`
}

// ToFHIR - Convert PatientBasic schema to FHIR Patient schema
func (r Patient) ToFHIR() PatientFHIR {
	fhirData := PatientFHIR{ResourceType: "Patient"}
	fhirData.ID = r.Pid
	fhirData.Gender = r.Gender
	fhirData.BirthDate = r.BirthDate

	if r.Name != nil {
		name := r.Name.ToFHIR()
		if name.Text != "" || name.Family != "" || len(name.Prefix) > 0 || len(name.Given) > 0 {
			fhirData.Name = append(fhirData.Name, name)
		}
	}

	return fhirData
}

func (r PatientFHIR) ToAPISpec() Patient {
	patientData := Patient{}
	patientData.Pid = r.ID
	patientData.Gender = r.Gender
	patientData.BirthDate = r.BirthDate
	if len(r.Name) > 0 {
		name := r.Name[0].ToAPISpec()
		patientData.Name = &name
	}

	return patientData
}
