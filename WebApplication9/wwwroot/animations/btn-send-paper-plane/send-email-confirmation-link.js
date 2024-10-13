$(document).ready(function () {
    document.querySelectorAll(".button").forEach((button) => {
        let getVar = (variable) =>
            getComputedStyle(button).getPropertyValue(variable);

        button.addEventListener("click", (e) => {
            let btnText = $(button).find(".default");
            let btnTextUnchanged = btnText.html();

            $(button).prop('disabled', true);
            btnText.fadeOut("fast", function () {
                $(this)
                    .html('<i class="fas fa-spinner fa-spin no-tick"></i>')
                    .fadeIn("slow");
            });

            setTimeout(function () {
                $.ajax({
                    url: sendEmailConfirmationLinkUrl,
                    type: "POST",
                    headers: {
                        'RequestVerificationToken': $("input[name='__RequestVerificationToken']").val()
                    },
                    success: function (response) {
                        if (response.success === true) {
                            setTimeout(function () {
                                if (!button.classList.contains("active")) {
                                    button.classList.add("active");

                                    gsap.to(button, {
                                        keyframes: [
                                            {
                                                "--left-wing-first-x": 50,
                                                "--left-wing-first-y": 100,
                                                "--right-wing-second-x": 50,
                                                "--right-wing-second-y": 100,
                                                duration: 0.2,
                                                onComplete() {
                                                    gsap.set(button, {
                                                        "--left-wing-first-y": 0,
                                                        "--left-wing-second-x": 40,
                                                        "--left-wing-second-y": 100,
                                                        "--left-wing-third-x": 0,
                                                        "--left-wing-third-y": 100,
                                                        "--left-body-third-x": 40,
                                                        "--right-wing-first-x": 50,
                                                        "--right-wing-first-y": 0,
                                                        "--right-wing-second-x": 60,
                                                        "--right-wing-second-y": 100,
                                                        "--right-wing-third-x": 100,
                                                        "--right-wing-third-y": 100,
                                                        "--right-body-third-x": 60,
                                                    });
                                                },
                                            },
                                            {
                                                "--left-wing-third-x": 20,
                                                "--left-wing-third-y": 90,
                                                "--left-wing-second-y": 90,
                                                "--left-body-third-y": 90,
                                                "--right-wing-third-x": 80,
                                                "--right-wing-third-y": 90,
                                                "--right-body-third-y": 90,
                                                "--right-wing-second-y": 90,
                                                duration: 0.2,
                                            },
                                            {
                                                "--rotate": 50,
                                                "--left-wing-third-y": 95,
                                                "--left-wing-third-x": 27,
                                                "--right-body-third-x": 45,
                                                "--right-wing-second-x": 45,
                                                "--right-wing-third-x": 60,
                                                "--right-wing-third-y": 83,
                                                duration: 0.25,
                                            },
                                            {
                                                "--rotate": 55,
                                                "--plane-x": -8,
                                                "--plane-y": 24,
                                                duration: 0.2,
                                            },
                                            {
                                                "--rotate": 40,
                                                "--plane-x": 45,
                                                "--plane-y": -180,
                                                "--plane-opacity": 0,
                                                duration: 0.3,
                                                onComplete() {
                                                    setTimeout(() => {
                                                        button.removeAttribute('style');
                                                        $(button).find('span.default').html(btnTextUnchanged);
                                                        $(button).prop('disabled', false);
                                                        gsap.fromTo(button, {
                                                            opacity: 0,
                                                            y: -8
                                                        }, {
                                                            opacity: 1,
                                                            y: 0,
                                                            clearProps: true,
                                                            duration: .3,
                                                            onComplete() {
                                                                button.classList.remove('active');

                                                            }
                                                        })
                                                    }, 3000)
                                                }
                                            },
                                        ],
                                    });
                                    gsap.to(button, {
                                        keyframes: [
                                            {
                                                "--text-opacity": 0,
                                                "--border-radius": 0,
                                                "--left-wing-background": getVar("--primary-darkest"),
                                                "--right-wing-background": getVar("--primary-darkest"),
                                                duration: 0.1,
                                            },
                                            {
                                                "--left-wing-background": getVar("--primary"),
                                                "--right-wing-background": getVar("--primary"),
                                                duration: 0.1,
                                            },
                                            {
                                                "--left-body-background": getVar("--primary-dark"),
                                                "--right-body-background": getVar("--primary-darkest"),
                                                duration: 0.4,
                                            },
                                            {
                                                "--success-opacity": 1,
                                                "--success-scale": 1,
                                                duration: 0.25,
                                                delay: 0.25,
                                            },
                                        ],
                                    });
                                }
                            }, 600);
                        } else {
                            if (response.redirectUrl) window.location.href = response.redirectUrl;
                        }
                    },
                    error: function (response) {
                        btnText.fadeOut("fast", function () {
                            $(this)
                                .html('<i class="fas fa-times text-danger no-tick"></i>')
                                .fadeIn("slow");
                            toastr['error']('Molimo osvežite stranicu i pokušajte ponovo ili kontaktirajte korisničku podršku.', 'Došlo je do greške')
                        })
                        .promise()
                        .done(function () {
                            setTimeout(function () {
                                btnText.fadeOut('fast', function () {
                                    $(this).html(btnTextUnchanged).fadeIn('slow');
                                    $(button).prop('disabled', false);
                                })
                            }, 1300)
                        })
                    },
                })
            }, 600)
        })
    });
})
