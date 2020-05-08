/*
 * HoloStorage Accessor API
 *
 * API to access holograms and metadata from HoloStorage
 *
 * API version: 1.0.0
 */

package holostorageaccessor

import (
	"encoding/json"
	"errors"
	"log"
	"strings"
	"time"
)

// Hologram - Metadata of a hologram
type Hologram struct {
	Hid                 string     `json:"hid,omitempty"`
	Title               string     `json:"title,omitempty"`
	Description         string     `json:"description,omitempty"`
	ContentType         string     `json:"contentType,omitempty"`
	FileSizeInKb        uint32     `json:"fileSizeInKb,omitempty"`
	BodySite            string     `json:"bodySite,omitempty"`
	DateOfImaging       *time.Time `json:"dateOfImaging,omitempty"`
	CreationDate        *time.Time `json:"creationDate,omitempty"`
	CreationMode        string     `json:"creationMode,omitempty"`
	CreationDescription string     `json:"creationDescription,omitempty"`
	Aid                 string     `json:"aid,omitempty"`
	Pid                 string     `json:"pid,omitempty"`
}

type HologramDocumentReferenceFHIR struct {
	ResourceType string              `json:"resourceType,omitempty"`
	ID           string              `json:"id,omitempty"`
	Status       string              `json:"status,omitempty"`
	Date         *time.Time          `json:"date,omitempty"`
	Type         CodeableConceptFHIR `json:"type,omitempty"`
	Content      []ContentFHIR       `json:"content,omitempty"`
	HologramMeta string              `json:"description,omitempty"`
	Subject      ReferenceFHIR       `json:"subject,omitempty"`
	Author       []ReferenceFHIR     `json:"author,omitempty"`
}

type HologramMeta struct {
	Description         string     `json:"description,omitempty"`
	CreationDescription string     `json:"creationDescription,omitempty"`
	BodySite            string     `json:"bodySite,omitempty"`
	DateOfImaging       *time.Time `json:"dateOfImaging,omitempty"`
}

type ReferenceFHIR struct {
	Reference string `json:"reference,omitempty"`
}

type CodeableConceptFHIR struct {
	Text string `json:"text,omitempty"`
}

type ContentFHIR struct {
	Attachment AttachmentFHIR `json:"attachment,omitempty"`
}
type AttachmentFHIR struct {
	ContentType string `json:"contentType,omitempty"`
	URL         string `json:"url,omitempty"`
	Size        uint32 `json:"size,omitempty"`
	Title       string `json:"title,omitempty"`
}

/*
Hologram Struct Methods
*/
func (h Hologram) GetHoloMetadata() HologramMeta {
	data := HologramMeta{
		Description:         h.Description,
		CreationDescription: h.CreationDescription,
		DateOfImaging:       h.DateOfImaging,
		BodySite:            h.BodySite,
	}
	return data
}

func (h HologramDocumentReferenceFHIR) GetHoloMetadata() HologramMeta {
	data := HologramMeta{}
	err := json.Unmarshal([]byte(h.HologramMeta), &data)
	if err != nil {
		log.Printf("Error parsing HologramMeta data: %s. Returning blank meta.", err.Error())
		data = HologramMeta{}
	}
	return data
}

func (h Hologram) ToFHIR() HologramDocumentReferenceFHIR {
	hologramDocRef := HologramDocumentReferenceFHIR{ResourceType: "DocumentReference", Status: "current"}
	hologramDocRef.ID = h.Hid
	hologramDocRef.Date = h.CreationDate
	hologramDocRef.Type = CodeableConceptFHIR{Text: h.CreationMode}

	// Process Attachment in ContentFHIR
	attachmentData := AttachmentFHIR{ContentType: h.ContentType, Size: h.FileSizeInKb * 1024, Title: h.Title}
	if attachmentData != (AttachmentFHIR{}) {
		contentData := ContentFHIR{Attachment: attachmentData}
		hologramDocRef.Content = append(hologramDocRef.Content, contentData)
	}

	// Process HologramMeta
	holoMeta := h.GetHoloMetadata()
	if holoMeta != (HologramMeta{}) {
		holoMetadata, _ := json.Marshal(holoMeta)
		hologramDocRef.HologramMeta = string(holoMetadata)
	}

	// Process References
	if h.Pid != "" {
		hologramDocRef.Subject = ReferenceFHIR{Reference: "Patient/" + h.Pid}
	}
	if h.Aid != "" {
		hologramDocRef.Author = []ReferenceFHIR{ReferenceFHIR{Reference: "Practitioner/" + h.Aid}}
	}

	return hologramDocRef
}

func (h *HologramDocumentReferenceFHIR) UpdateAttachmentURL(url string) error {
	if len(h.Content) > 0 {
		h.Content[0].Attachment.URL = url
		return nil
	} else {
		return errors.New("No content within DocumentReference")
	}
}

func (h HologramDocumentReferenceFHIR) ToAPISpec() Hologram {
	hologramData := Hologram{}
	hologramData.Hid = h.ID
	hologramData.CreationDate = h.Date
	hologramData.CreationMode = h.Type.Text

	if len(h.Author) > 0 {
		if strings.HasPrefix(h.Author[0].Reference, "Practitioner/") {
			hologramData.Aid = h.Author[0].Reference[len("Practitioner/"):]
		} else {
			hologramData.Aid = h.Author[0].Reference
		}
	}
	if (h.Subject != ReferenceFHIR{}) {
		if strings.HasPrefix(h.Subject.Reference, "Patient/") {
			hologramData.Pid = h.Subject.Reference[len("Patient/"):]
		} else {
			hologramData.Pid = h.Subject.Reference
		}
	}

	meta := h.GetHoloMetadata()
	hologramData.Description = meta.Description
	hologramData.BodySite = meta.BodySite
	hologramData.DateOfImaging = meta.DateOfImaging
	hologramData.CreationDescription = meta.CreationDescription

	if len(h.Content) > 0 {
		attachmentData := h.Content[0].Attachment
		hologramData.Title = attachmentData.Title
		hologramData.FileSizeInKb = attachmentData.Size / 1024
		hologramData.ContentType = attachmentData.ContentType
	}

	return hologramData
}
