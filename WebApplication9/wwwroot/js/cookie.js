document.addEventListener("DOMContentLoaded", function () {
    if (localStorage.getItem('cookieNoticeAnswered') != 'true') {
        let cookieNotice =
            '<div id="cookie-notice-card" class="card card-span align-items-baseline d-flex flex-wrap justify-content-between position-fixed mb-4" style="left: 1.5rem;width: 250px; bottom: 0px;z-index:100 !important;">' +
            '<div class="card-body">' +
            '<div class="card-img-top  mb-3 d-flex justify-content-start">' +
            '<div class="text-center mr-2 p-2 rounded-soft" data-toggle="set-color-and-background-color" data-color="#3e1c00" style="width: 2.7rem !important; height: 2.6rem !important;">' +
            '<i class="fad fa-cookie fa-fw fa-lg align-middle"></i>' +
            '</div>' +
            '<p class="d-flex align-items-center fs--1 mb-0">Kolačići</p>' +
            '</div>' +
            '<p class="fs--2">Ovaj veb sajt koristi kolačiće. Nastavkom pregledanja prihvatate našu upotrebu kolačića. Pročitajte više u našoj politici privatnosti.</p>' +
            '<button id="btn-cookie-notice" class="btn btn-sm btn-falcon-primary float-right fs--2" type="button">' +
            '<span id="btn-cookie-notice-text">' +
            'u redu' +
            '</span>' +
            '</button>' +
            '</div>' +
            '</div>';

        $('body').append($.parseHTML(cookieNotice));

        setTimeout(function () {
            $('#btn-cookie-notice').click(function () {
                localStorage.setItem('cookieNoticeAnswered', 'true');
                const btnCookieWidth = $(this).width();
                const btnCookieHeight = $(this).height();
                $('#btn-cookie-notice-text').fadeOut('fast', function () {
                    $(this).html('<i class="fas fa-check text-success no-tick"></i>').fadeIn('slow');
                    $('#btn-cookie-notice').width(btnCookieWidth);
                    $('#btn-cookie-notice').height(btnCookieHeight);
                    setTimeout(function () {
                        $('#cookie-notice-card').fadeOut(1100).promise().done(function() {
                            $('#cookie-notice-card').remove();
                        })
                    }, 1000)
                })
            })
        }, 20)
    }
});