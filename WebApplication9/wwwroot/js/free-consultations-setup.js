$(document).ready(function () {
    const btnTherapistFreeConsultations = $('.btn-therapist-free-consultations');

    btnTherapistFreeConsultations.click(function () {
        const targetButton = $(this);
        const targetButtonText = targetButton.find('.btn-therapist-free-consultations-text');
        const targetButtonTextUnchanged = targetButtonText.html();

        btnTherapistFreeConsultations.prop('disabled', true);
        let width = targetButton.width();
        let height = targetButton.height();
        targetButtonText.fadeOut('fast', function () {
            $(this).html('<i class="fas fa-spinner text-primary fa-spin no-tick"></i>').fadeIn('slow');
            targetButton.width(width);
            targetButton.height(height);
        });

        setTimeout(function () {
            $.ajax({
                url: getConsultationsUrl,
                type: 'POST',
                data: { therapistId: targetButton.attr('data-therapist-id') },
                success: function (response) {
                    if (response.success === true) {
                        targetButtonText.fadeOut('fast', function () {
                            $(this).html('<i class="text-success fas fa-check no-tick"></i>').fadeIn('slow', function () {
                                $('html').append(response.partialView);
                                setTimeout(function () {
                                    targetButtonText.fadeOut('fast', function () {
                                        $(this).html(targetButtonTextUnchanged).fadeIn('slow');
                                        btnTherapistFreeConsultations.prop('disabled', false);
                                    })
                                }, 600);
                            });
                        });
                    } else {
                        if (response.redirectUrl) window.location.href = response.redirectUrl;
                        else {
                            targetButtonText.fadeOut('fast', function () {
                                $(this).html('<i class="text-danger fas fa-times no-tick"></i>').fadeIn('slow');
                                toastr[response.severity](response.body, response.title);
                            })
                                .promise()
                                .done(function () {
                                    setTimeout(function () {
                                        targetButtonText.fadeOut('fast', function () {
                                            $(this).html(targetButtonTextUnchanged).fadeIn('slow');
                                            btnTherapistFreeConsultations.prop('disabled', false);
                                        });
                                    }, 1300)
                                })
                        }
                    }
                },
                error: function () {

                }
            })
        }, 600)
    })
})