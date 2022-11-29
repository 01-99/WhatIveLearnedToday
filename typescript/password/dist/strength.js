var PasswordStrength = /** @class */ (function () {
    function PasswordStrength() {
    }
    PasswordStrength.prototype.checkPassword = function (password) {
        var strength = 0;
        if (!password) {
            return 0;
        }
        if (password.length > 8) {
            strength += 1;
        }
        if (password.match(/[a-z]+/)) {
            strength += 1;
        }
        if (password.match(/[A-Z]+/)) {
            strength += 1;
        }
        if (password.match(/[0-9]+/)) {
            strength += 1;
        }
        if (password.match(/[$@@#&!_.-]+/)) {
            strength += 1;
        }
        if (password.match(/(.)\1\1\1\1/)) {
            if (strength != 5)
                strength = -1;
        }
        return strength;
    };
    return PasswordStrength;
}());
module.exports = PasswordStrength;
