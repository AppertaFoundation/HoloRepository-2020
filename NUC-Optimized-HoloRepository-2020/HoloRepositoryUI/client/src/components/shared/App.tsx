import React, { Component } from "react";
import "./App.scss";
import MainContainer from "./MainContainer";
import { initializeIcons } from "@uifabric/icons";
import { loadTheme } from "office-ui-fabric-react";
import BackendServerService from "../../services/BackendServerService";
import { IHologram, IPatient, IPractitioner, IPipeline } from "../../../../types";
import { AppContext, IAppState, initialState, initialPractitionerSpecificState } from "./AppState";
import { navigate } from "@reach/router";

// Note: See https://developer.microsoft.com/en-us/fabric/#/styles/web/icons#fabric-react
initializeIcons();

// Note: See https://fabricweb.z5.web.core.windows.net/pr-deploy-site/refs/heads/master/theming-designer/index.html
// The colour scheme is created to reflect the colours of antdesign. With this, the overall look
// becomes a bit more coherent, as the two styling frameworks used (Fabric and antd) are different
loadTheme({
  palette: {
    themePrimary: "#1890ff",
    themeLighterAlt: "#f6fbff",
    themeLighter: "#daedff",
    themeLight: "#b9ddff",
    themeTertiary: "#74bcff",
    themeSecondary: "#339cff",
    themeDarkAlt: "#1581e6",
    themeDark: "#116dc2",
    themeDarker: "#0d508f",
    neutralLighterAlt: "#f8f8f8",
    neutralLighter: "#f4f4f4",
    neutralLight: "#eaeaea",
    neutralQuaternaryAlt: "#dadada",
    neutralQuaternary: "#d0d0d0",
    neutralTertiaryAlt: "#c8c8c8",
    neutralTertiary: "#595959",
    neutralSecondary: "#373737",
    neutralPrimaryAlt: "#2f2f2f",
    neutralPrimary: "#000000",
    neutralDark: "#151515",
    black: "#0b0b0b",
    white: "#ffffff"
  }
});

class App extends Component<any, IAppState> {
  constructor(props: any) {
    super(props);
    this.state = {
      ...initialState,
      handlePractitionerChange: this._handlePractitionerChange,
      handlePatientsChange: this._handlePatientsChange,
      handleSelectedPatientIdChange: this._handleSelectedPatientIdChange,
      handlePipelinesChange: this._handlePipelinesChange,
      handleDeleteHolograms: this._handleDeleteHolograms,
      handleDownloadHolograms: this._handleDownloadHolograms,
      handleHologramCreated_Upload: this._handleHologramCreated_Upload,
      handleHologramCreated_Generate: this._handleHologramCreated_Generate,
      handleLogin: this._handleLogin,
      handleLogout: this._handleLogout
    };
  }

  render() {
    return (
      <AppContext.Provider value={this.state}>
        <div className="App">
          <MainContainer />
        </div>
      </AppContext.Provider>
    );
  }

  private _handleLogin = (practitionerId: string, pin: string) => {
    BackendServerService.getPractitioner(practitionerId).then(practitioner => {
      console.log("Fetched data: practitioner", practitioner);
      this._handlePractitionerChange(practitioner!);
    });

    // Fetch all patients for which the current practitioner is responsible
    BackendServerService.getAllPatientsForPractitioner(practitionerId).then(patients => {
      console.log("Fetched data: patients", patients);
      this._handlePatientsChange(patients!);
    });

    // Fetch information about available pipelines
    BackendServerService.getAllPipelines().then(pipelines => {
      console.log("Fetched data: pipelines", pipelines);
      this._handlePipelinesChange(pipelines || []);
    });
    this.setState({ pin });
    this.setState({ loginWasInitiated: true });
  };

  private _handleLogout = () => {
    console.log("Practitioner logged out");
    navigate("/");
    this.setState({ ...initialPractitionerSpecificState });
  };

  private _fetchPatientSpecificData = () => {
    this._fetchImagingStudiesForPatients();
    this._fetchHologramsForPatients();
  };

