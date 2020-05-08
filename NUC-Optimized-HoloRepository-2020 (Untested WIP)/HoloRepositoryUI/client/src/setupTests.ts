/**
 * This snippet is automatically run before each test when invoking "npm run test".
 */
import { configure } from "enzyme";
import Adapter from "enzyme-adapter-react-16";
import { initializeIcons } from "@uifabric/icons";
import BackendServerAxios from "./services/BackendServerAxios";
import MockAdapter from "axios-mock-adapter";

// Configure Enzyme adapter
configure({ adapter: new Adapter() });

// Configure Axios mock adapter to prevent all actual network calls and just return 200 for now
const mock = new MockAdapter(BackendServerAxios);
mock.onAny().reply(200);

// Note: See https://developer.microsoft.com/en-us/fabric/#/styles/web/icons#fabric-react
initializeIcons();
