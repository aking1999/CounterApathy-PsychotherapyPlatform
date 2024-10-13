$(document).ready(function () {
    const inviteLink = $('#InviteLink');
    const bookingId = $('#BookedConsultation_BookingId');
    const consultationId = $('#BookedConsultation_ConsultationId');
    const addInviteLink = $('#btn-add-invite-link');
    const addInviteLinkModal = $('#add-invite-link-modal');
    const cancel = $('#cancel');
    const btnModalDismissTimes = $('#btn-dismiss-modal-times');
    const btnLaunchModal = $('#btn-launch-modal');

    document.querySelectorAll("#btn-submit").forEach((button) => {
        let getVar = (variable) =>
            getComputedStyle(button).getPropertyValue(variable);

        button.addEventListener('click', (e) => {
            let btnText = $(button).find(".default");
            let btnTextUnchanged = btnText.html();

            $(button).prop('disabled', true);
            cancel.prop('disabled', true);
            btnModalDismissTimes.prop('disabled', true);
            inviteLink.prop('readonly', true);
            btnText.fadeOut("fast", function () {
                $(this)
                    .html('<i class="fas fa-spinner fa-spin no-tick"></i>')
                    .fadeIn("slow");
            });

            setTimeout(function () {
                if ($('#add-invite-link-form').validate().element(inviteLink)) {
                    let formData = new FormData($('#add-invite-link-form').get(0));
                    formData.append('details.InviteLink', inviteLink.val());
                    formData.append('details.BookedConsultation.BookingId', bookingId.val());
                    formData.append('details.BookedConsultation.ConsultationId', consultationId.val());

                    $.ajax({
                        url: consultationInviteLinkEndpoint,
                        type: 'POST',
                        data: formData,
                        processData: false,
                        contentType: false,
                        headers: {
                            'RequestVerificationToken': $("input[name='__RequestVerificationToken']").val()
                        },
                        success: function (response) {
                            if (response.success === true) {
                                setTimeout(function () {
                                    if (!button.classList.contains("active")) {
                                        button.classList.add("active");
                                        cancel.prop('disabled', false)
                                        btnModalDismissTimes.prop('disabled', false);

                                        $('#invite-link-notice-placeholder').fadeOut('fast', function () {
                                            $(this).html(
                                                "<i class='far fa-link mr-2'></i>" + btnLaunchModal.attr('data-contactmethodname') + " link za pristup konsultacijama:" +
                                                "<p class='text-primary mt-3 mb-4 text-break'>" + inviteLink.val() + "</p>").fadeIn('slow');
                                            btnLaunchModal.html("<i class='far fa-link mr-2'></i>Dodajte novi " + btnLaunchModal.attr('data-contactmethodname') + " link za pristup");
                                        });

                                        toastr[response.severity](response.body, response.title);

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
                                                            inviteLink.prop('readonly', false);
                                                            gsap.fromTo(button, {
                                                                opacity: 0,
                                                                y: -8
                                                            }, {
                                                                opacity: 1,
                                                                y: 0,
                                                                clearProps: true,
                                                                duration: .3,
                                                                onComplete() {
                                                                    addInviteLinkModal.modal('hide');
                                                                    button.classList.remove('active');
                                                                }
                                                            })
                                                        }, 3000);
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
                                }, 600)
                            } else {
                                if (response.redirectUrl) window.location.href = response.redirectUrl;
                                else {
                                    btnText.fadeOut("fast", function () {
                                        $(this)
                                            .html('<i class="fas fa-times text-danger no-tick"></i>')
                                            .fadeIn("slow");
                                        toastr[response.severity](response.body, response.title);
                                    })
                                        .promise()
                                        .done(function () {
                                            setTimeout(function () {
                                                btnText.fadeOut('fast', function () {
                                                    $(this).html(btnTextUnchanged).fadeIn('slow');
                                                    inviteLink.prop('readonly', false);
                                                    $(button).prop('disabled', false);
                                                    cancel.prop('disabled', false);
                                                    btnModalDismissTimes.prop('disabled', false);
                                                })
                                            }, 1300)
                                        })
                                }
                            }
                        },
                        error: function () {
                            btnText.fadeOut("fast", function () {
                                $(this)
                                    .html('<i class="fas fa-times text-danger no-tick"></i>').fadeIn("slow");
                                toastr['error']('Molimo osvežite stranicu i pokušajte ponovo ili kontaktirajte podršku za terapeute.', 'Došlo je do greške');
                            })
                                .promise()
                                .done(function () {
                                    setTimeout(function () {
                                        btnText.fadeOut('fast', function () {
                                            $(this).html(btnTextUnchanged).fadeIn('slow');
                                            inviteLink.prop('readonly', false);
                                            $(button).prop('disabled', false);
                                            cancel.prop('disabled', false);
                                            btnModalDismissTimes.prop('disabled', false);
                                        })
                                    }, 1300)
                                })
                        }
                    })
                } else {
                    btnText.fadeOut('fast', function () {
                        $(this).html('<i class="fas fa-times text-danger no-tick"></i>').fadeIn('slow');
                        toastr['info']('', 'Popunite sva obavezna polja');
                    })
                        .promise()
                        .done(function () {
                            setTimeout(function () {
                                btnText.fadeOut('fast', function () {
                                    $(this).html(btnTextUnchanged).fadeIn('slow');
                                    inviteLink.prop('readonly', false);
                                    $(button).prop('disabled', false);
                                    cancel.prop('disabled', false);
                                    btnModalDismissTimes.prop('disabled', false);
                                })
                            }, 1300)
                        });
                }
            }, 600);
        });
    });
});