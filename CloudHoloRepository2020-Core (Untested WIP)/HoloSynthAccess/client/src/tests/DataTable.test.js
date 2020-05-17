import React from 'react';
import { shallow } from 'enzyme';
import DataTable from '../dataTable.js';
import renderer from 'react-test-renderer';
import testData from 'data/lung.json';
import { headers } from './data/headers.js';

const data = {
    bodyPart:'lung',
    local:false
}

let wrapper = shallow(<DataTable
    bodyPart={data.bodyPart}
    local={data.local}
/>);

let spyGetImagingStudies = jest
  .spyOn(DataTable.prototype, "dataFetch")
  .mockImplementation(() => Promise.resolve({
    columns: headers,
    rows: testData
  }));

afterAll(() => {
    spyGetImagingStudies.mockClear();
});

describe('App component', () => {
  it('Renders correctly', () => {
    expect(wrapper).toMatchSnapshot();
  });

  it('should set the props', () => {
    expect(wrapper.prop("bodyPart")).toEqual("lung");
});

});