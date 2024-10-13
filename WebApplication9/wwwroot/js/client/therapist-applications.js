$(document).ready(function () {
    $('div#container-main main[role="main"]').removeClass('pb-3');

    $('#btn-benefits-of-working-psychotherapists').click(function () {
        localStorage.setItem('BenefitsOfWorkingPsychotherapistsModalVisited', 'true');
    })

    $('.btn-join').click(function (event) {
        event.preventDefault();
        let targetBtn = $(this);
        let targetBtnText = targetBtn.find('.btn-join-text');
        let redirectUrl = targetBtn.attr('href')
        let height = $(this).height();

        targetBtnText.fadeOut('fast', function () {
            $(this).html('<i class="fas fa-spinner fa-spin no-tick"></i>').fadeIn('slow');
            targetBtn.height(height);
        });

        setTimeout(function () {
            targetBtnText.fadeOut('fast', function () {
                $(this).html('<i class="ca-lightgreen fas fa-check no-tick"></i>').fadeIn('slow', function () {
                    setTimeout(function () {
                        window.location.href = redirectUrl;
                    }, 600);
                });
            })
        }, 1200)
    })
})