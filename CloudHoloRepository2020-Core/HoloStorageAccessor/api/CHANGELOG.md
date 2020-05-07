# HoloStorage Accessor API Changelog
All changes done to the HoloStorage Accessor API spec will be documented here.

View the interactive documentation of the most updated API at the following link:
https://app.swaggerhub.com/apis/boonwj/HoloRepository/

## [1.1.0] - 2019-08-08
### Changed
- `GET /patients` query parameter change from `pid` to `pids`
- `GET /authors` query parameter change from `aid` to `aids`
- `GET /holograms` query parameter change from `hid` to `hids` and `pid` to `pids`

## [1.0.1] - 2019-08-08
### Changed
- `GET /patients`, `GET /holograms` and `GET /authors` to return 400 error when no query parameters are provided

## [1.0.0] - 2019-08-05
### Changed
- Update api description to be more explict with the return results of mass queries of `holograms`, `authors` and `patients`

## [1.0.0] - 2019-07-26
Bump version number to finalise initial api version

## [0.4.0] - 2019-07-26
### Changed
- `hologram` and `hologramUpload` data type
  - Added `bodySite` field
  - Added `dateOfImaging` field
  - Added `creationDescription` field
  - Removed `pipelineID` field
  - Renamed `createdDate` to `creationDate`
- `POST: /holograms` successful response is `hologram` data

## [0.3.1] - 2019-07-26
### Changed
- `POST: /holograms` request format changed
    - Removed hid field
    - Added full author field
    - Added full patient field
- `POST: /holograms` successful creation status code change from 200 to 201
- `hologram` metadata information changed to be more explicit
    - `author` to `aid`
    - `patient` to `pid`

## [0.3.0] - 2019-07-25
This is a fundamentally different approach to the schema style in 0.2.0.
### Changed
- `author` and `patient` fields in `Hologram` metadata now only contains UUIDs to their resource
- `/holograms` supports query via `pid`
- `/patients` no longer support query for `creationMode`
- `/patients` results no longer not contain `hologram` information
- `/patients` and `/authors` return values in an object of form {pid: {patientData}, ... }
- `/authors` return values in an object of form {aid: {authorData}, ... }
- `/holograms` return values in object of form {hid: [{authorData1}, ...], ... }.
### Added
- `/authors` endpoint to query for author information, available operations are similar to `/patients`
    - PUT: /authors
    - GET: /authors?aid=id1,id2,...,idx
    - GET: /authors/:aid

## [0.2.0] - 2019-07-25
### Changed
- Hologram metadata is nested within `metadata` property in Hologram Schema
- Patient metadata is nested within `metadata` property in Patient Schema
- `/patients` and `/patients/{pid}` results will contain hologram `author` information
### Added
- Add `FROM_DEPTHVISOR_RECORDING` to `creationMode` enum
- Query field `creationmode` for filtering results based on `creationMode` of holograms on `/patients` and `/holograms` endpoints
- Error message returned on errors

## [0.1.4] - 2019-07-14
### Changed
- `creationMode` field's content to "GENERATE_FROM_IMAGING_STUDY" and
"UPLOAD_EXISTING_MODE"

## [0.1.3] - 2019-07-13
### Added
- `creationMode` field to hologram resource based on HoloPipelines input
- `pipelineId` field to hologram resource based on HoloPipelines input
- Update title and description of hologram upload schema

## [0.1.2]
Initial version of api spec based on inputs from UI and HoloLens App team.
