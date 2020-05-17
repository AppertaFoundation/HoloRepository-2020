import React from 'react';
import { shallow } from 'enzyme';
import App from '../App';
import renderer from 'react-test-renderer';

let wrapper = shallow(<App/>);

describe('App component', () => {
  it('Renders correctly', () => {
    expect(wrapper).toMatchSnapshot();
  });

  it('header title matches App Name', () => {
    expect(wrapper.find('.header-container').text()).toEqual('HoloSynthAccess');
  });
});