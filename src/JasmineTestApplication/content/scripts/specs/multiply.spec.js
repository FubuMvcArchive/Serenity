describe('multiply tester', function () {

    it('add two numbers', function () {
        expect(multiply(1, 0)).toEqual(0);
        expect(multiply(2, 2)).toEqual(4);
        expect(multiply(3, 3)).toEqual(9);
        expect(multiply(4, 4)).toEqual(16);
    });

});