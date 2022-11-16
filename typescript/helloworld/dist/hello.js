var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
        return extendStatics(d, b);
    };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
var Human = /** @class */ (function () {
    function Human(firstname) {
        this.FirstName = firstname;
    }
    return Human;
}());
var Person = /** @class */ (function (_super) {
    __extends(Person, _super);
    function Person(lastname, firstname) {
        var _this = _super.call(this, firstname) || this;
        _this.LastName = lastname;
        return _this;
    }
    Person.prototype.sayHello = function () {
        console.log("Hello " + this.FirstName + " " + this.LastName);
    };
    return Person;
}(Human));
var person = new Person("Binder", "Mario");
person.sayHello();
