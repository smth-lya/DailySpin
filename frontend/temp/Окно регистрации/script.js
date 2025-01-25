// script.js
document.addEventListener("DOMContentLoaded", () => {
    const emailInput = document.getElementById("email");
    const emailHints = document.getElementById("email-hints").querySelectorAll("li");

    const passwordInput = document.getElementById("password");
    const passwordHints = document.getElementById("password-hints").querySelectorAll("li");

    emailInput.addEventListener("input", () => {
        const value = emailInput.value;
        emailHints[0].dataset.valid = value.includes("@");
        emailHints[1].dataset.valid = value.includes(".") && value.split("@")[1]?.includes(".");
    });

    passwordInput.addEventListener("input", () => {
        const value = passwordInput.value;
        passwordHints[0].dataset.valid = value.length >= 8;
        passwordHints[1].dataset.valid = /[A-Z]/.test(value);
        passwordHints[2].dataset.valid = /\d/.test(value);
        passwordHints[3].dataset.valid = /[!@#$%^&*(),.?":{}|<>]/.test(value);
    });

    const form = document.getElementById("registration-form");
    form.addEventListener("submit", (e) => {
        e.preventDefault();
        const allValid = [
            ...emailHints,
            ...passwordHints
        ].every(hint => hint.dataset.valid === "true");

        if (allValid) {
            alert("Registration successful!");
        } else {
            alert("Please fix the errors before submitting.");
        }
    });
});
