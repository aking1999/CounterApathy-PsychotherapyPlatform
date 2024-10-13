$(document).ready(function () {
    const slidePage = document.querySelector(".slide-page");
    const nextBtnFirst = document.querySelector(".firstNext");
    const nextBtnFirstText = $(nextBtnFirst).find("span");

    const prevBtnSec = document.querySelector(".prev-1");
    const nextBtnSec = document.querySelector(".next-1");
    const nextBtnSecText = $(nextBtnSec).find("span");

    const prevBtnThird = document.querySelector(".prev-2");
    const nextBtnThird = document.querySelector(".next-2");
    const nextBtnThirdText = $(nextBtnThird).find("span");

    const prevBtnFourth = document.querySelector(".prev-3");
    const submitBtn = document.querySelector(".submit");
    const submitBtnText = $(submitBtn).find("span");
    const submitTextUnchanged = submitBtnText.html();

    const progressText = document.querySelectorAll(".step p");
    const progressCheck = document.querySelectorAll(".step .check");
    const bullet = document.querySelectorAll(".step .bullet");
    let current = 1;

    const firstName = document.getElementById("FirstName");
    const lastName = document.getElementById("LastName");
    const email = document.getElementById("Email");
    const phoneNumber = document.getElementById("PhoneNumber");
    const password = document.getElementById("Password");
    const confirmPassword = document.getElementById("ConfirmPassword");
    const terms = document.getElementById("TermsOfService");
    const privacy = document.getElementById("PrivacyPolicy");

    const registerForm = $("#register-form");

    DisableAllButtonsExcept([nextBtnFirst]);
    MakeAllInputsReadOnlyExcept([firstName, lastName]);

    nextBtnFirst.addEventListener("click", function (event) {
        event.preventDefault();

        DisableAllButtonsExcept([]);
        MakeAllInputsReadOnlyExcept([]);

        nextBtnFirstText.fadeOut("fast", function () {
            $(this).html('<i class="fas fa-spinner fa-spin no-tick"></i>').fadeIn("slow");
        });

        setTimeout(function () {
            let firstNameIsValid = ElementIsValid(firstName);
            let lastNameIsValid = ElementIsValid(lastName);
            if (firstNameIsValid && lastNameIsValid) {
                nextBtnFirstText.fadeOut("fast", function () {
                    $(this)
                        .html('<i class="ca-lightgreen fas fa-check"></i>')
                        .fadeIn(function () {
                            setTimeout(function () {
                                slidePage.style.marginLeft = "-25%";
                                bullet[current - 1].classList.add("active");
                                progressCheck[current - 1].classList.add("active");
                                progressText[current - 1].classList.add("active");
                                current += 1;
                                nextBtnFirstText.html("Nastavi");
                                DisableAllButtonsExcept([nextBtnSec, prevBtnSec]);
                                MakeAllInputsReadOnlyExcept([email, phoneNumber]);
                            }, 1000);
                        });
                });
            } else {
                nextBtnFirstText
                    .fadeOut("fast", function () {
                        $(this)
                            .html('<i class="text-danger fas fa-times"></i>')
                            .fadeIn("slow");
                        toastr["info"](null, "Popunite sva obavezna polja");
                    })
                    .promise()
                    .done(function () {
                        setTimeout(function () {
                            nextBtnFirstText.fadeOut("fast", function () {
                                $(this).html("Nastavi").fadeIn("slow");
                                DisableAllButtonsExcept([nextBtnFirst]);
                                MakeAllInputsReadOnlyExcept([firstName, lastName]);
                            });
                        }, 600);
                    });
            }
        }, 600);
    });
    nextBtnSec.addEventListener("click", function (event) {
        event.preventDefault();

        DisableAllButtonsExcept([]);
        MakeAllInputsReadOnlyExcept([]);

        nextBtnSecText.fadeOut("fast", function () {
            $(this).html('<i class="fas fa-spinner fa-spin no-tick"></i>').fadeIn("slow");
        });

        setTimeout(() => {
            let emailIsValid = ElementIsValid(email);
            let phoneNumberIsValid = ElementIsValid(phoneNumber);
            if (emailIsValid && phoneNumberIsValid) {
                nextBtnSecText.fadeOut("fast", function () {
                    $(this)
                        .html('<i class="ca-lightgreen fas fa-check"></i>')
                        .fadeIn(function () {
                            setTimeout(function () {
                                slidePage.style.marginLeft = "-50%";
                                bullet[current - 1].classList.add("active");
                                progressCheck[current - 1].classList.add("active");
                                progressText[current - 1].classList.add("active");
                                current += 1;
                                nextBtnSecText.html("Nastavi");
                                DisableAllButtonsExcept([nextBtnThird, prevBtnThird]);
                                MakeAllInputsReadOnlyExcept([password, confirmPassword]);
                            }, 1000);
                        });
                });
            } else {
                nextBtnSecText
                    .fadeOut("fast", function () {
                        $(this)
                            .html('<i class="text-danger fas fa-times"></i>')
                            .fadeIn("slow");
                        toastr["info"](null, "Popunite sva obavezna polja");
                    })
                    .promise()
                    .done(function () {
                        setTimeout(function () {
                            nextBtnSecText.fadeOut("fast", function () {
                                $(this).html("Nastavi").fadeIn("slow");
                                DisableAllButtonsExcept([nextBtnSec, prevBtnSec]);
                                MakeAllInputsReadOnlyExcept([email, phoneNumber]);
                            });
                        }, 600);
                    });
            }
        }, 600);
    });
    nextBtnThird.addEventListener("click", function (event) {
        event.preventDefault();

        DisableAllButtonsExcept([]);
        MakeAllInputsReadOnlyExcept([]);

        nextBtnThirdText.fadeOut("fast", function () {
            $(this).html('<i class="fas fa-spinner fa-spin no-tick"></i>').fadeIn("slow");
        });

        setTimeout(() => {
            let passwordIsValid = ElementIsValid(password);
            let confirmPasswordIsValid = ElementIsValid(confirmPassword);
            if (passwordIsValid && confirmPasswordIsValid) {
                nextBtnThirdText.fadeOut("fast", function () {
                    $(this)
                        .html('<i class="ca-lightgreen fas fa-check"></i>')
                        .fadeIn(function () {
                            setTimeout(function () {
                                slidePage.style.marginLeft = "-75%";
                                bullet[current - 1].classList.add("active");
                                progressCheck[current - 1].classList.add("active");
                                progressText[current - 1].classList.add("active");
                                current += 1;
                                nextBtnThirdText.html("Nastavi");
                                DisableAllButtonsExcept([submitBtn, prevBtnFourth]);
                            }, 1000);
                        });
                });
            } else {
                nextBtnThirdText
                    .fadeOut("fast", function () {
                        $(this)
                            .html('<i class="text-danger fas fa-times"></i>')
                            .fadeIn("slow");
                        toastr["info"](null, "Popunite sva obavezna polja");
                    })
                    .promise()
                    .done(function () {
                        setTimeout(function () {
                            nextBtnThirdText.fadeOut("fast", function () {
                                $(this).html("Nastavi").fadeIn("slow");
                                DisableAllButtonsExcept([nextBtnThird, prevBtnThird]);
                                MakeAllInputsReadOnlyExcept([password, confirmPassword]);
                            });
                        }, 600);
                    });
            }
        }, 600);
    });
    submitBtn.addEventListener("click", function (event) {
        event.preventDefault();

        DisableAllButtonsExcept([]);
        MakeAllInputsReadOnlyExcept([]);

        submitBtnText.fadeOut("fast", function () {
            $(this).html('<i class="fas fa-spinner fa-spin no-tick"></i>').fadeIn("slow");
        });

        setTimeout(() => {
            let termsOfServiceIsValid = ElementIsValid(terms);
            let privacyPolicyIsValid = ElementIsValid(privacy);
            if (termsOfServiceIsValid && privacyPolicyIsValid) {
                submitBtnText.fadeOut("fast", function () {
                    $(this)
                        .html('<i class="ca-lightgreen fas fa-check"></i>')
                        .fadeIn('slow', function () {
                            bullet[current - 1].classList.add("active");
                            progressCheck[current - 1].classList.add("active");
                            progressText[current - 1].classList.add("active");
                            //current += 1;
                            //current -= 1;
                            setTimeout(function () {
                                if (registerForm.valid()) {
                                    setTimeout(function () {
                                        registerForm.submit();
                                    }, 600);
                                } else {
                                    DisableAllButtonsExcept([submitBtn, prevBtnFourth]);
                                    submitBtnText.html(submitTextUnchanged);
                                    toastr["info"](
                                        "Molimo osvežite stranicu i pokušajte ponovo ili kontaktirajte korisničku podršku.",
                                        "Došlo je do greške"
                                    );
                                    /*current -= 1;*/
                                }
                            }, 1000);
                        });
                });
            } else {
                submitBtnText
                    .fadeOut("fast", function () {
                        $(this)
                            .html('<i class="text-danger fas fa-times"></i>')
                            .fadeIn("slow");
                        toastr["info"](null, "Popunite sva obavezna polja");
                    })
                    .promise()
                    .done(function () {
                        setTimeout(function () {
                            submitBtnText.fadeOut("fast", function () {
                                $(this).html(submitTextUnchanged).fadeIn("slow");
                                DisableAllButtonsExcept([submitBtn, prevBtnFourth]);
                            });
                        }, 600);
                    });
            }
        }, 600);
    });

    prevBtnSec.addEventListener("click", function (event) {
        event.preventDefault();
        slidePage.style.marginLeft = "0%";
        bullet[current - 2].classList.remove("active");
        progressCheck[current - 2].classList.remove("active");
        progressText[current - 2].classList.remove("active");
        current -= 1;
        DisableAllButtonsExcept([nextBtnFirst]);
        MakeAllInputsReadOnlyExcept([firstName, lastName]);
    });
    prevBtnThird.addEventListener("click", function (event) {
        event.preventDefault();
        slidePage.style.marginLeft = "-25%";
        bullet[current - 2].classList.remove("active");
        progressCheck[current - 2].classList.remove("active");
        progressText[current - 2].classList.remove("active");
        current -= 1;
        DisableAllButtonsExcept([prevBtnSec, nextBtnSec]);
        MakeAllInputsReadOnlyExcept([email, phoneNumber]);
    });
    prevBtnFourth.addEventListener("click", function (event) {
        event.preventDefault();
        slidePage.style.marginLeft = "-50%";
        bullet[current - 2].classList.remove("active");
        progressCheck[current - 2].classList.remove("active");
        progressText[current - 2].classList.remove("active");
        current -= 1;
        DisableAllButtonsExcept([prevBtnThird, nextBtnThird]);
        MakeAllInputsReadOnlyExcept([password, confirmPassword]);
    });

    function ElementIsValid(element) {
        if (registerForm.validate().element($(element))) return true;
        return false;
    }

    function MakeAllInputsReadOnlyExcept(inputElements) {
        firstName.readOnly = true;
        lastName.readOnly = true;
        email.readOnly = true;
        phoneNumber.readOnly = true;
        password.readOnly = true;
        confirmPassword.readOnly = true;


        inputElements.forEach(function (element, index) {
            element.readOnly = false
        }, inputElements)
    }

    function DisableAllButtonsExcept(buttonElements) {
        nextBtnFirst.disabled = true;
        nextBtnSec.disabled = true;
        prevBtnSec.disabled = true;
        nextBtnThird.disabled = true;
        prevBtnThird.disabled = true;
        submitBtn.disabled = true;
        prevBtnFourth.disabled = true;

        buttonElements.forEach(function (element, index) {
            element.disabled = false;
        }, buttonElements);
    }
});
