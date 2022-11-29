const p = require('./dist/strength')

const test = async function(password, strength){

    const s = new p();
    sut  = await s.checkPassword(password, strength);
    console.log("password="+password +" strength="+strength);
    console.log(sut==strength);
   };


test("abc", 1);
test("Abc", 2);
test("Abc_", 3);

