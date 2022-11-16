class Human {
    FirstName: string;

    constructor(firstname: string) {
        this.FirstName = firstname;
    }
}

class Person extends Human {
    LastName: string;
    constructor(lastname: string, firstname: string) {
        super(firstname);
        this.LastName = lastname;
    }

    sayHello() {
        console.log("Hello " + this.FirstName + " " + this.LastName);
    }
}

let person = new Person("Binder", "Mario");
person.sayHello();
