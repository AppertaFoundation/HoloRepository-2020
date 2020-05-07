import { Application } from "express";
import PatientsRouter from "./api/routes/patients/patients.router";
import HologramsRouter from "./api/routes/holograms/holograms.router";
import PipelinesRouter from "./api/routes/pipelines/pipelines.router";
import PractitionersRouter from "./api/routes/practitioners/practitioners.router";
import ImagingStudiesRouter from "./api/routes/imagingStudies/imagingStudies.router";

const apiVersion = 1;
const apiPrefix = `/api/v${apiVersion}`;

const practitionersRoute = `${apiPrefix}/practitioners`;
const patientsRoute = `${apiPrefix}/patients`;
const hologramsRoute = `${apiPrefix}/holograms`;
const pipelinesRoute = `${apiPrefix}/pipelines`;
const imagingStudiesRoute = `${apiPrefix}/imagingStudies`;

const routes = (app: Application): void => {
  app.use(practitionersRoute, PractitionersRouter);
  app.use(patientsRoute, PatientsRouter);
  app.use(hologramsRoute, HologramsRouter);
  app.use(pipelinesRoute, PipelinesRouter);
  app.use(imagingStudiesRoute, ImagingStudiesRouter);
};

export default routes;
