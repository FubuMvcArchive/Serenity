describe('add tester', function () {

    it('add two numbers', function () {
        expect(add(0, 1)).toEqual(1);
        expect(add(1, 1)).toEqual(2);
        expect(add(3, 1)).toEqual(4);
        expect(add(5, 3)).toEqual(8);
    });

});