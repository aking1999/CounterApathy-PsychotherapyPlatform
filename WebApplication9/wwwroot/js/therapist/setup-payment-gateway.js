$(document).ready(function () {
    const btnSetupPaymentGateway = $('#setup-payment-gateway');
    const btnSetupPaymentGatewayText = $('#setup-payment-gateway-text');
    const btnSetupPaymentGatewayTextUnchanged = btnSetupPaymentGatewayText.html();

    btnSetupPaymentGateway.click(function () {
        DisableInputs();
        let width = $(this).width();
        let height = $(this).height();
        btnSetupPaymentGatewayText.fadeOut('fast', function () {
            $(this).html('<i class="fas fa-spinner fa-spin no-tick"></i>').fadeIn('slow');
            btnSetupPaymentGateway.width(width);
            btnSetupPaymentGateway.height(height);
        })

        setTimeout(function () {
            $.ajax({
                url: stripeAccountSetupUrl,
                type: 'POST',
                headers: {
                    'RequestVerificationToken': $("input[name='__RequestVerificationToken']").val()
                },
                success: function (response) {
                    if (response.success) {
                        btnSetupPaymentGatewayText.fadeOut('fast', function () {
                            $(this).html('<i class="ca-lightgreen fas fa-check no-tick"></i>').fadeIn('slow', function () {
                                setTimeout(function () {
                                    window.location.href = response.location;
                                });
                            });
                        });
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
    })

    function EnableInputs() {
        btnSetupPaymentGateway.prop('disabled', false);
    }

    function DisableInputs() {
        btnSetupPaymentGateway.prop('disabled', true);
    }

    function HandleError(title, body, severity) {
        btnSetupPaymentGatewayText.fadeOut('fast', function () {
            $(this).html('<i class="text-danger fas fa-times no-tick"></i>').fadeIn('slow');
            toastr[severity](body, title)
        })
            .promise()
            .done(function () {
                setTimeout(function () {
                    btnSetupPaymentGatewayText.fadeOut('fast', function () {
                        $(this).html(btnSetupPaymentGatewayTextUnchanged).fadeIn('slow');
                        EnableInputs();
                    });
                }, 1300)
            });
    }
});