import React from 'react';
import { shallow } from 'enzyme';
import SearchTable from '../searchTable';
import renderer from 'react-test-renderer';
import { DropdownList } from 'react-widgets'

let wrapper = shallow(<SearchTable/>);

describe('Rendering', () => {
    it('Renders component correctly', () => {
    expect(wrapper).toMatchSnapshot();
    });

    it('should render one label', () => {
    expect(wrapper.find('label')).toHaveLength(1);
    });

    it('should render local form', () => {
        expect(wrapper.find('form')).toHaveLength(1);
    });

    it('should render local form search  text', () => {
        expect(wrapper.find('.searchLocalContainer').text()).toEqual('Search a body part locally:');
    });

    it('should render local inputs', () => {
        expect(wrapper.find('input')).toHaveLength(2);
    });


});

describe('Interactions', () => {
    it('should set local and body part', () => {
        wrapper.find('form').simulate('submit',{
         target: { value: "organ" },
         preventDefault: () => {
        }
    });
        expect(wrapper.state("bodyPart")).toEqual("organ");
        expect(wrapper.state.local).toEqual(false);
    });

    it('should set API and body part', () => {
        wrapper.find('.searchAPIContainer').simulate('change',
        {preventDefault: () => {
        }, target: { value: "organ" } 
    });
        expect(wrapper.state("bodyPart")).toEqual("organ");
        expect(wrapper.state.local).toEqual(true);
    });

    
    it('on change function is called', () => {
        const changeFn = jest.fn();
        const component = shallow(<DropdownList onChange={changeFn} />);
        component
        .find('Uncontrolled(DropdownList)')
        .simulate('change');
        expect(changeFn).toHaveBeenCalled();
    });
});