  private _fetchImagingStudiesForPatients = () => {
    const { patients } = this.state;
    if (!patients) return;

    BackendServerService.getImagingStudiesForAllPatients(patients).then(combinedResult => {
      console.log("Fetched data: imaging studies", combinedResult);
      for (const pid in combinedResult) {
        const studies = combinedResult[pid];
        const patient = patients[pid];
        if (patient) {
          patient.imagingStudies = studies;
        }
        this.setState({
          patients: {
            ...patients,
            [pid]: patient
          }
        });
      }
    });
  };

  private _fetchHologramsForPatients = () => {
    // Note: Similar with _fetchImagingStudiesForPatients, should be refactored
    const { patients } = this.state;
    if (!patients) return;

    BackendServerService.getHologramsForAllPatients(patients).then(combinedResult => {
      console.log("Fetched data: holograms", combinedResult);
      for (const pid in combinedResult) {
        const holograms = combinedResult[pid];
        const patient = patients[pid];

        // Note: As the Accessor API only includes pid for patients, set patient name to current patient
        holograms.forEach(hologram => (hologram.patientName = patient.name.full));

        patient.holograms = holograms;
        this.setState({
          patients: {
            ...patients,
            [pid]: patient
          }
        });
      }
    });
  };

  private _handlePractitionerChange = (practitioner: IPractitioner) => {
    this.setState({ practitioner });
  };

  private _handlePatientsChange = (patientsArray?: IPatient[]) => {
    if (!patientsArray) return;

    console.debug(`Received ${patientsArray.length} patients`);
    const patients = patientsArray.reduce(
      (accumulator, patient) => ({ ...accumulator, [patient.pid]: patient }),
      {}
    );
    this.setState({ patients }, this._fetchPatientSpecificData);
  };

  private _handleSelectedPatientIdChange = (pid: string) => {
    this.setState({ selectedPatientId: pid });
  };

  private _handlePipelinesChange = (pipelines: IPipeline[]) => {
    this.setState({ pipelines });
  };

  private _handleDeleteHolograms = (hids: string[]) => {
    hids.forEach(hid =>
      BackendServerService.deleteHologramById(hid).then(response => {
        if (response === true) {
          this._handleHologramDeleted(hid);
        }
      })
    );
  };

  private _handleDownloadHolograms = (hids: string[]) => {
    hids.forEach(hid => {
      BackendServerService.downloadHologramById(hid);
    });
  };

  private _handleHologramDeleted = (hid: string) => {
    const pid = this._getPidForHid(hid);
    const patient = pid && this.state.patients[pid];
    if (!pid || !patient || !patient.holograms) {
      return;
    }
    patient.holograms = patient.holograms.filter(hologram => hologram.hid !== hid);

    // Note: Duplicate code, should be refactored
    this.setState({
      patients: {
        ...this.state.patients,
        [pid]: patient
      }
    });
  };

  private _handleHologramCreated_Upload = (hologram: IHologram) => {
    const pid = hologram.pid;
    const patient = this.state.patients[pid];

    // Adding data because Accessor only sends aid and pid
    hologram.patientName = patient.name.full;
    hologram.authorName = this.state.practitioner!.name.full;

    if (!patient.holograms) {
      patient.holograms = [];
    }
    patient.holograms.push(hologram);

    this.setState({
      patients: {
        ...this.state.patients,
        [pid]: patient
      }
    });
  };

  private _handleHologramCreated_Generate = () => {
    // A wrapper around private function to refresh all. This workaround is required, as the
    // HoloPipelines currently can't return the newly generated hologram and thus require
    // all holograms to be refreshed.
    this._fetchHologramsForPatients();
  };

  private _getPidForHid = (hid: string): string | null => {
    const patient = Object.values(this.state.patients).find(
      patient => patient.holograms && patient.holograms.find(hologram => hologram.hid === hid)
    );

    return patient ? patient.pid : null;
  };
}

export default App;
