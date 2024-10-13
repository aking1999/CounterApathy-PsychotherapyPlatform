$(document).ready(async function () {
    const slidePage = document.querySelector(".slide-page");
    const progressText = document.querySelectorAll(".step p");
    const progressCheck = document.querySelectorAll(".step .check");
    const bullet = document.querySelectorAll(".step .bullet");
    let current = 1;

    const firstSlide = $('#first-slide');
    const firstSlideInnerClone = $('#first-slide-inner').clone();

    //let paypal = $("#pay-with-paypal");
    //const paypalContentUnchanged = '<span>' + paypal.find('span').html() + '</span>';

    let cards = $('#pay-with-cards');
    const cardsContentUnchanged = '<span>' + cards.find('span').html() + '</span>';

    let usdc = $('#pay-with-usdc');
    let usdt = $('#pay-with-usdt');

    const dynamiclSlide = $('#dynamic-slide');
    //const paypalSlideInnerClone = $('#paypal-slide-inner').clone();
    const cardsSlideInnerClone = $('#cards-slide-inner').clone();
    const addWebCreditForm = $('#add-web-credit-form');

    //paypal.click(PayPalClickEvents);
    cards.click(CardsClickEvents);

    function ElementIsValid(element) {
        return addWebCreditForm.validate().element(element);
    }

    function FormIsValid() {
        const amountValid = ElementIsValid($('.amount'));

        return amountValid;
    }

    //function PayPalClickEvents() {
    //    const height = paypal.height();
    //    const btnContent = paypal.find('span');
    //    DisablePaymentButtons();

    //    btnContent.fadeOut("fast", function () {
    //        $(this).html('<i class="fas fa-spinner fa-spin text-primary no-tick"></i>').fadeIn("slow");
    //        paypal.height(height)
    //    });

    //    setTimeout(function () {
    //        btnContent.fadeOut('fast', function () {
    //            $(this).html('<i class="fas fa-check text-success"></i>').fadeIn('slow', function () {
    //                setTimeout(function () {
    //                    RemoveFirstSlide();
    //                    MakePayPalSlide();
    //                    $('#paypal-slide-inner').removeClass('d-none');
    //                    slidePage.style.marginLeft = "-25%";
    //                    try {
    //                        bullet[current - 1].classList.add("active");
    //                        progressCheck[current - 1].classList.add("active");
    //                        progressText[current - 1].classList.add("active");
    //                    } catch (error) {

    //                    }
    //                    current += 1;
    //                }, 1000)
    //            })
    //        })
    //    }, 600);
    //}

    function CardsClickEvents() {
        const height = cards.height();
        const btnContent = cards.find('span');

        DisablePaymentButtons();

        btnContent.fadeOut("fast", function () {
            $(this).html('<i class="fas fa-spinner fa-spin text-primary no-tick"></i>').fadeIn("slow");
            cards.height(height)
        });

        setTimeout(function () {
            btnContent.fadeOut('fast', function () {
                $(this).html('<i class="fas fa-check text-success"></i>').fadeIn('slow', function () {
                    setTimeout(function () {
                        RemoveFirstSlide();
                        MakeCardsSlide();
                        $('#cards-slide-inner').removeClass('d-none');
                        slidePage.style.marginLeft = "-25%";
                        try {
                            bullet[current - 1].classList.add("active");
                            progressCheck[current - 1].classList.add("active");
                            progressText[current - 1].classList.add("active");
                        } catch (error) {

                        }
                        current += 1;
                    }, 1000)
                })
            })
        }, 600);
    }

    function RemoveFirstSlide() {
        $('#first-slide-inner').remove();
    }

    function MakeFirstSlide() {
        firstSlide.html(firstSlideInnerClone);
        //paypal = firstSlide.find('#pay-with-paypal');
        cards = firstSlide.find('#pay-with-cards');

        //paypal.click(PayPalClickEvents);
        cards.click(CardsClickEvents);
    }

    //function MakePayPalSlide() {
    //    dynamiclSlide.html(paypalSlideInnerClone);

    //    const payPalAmount = $('#PayPal_Amount');
    //    payPalAmount.keyup(function () {
    //        $.ajax({
    //            url: urls.exchangeRate,
    //            type: 'GET',
    //            dataType: 'json',
    //            contentType: "application/json",
    //            success: function (response) {
    //                addWebCreditForm.find('.input-group-append .input-group-text').html('~' + ((payPalAmount.val() * (1 + (paypalSettings.feePercentage / 100))) / response.exchangeRate).toFixed(2) + "<i class='fal fa-dollar-sign fa-sm text-success ml-1'></i>");
    //            },
    //            error: function (response) {
    //                addWebCreditForm.find('.input-group-append .input-group-text').html('~' + ((payPalAmount.val() * (1 + (paypalSettings.feePercentage / 100))) / 118).toFixed(2) + "<i class='fal fa-dollar-sign fa-sm text-success ml-1'></i>");
    //            }
    //        })
    //    })

    //    const next = $(".paypal-next-1");
    //    next.click(function (event) {
    //        const nextText = $(this).find('#paypal-btnNext1Text');
    //        const nextTextUnchanged = nextText.html();

    //        event.stopPropagation();
    //        DisablePaymentButtons();
    //        $(this).prop('disabled', true);
    //        payPalAmount.prop('readonly', true);

    //        nextText.fadeOut('fast', function () {
    //            $(this).html('<i class="fas fa-spinner fa-spin no-tick"></i>').fadeIn('slow');
    //        });

    //        setTimeout(function () {
    //            if (FormIsValid()) {
    //                let formData = new FormData(addWebCreditForm.get(0));
    //                formData.append('webCreditVm.Amount', payPalAmount.val());

    //                $.ajax({
    //                    url: urls.executeOrder,
    //                    type: 'POST',
    //                    data: formData,
    //                    processData: false,
    //                    contentType: false,
    //                    headers: {
    //                        'RequestVerificationToken': $("input[name='__RequestVerificationToken']").val()
    //                    },
    //                    success: function (response) {
    //                        if (response.success === true) {
    //                            nextText.fadeOut('fast', function () {
    //                                try {
    //                                    bullet[current - 1].classList.add("active");
    //                                    progressCheck[current - 1].classList.add("active");
    //                                    progressText[current - 1].classList.add("active");
    //                                } catch (error) {

    //                                }
    //                                current += 1;
    //                                $(this).html('<i class="ca-lightgreen fas fa-check no-tick"></i>').fadeIn('slow', function () {
    //                                    window.location.href = response.approveUrl
    //                                })
    //                            })
    //                        } else {
    //                            if (response.redirectUrl) window.location.href = response.redirectUrl;
    //                            else {
    //                                nextText.fadeOut('fast', function () {
    //                                    $(this).html('<i class="fas fa-times text-danger no-tick"></i>').fadeIn('slow');
    //                                    toastr[response.severity](response.body, response.title)
    //                                })
    //                                .promise()
    //                                .done(function () {
    //                                    setTimeout(function () {
    //                                        nextText.fadeOut('fast', function () {
    //                                            $(this).html(nextTextUnchanged).fadeIn('slow');
    //                                            next.prop('disabled', false);
    //                                            payPalAmount.prop('readonly', false);
    //                                        })
    //                                    }, 1300)
    //                                });
    //                            }
    //                        }
    //                    },
    //                    error: function (response) {
    //                        nextText.fadeOut('fast', function () {
    //                            $(this).html('<i class="fas fa-times text-danger no-tick"></i>').fadeIn('slow');
    //                            toastr['error']('Molimo pokušajte ponovo ili kontaktirajte korisničku podršku.', 'Došlo je do greške')
    //                        })
    //                        .promise()
    //                        .done(function () {
    //                            setTimeout(function () {
    //                                nextText.fadeOut('fast', function () {
    //                                    $(this).html(nextTextUnchanged).fadeIn('slow');
    //                                    next.prop('disabled', false);
    //                                    payPalAmount.prop('readonly', false);
    //                                    EnablePaymentButtons();
    //                                })
    //                            }, 1300)
    //                        });
    //                    }
    //                })
    //            } else {
    //                nextText.fadeOut('fast', function () {
    //                    $(this).html('<i class="fas fa-times text-danger no-tick"></i>').fadeIn('slow');
    //                    toastr['info'](null, 'Popunite sva obavezna polja')
    //                })
    //                .promise()
    //                .done(function () {
    //                    setTimeout(function () {
    //                        nextText.fadeOut('fast', function () {
    //                            $(this).html(nextTextUnchanged).fadeIn('slow');
    //                            next.prop('disabled', false);
    //                            payPalAmount.prop('readonly', false);
    //                        })
    //                    }, 1300)
    //                });
    //            }
    //        }, 1200)
    //    })
    //    document.querySelector(".paypal-prev-1").addEventListener("click", function () {
    //        RemoveDynamicSlide();
    //        MakeFirstSlide();
    //        $('#paypal-slide-inner').addClass('d-none');
    //        slidePage.style.marginLeft = "0%";
    //        try {
    //            bullet[current - 2].classList.remove("active");
    //            progressCheck[current - 2].classList.remove("active");
    //            progressText[current - 2].classList.remove("active");
    //            current -= 1;
    //        } catch (error) {

    //        }
    //        EnablePaymentButtons();
    //        paypal.html(paypalContentUnchanged);
    //    });

    //    return true;
    //}

    async function MakeCardsSlide() {
        dynamiclSlide.html(cardsSlideInnerClone);

        const cardsAmount = $('#Cards_Amount');
        const next = $(".cards-next-1");
        next.click(async function (event) {
            const nextText = $(this).find('#cards-btnNext1Text');
            const nextTextUnchanged = nextText.html();

            event.stopPropagation();
            DisablePaymentButtons();
            $(this).prop('disabled', true);
            cardsAmount.prop('readonly', true);

            nextText.fadeOut('fast', function () {
                $(this).html('<i class="fas fa-spinner fa-spin no-tick"></i>').fadeIn('slow');
            });

            setTimeout(async function () {
                if (FormIsValid()) {
                    let formData = new FormData(addWebCreditForm.get(0));
                    formData.append('cardVm.Amount', cardsAmount.val());

                    $.ajax({
                        url: urls.cardsPayment,
                        type: 'POST',
                        data: formData,
                        processData: false,
                        contentType: false,
                        headers: {
                            'RequestVerificationToken': $("input[name='__RequestVerificationToken']").val()
                        },
                        success: async function (response) {
                            if (response.success === true) {
                                nextText.fadeOut('fast', async function () {
                                    $(this).html('<i class="ca-lightgreen fas fa-check no-tick"></i>').fadeIn('slow', async function () {
                                        await RenderPaymentModal(
                                            response.publicKey,
                                            response.clientSecret,
                                            response.amount,
                                            response.currency,
                                            response.paymentSuccessfulReturnUrl,
                                            response.title,
                                            response.body,
                                            response.severity
                                        );
                                        try {
                                            bullet[current - 1].classList.add("active");
                                            progressCheck[current - 1].classList.add("active");
                                            progressText[current - 1].classList.add("active");
                                        } catch (error) {

                                        }
                                        current += 1;
                                        setTimeout(function () {
                                            nextText.fadeOut('fast', function () {
                                                $(this).html(nextTextUnchanged).fadeIn('slow');
                                                next.prop('disabled', false);
                                                cardsAmount.prop('readonly', false);
                                            })
                                        }, 1200)
                                    })
                                })
                            } else {
                                if (response.redirectUrl) window.location.href = response.redirectUrl;
                                else {
                                    nextText.fadeOut('fast', function () {
                                        $(this).html('<i class="fas fa-times text-danger no-tick"></i>').fadeIn('slow');
                                        toastr[response.severity](response.body, response.title)
                                    }).promise()
                                        .done(function () {
                                            setTimeout(function () {
                                                nextText.fadeOut('fast', function () {
                                                    $(this).html(nextTextUnchanged).fadeIn('slow');
                                                    next.prop('disabled', false);
                                                    cardsAmount.prop('readonly', false);
                                                })
                                            }, 1300)
                                        });
                                }
                            }
                        },
                        error: function () {
                            nextText.fadeOut('fast', function () {
                                $(this).html('<i class="fas fa-times text-danger no-tick"></i>').fadeIn('slow');
                                toastr['error']('Molimo pokušajte ponovo ili kontaktirajte korisničku podršku.', 'Došlo je do greške')
                            }).promise()
                                .done(function () {
                                    setTimeout(function () {
                                        nextText.fadeOut('fast', function () {
                                            $(this).html(nextTextUnchanged).fadeIn('slow');
                                            next.prop('disabled', false);
                                            cardsAmount.prop('readonly', false);
                                            EnablePaymentButtons();
                                        })
                                    }, 1300)
                                })
                        }
                    })
                } else {
                    nextText.fadeOut('fast', function () {
                        $(this).html('<i class="fas fa-times text-danger no-tick"></i>').fadeIn('slow');
                        toastr['info'](null, 'Popunite sva obavezna polja')
                    }).promise()
                        .done(function () {
                            setTimeout(function () {
                                nextText.fadeOut('fast', function () {
                                    $(this).html(nextTextUnchanged).fadeIn('slow');
                                    next.prop('disabled', false);
                                    cardsAmount.prop('readonly', false);
                                })
                            }, 1300)
                        });
                }
            }, 1200);
        })
        document.querySelector(".cards-prev-1").addEventListener("click", function () {
            RemoveDynamicSlide();
            MakeFirstSlide();
            $('#cards-slide-inner').addClass('d-none');
            slidePage.style.marginLeft = "0%";
            try {
                bullet[current - 2].classList.remove("active");
                progressCheck[current - 2].classList.remove("active");
                progressText[current - 2].classList.remove("active");
                current -= 1;
            } catch (error) {

            }
            EnablePaymentButtons();
            cards.html(cardsContentUnchanged);
        });

        return true;
    }

    function RemoveDynamicSlide() {
        dynamiclSlide.html('');
    }

    function EnablePaymentButtons() {
        //paypal.prop('disabled', false);
        cards.prop('disabled', false);
        usdc.prop('disabled', false);
        usdt.prop('disabled', false);
    }

    function DisablePaymentButtons() {
        //paypal.prop('disabled', true);
        cards.prop('disabled', true);
        usdc.prop('disabled', true);
        usdt.prop('disabled', true);
    }

    async function RenderPaymentModal(publicKey, clientSecret, amount, currency, paymentSuccessfulReturnUrl, title, body, severity) {
        let modal = '<div id="add-web-credit-payment-modal" data-backdrop="static" data-keyboard="false" data-toggle="modal" class="modal fade p-0" tabindex="-1" role="dialog" aria-labelledby="ModalLabel" aria-hidden="true">' +
            '<div class="modal-dialog modal-dialog-centered" role="document">' +
            '<div class="modal-content body-color">' +
            '<div class="modal-header border-bottom-0 pb-0">' +
            '<h5 class="modal-title text-center mx-auto">Unesite podatke za uplatu</h5>' +
            '<button type="button" id="dismiss-payment-modal" class="close m-0 p-0" data-dismiss="modal" aria-label="Zatvori" disabled>' +
            '<span aria-hidden="true">&times;</span>' +
            '</button>' +
            '</div>' +
            '<div id="modal-body" class="modal-body text-center">' +
            '<form id="payment-form">' +
            '<div id="payment-element">' +
            '<div id="loader" class="row no-gutters placeholder-wave">' +
            '<div class="col-7 placeholder rounded-lg my-3"><h2 class="py-3 mb-1"></h2></div>' +
            '<div class="col-5"></div>' +
            '<div class="col-12 placeholder rounded-lg my-3"><h2 class="py-3 mb-1"></h2></div>' +
            '<div class="col-6 placeholder rounded-lg my-3"><h2 class="py-3 mb-1"></h2></div>' +
            '<div class="col-5 placeholder rounded-lg my-3 ml-auto"><h2 class="py-3 mb-1"></h2></div>' +
            '<div class="col-12 placeholder rounded-lg my-3"><h2 class="py-3 mb-1"></h2></div>' +
            '<div class="col-12 placeholder rounded-lg my-3"><h2 class="py-3 mb-1"></h2></div>' +
            '<button class="btn btn-primary col-12 placeholder disabled mb-1"></button>' +
            '</div>' +
            '</div>' +
            '<div class="alert alert-danger text-center mt-2 d-none" role="alert" id="error-messages"></div>' + 
            '<div id="pay-button-holder" class="d-none mt-3"><button id="pay-button" type="button" class="btn btn-primary btn-block">' +
            `<span id="pay-button-text">&nbsp;<i class="fas fa-lock mr-2"></i>Platite ${amount} ${currency}&nbsp;</span>` +
            '</button></div>' +
            '</form>' +
            '</div>' +
            '</div>' +
            '</div>';

        let createdModal = $($.parseHTML(modal));
        const stripe = Stripe(publicKey);
        const elements = stripe.elements({ clientSecret });
        const paymentElement = elements.create('payment');

        $(createdModal).modal();

        $(createdModal).on('hidden.bs.modal', function () {
            $(createdModal).remove();
        });

        setTimeout(function () {
            const paymentElementHolder = $('#payment-element');
            const paymentElementHolderWidth = paymentElementHolder.width();
            const paymentElementHolderHeight = paymentElementHolder.height();
            paymentElement.mount('#payment-element');
            paymentElementHolder.width(paymentElementHolderWidth);
            paymentElementHolder.height(paymentElementHolderHeight);

            paymentElement.on('ready', function () {
                paymentElementHolder.css('width', 'auto');
                paymentElementHolder.css('height', 'auto');
                $('#pay-button-holder').removeClass('d-none');
                const payButton = $('#pay-button');
                const dismissPaymentModal = $('#dismiss-payment-modal');
                const payButtonText = $('#pay-button-text');
                const payButtonTextUnchanged = payButtonText.html();

                dismissPaymentModal.prop('disabled', false);
                dismissPaymentModal.click(function () {
                    EnableInputs();
                    submit.html(submitTextUnchanged);
                    bookModal.modal('show');
                })

                payButton.click(async function () {
                    payButton.prop('disabled', true);
                    dismissPaymentModal.prop('disabled', true);

                    payButtonText.fadeOut('fast', async function () {
                        $(this).html('<i class="fas fa-spinner fa-spin no-tick"></i>').fadeIn('slow');
                    })
                    const { paymentIntent, error } = await stripe.confirmPayment({
                        elements,
                        confirmParams: {
                            return_url: paymentSuccessfulReturnUrl
                        },
                        redirect: "if_required"
                    })

                    setTimeout(function () {
                        if (error) {
                            dismissPaymentModal.prop('disabled', false);
                            payButtonText.fadeOut(function () {
                                $(this).html('<i class="text-danger fas fa-times no-tick"></i>').fadeIn('slow');
                                $('#error-messages').text(error.message)
                                $('#error-messages').removeClass('d-none');
                            }).promise()
                                .done(function () {
                                    setTimeout(function () {
                                        payButtonText.fadeOut('fast', function () {
                                            $(this).html(payButtonTextUnchanged).fadeIn('slow');
                                            payButton.prop('disabled', false);
                                            dismissPaymentModal.prop('disabled', false);
                                        })
                                    }, 1300)
                                })
                        } else if (paymentIntent && paymentIntent.status === 'succeeded') {
                            payButtonText.fadeOut('fast', function () {
                                $(this).html('<i class="ca-lightgreen fas fa-check no-tick"></i>').fadeIn('slow', function () {
                                    setTimeout(function () {
                                        $(createdModal).modal('hide');
                                        $(createdModal).remove();
                                        Swal.fire({
                                            title: title,
                                            text: body,
                                            icon: severity,
                                            confirmButtonText: 'Pogledajte detalje',
                                        }).then(() => {
                                            window.location.href = paymentSuccessfulReturnUrl;
                                        })
                                    }, 600)
                                });
                            })
                        } else {
                            Swal.mixin({
                                customClass: {
                                    confirmButton: 'btn btn-sm btn-primary mr-2',
                                    cancelButton: 'btn btn-sm btn-falcon-secondary'
                                },
                                buttonsStyling: false
                            }).fire({
                                title: 'Došlo je do greške prilikom plaćanja',
                                text: 'Vaš račun nije zadužen. Osvežite stranicu i pokušajte ponovo ili kontaktirajte korisničku podršku.',
                                icon: 'error',
                                showCancelButton: true,
                                confirmButtonText: 'Korisnička podrška',
                                cancelButtonText: 'Kasnije ću'
                            }).then((result) => {
                                if (result.isConfirmed) {
                                    window.location.href = '@Url.Action("CustomerSupport", "Home", new { Area = "" })'
                                } else {
                                    location.reload();
                                }
                            })
                            throw new Error('Unhandled PaymentIntent.Status = ' + paymentIntent.status)
                        }
                    }, 1200)
                })
            })

            paymentElement.on('change', function (event) {
                const displayError = $('#error-messages');
                if (event.error) {
                    displayError.text(event.error.message)
                    displayError.removeClass('d-none');
                } else {
                    displayError.text('')
                    displayError.addClass('d-none');
                }
            });
        }, 1000);
    }
})