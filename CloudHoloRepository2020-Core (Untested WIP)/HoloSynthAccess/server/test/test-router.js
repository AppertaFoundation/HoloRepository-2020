const chai = require("chai");
const expect = chai.expect;
// import sinon
const sinon = require("sinon");
const transmission_router = require("../routes/transmission_router.js");
const transmission_service = require('../services/transmission_service.js');

// Configure chai
chai.use(chaiHttp);
chai.should();


 // Test for POST Request
 describe("POST /", () => {

   it("should return 200 after succesful services completion", (done) => {
        before(() => {
            sinon.stub(transmission_service, 'transmit').returns({result:{payload:200}})
        })

       chai.request(transmission_router)
           .post(`/`)
           .send({downloadURL: 'testURL'})
           .end((err, res) => {
               res.should.have.status(200);
               res.body.should.be.a('object');
               res.body.success.should.be.true;
               done();
           });
   });

   it("should return 500 after succesful services completion", (done) => {
    before(() => {
        sinon.stub(transmission_service, 'transmit').returns({result:{payload:500}})
    })

   chai.request(transmission_router)
       .post(`/`)
       .send({downloadURL: 'testURL'})
       .end((err, res) => {
           res.should.have.status(500);
           res.body.should.be.a('object');
           res.body.success.should.be.false;
           done();
       });
});

 });