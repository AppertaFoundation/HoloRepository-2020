import {
  capitaliseString,
  getAgeFromDobString,
  getNumberOfHologramsString
} from "../../util/PatientUtil";
// @ts-ignore TS7015
import mockNow from "jest-mock-now";
import sampleHolograms from "../samples/sampleHolograms.json";
import { IHologram } from "../../../../types";

it("capitaliseString capitalises a string correctly", () => {
  const input = "foo bar";
  const result = capitaliseString(input);
  expect(result).toEqual("Foo bar");
});

it("capitaliseString handles an already capitalisedstring correctly", () => {
  const input = "Foo bar";
  const result = capitaliseString(input);
  expect(result).toEqual("Foo bar");
});

it("getAgeFromDobString functions correctly", () => {
  mockNow(new Date("2014-01-01"));
  const input = "1989-07-07";
  const result = getAgeFromDobString(input);
  expect(result).toBe(24);
});

it("getNumberOfHologramsString functions correctly", () => {
  const input = sampleHolograms as IHologram[];
  const result = getNumberOfHologramsString(input);
  expect(result).toEqual("3 holograms available");
});

it("getNumberOfHologramsString functions correctly for empty array", () => {
  const result = getNumberOfHologramsString([]);
  expect(result).toEqual("No holograms available");
});
