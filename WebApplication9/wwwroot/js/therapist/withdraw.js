$(document).ready(function () {
    let submit = $('#btn-submit');
    let submitText = $('#submit-text');
    let submitTextUnchanged = submitText.html();

    submit.click(function () {
        DisableInputs();
        submitText.fadeOut('fast', function () {
            $(this).html('<i class="fas fa-spinner fa-spin no-tick"></i>').fadeIn('slow');
        })

        setTimeout(function () {
            $.ajax({
                url: withdrawUrl,
                type: 'POST',
                headers: {
                    'RequestVerificationToken': $("input[name='__RequestVerificationToken']").val()
                },
                success: function (response) {
                    if (response.success === true) {
                        const withdrawalSuccessfulModal = $('#withdrawal-successful-modal');
                        withdrawalSuccessfulModal.modal();

                        withdrawalSuccessfulModal.on('shown.bs.modal', function () {
                            setTimeout(function () {
                                submitText.fadeOut('fast', function () {
                                    $(this).html('<i class="ca-lightgreen fas fa-check no-tick"></i>').fadeIn('slow', function () {
                                        withdrawalSuccessfulModal.fadeOut('fast', function () {
                                            $(this).remove();
                                        });
                                        Swal.fire({
                                            title: response.title,
                                            text: response.body,
                                            icon: response.severity,
                                            confirmButtonColor: '#00d27a',
                                            confirmButtonText: 'Pogledajte detalje'
                                        }).then(() => {
                                            window.location.href = response.redirectUrl;
                                        });

                                    });
                                })
                            }, 5000);
                        })
                    } else {
                        if (response.redirectUrl) window.location.href = response.redirectUrl;
                        else HandleError(response.title, response.body, response.severity);
                    }
                },
                error: function () {
                    HandleError('Došlo je do greške', 'Molimo pokušajte ponovo ili kontaktirajte podršku za terapeute.', 'error');
                }
            })
        }, 1200);
    });

    function EnableInputs() {
        submit.prop('disabled', false);
    }

    function DisableInputs() {
        submit.prop('disabled', true);
    }

    function HandleError(title, body, severity) {
        submitText.fadeOut('fast', function () {
            $(this).html('<i class="text-danger fas fa-times no-tick"></i>').fadeIn('slow');
            toastr[severity](body, title);
        })
        .promise()
        .done(function () {
            setTimeout(function () {
                submitText.fadeOut('fast', function () {
                    $(this).html(submitTextUnchanged).fadeIn('slow');
                    EnableInputs();
                })
            }, 1300)
        })
    }
});