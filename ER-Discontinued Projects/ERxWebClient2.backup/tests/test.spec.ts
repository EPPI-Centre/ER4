import { async, fakeAsync, tick } from "@angular/core/testing";

describe("test", () => {
    it("equal 3", async(() => {
        expect(3 + 5).toEqual(8);
    }))
})