import React, { Component, ComponentType, Context, createContext, PureComponent } from "react";
import { IPatient, IPipeline, IPractitioner } from "../../../../types";

export type PidToPatientsMap = Record<string, IPatient>;

export type PropsWithContext = { context?: IAppState };

export interface IAppState {
  practitioner?: IPractitioner;
  patients: PidToPatientsMap;
  selectedPatientId?: string;
  pipelines: IPipeline[];
  pin?: string;
  loginWasInitiated: boolean;
  handlePractitionerChange: Function;
  handlePatientsChange: Function;
  handleSelectedPatientIdChange: Function;
  handlePipelinesChange: Function;
  handleDeleteHolograms: Function;
  handleDownloadHolograms: Function;
  handleHologramCreated_Upload: Function;
  handleHologramCreated_Generate: Function;
  handleLogin: Function;
  handleLogout: Function;
}

// Subset of initial state that can be reused to wipe practitioner-specific data after logout
const initialPractitionerSpecificState = {
  practitioner: undefined,
  patients: {},
  selectedPatientId: undefined,
  pin: undefined,
  loginWasInitiated: false
};

const initialState: IAppState = {
  ...initialPractitionerSpecificState,
  pipelines: [],
  handlePractitionerChange: () => {},
  handleSelectedPatientIdChange: () => {},
  handlePatientsChange: () => {},
  handlePipelinesChange: () => {},
  handleDeleteHolograms: () => {},
  handleDownloadHolograms: () => {},
  handleHologramCreated_Upload: () => {},
  handleHologramCreated_Generate: () => {},
  handleLogin: () => {},
  handleLogout: () => {}
};

// using same interface for App state and context
const AppContext: Context<IAppState> = createContext(initialState);

/**
 * Higher-order component to wrap an arbitrary component and provide it with app context.
 *
 * @param InnerComponent  the component to wrap
 * @param pureComponent   whether to return a PureComponent (use false if InnerComponent uses other
 *                        props and fails to update properly due to PureComponent's flat diffs)
 */
const withAppContext = <ComponentProps extends {}>(
  InnerComponent: ComponentType<ComponentProps>,
  pureComponent: boolean = true
) => {
  const componentType = pureComponent ? PureComponent : Component;
  return class WithContext extends componentType<ComponentProps & PropsWithContext> {
    render() {
      return (
        <AppContext.Consumer>
          {(context: IAppState) => <InnerComponent {...this.props} context={context} />}
        </AppContext.Consumer>
      );
    }
  };
};

export { initialState, initialPractitionerSpecificState, AppContext, withAppContext };
