const chai = require("chai");
const expect = chai.expect;
// import sinon
const sinon = require("sinon");

const transmission_service = require('../services/transmission_service.js');

let study_retriever= require("../api/study_retriever.js");
let blob_uploader = require("../api/blob_uploader.js");
let fhir_client = require("../api/fhir_client.js");


 // Test controller with succesful services
 describe("POST /", () => {

   it("should transmit to HoloRepository EHR with success", (done) => {
        before(() => {
            sinon.stub(study_retriever, 'downloadStudy').returns({result:{payload:200}})
            sinon.stub(blob_uploader, 'uploadStudy').returns({result:{payload:200}})
            sinon.stub(fhir_client, 'insertFHIR').returns({result:{payload:200}})
        })

        let transmission = transmission_service.transmit;

        transmission.res.should.have.status(200);
        transmission.res.body.should.be.a('object');
        transmission.res.body.success.should.be.true;

        done()
   });

 });

 // Test controller with unsuccesful services
 describe("POST /", () => {

   it("should transmit to HoloRepository EHR with failiure", (done) => {
        before(() => {
            sinon.stub(study_retriever, 'downloadStudy').returns({result:{payload:500}})
            sinon.stub(blob_uploader, 'uploadStudy').returns({result:{payload:500}})
            sinon.stub(fhir_client, 'insertFHIR').returns({result:{payload:500}})
        })

        let transmission = transmission_service.transmit;

        transmission.res.should.have.status(500);
        transmission.res.body.should.be.a('object');
        transmission.res.body.success.should.be.false;

        done()
   });

 });