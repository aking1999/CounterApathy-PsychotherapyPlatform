$(document).ready(function () {
    const download = $('#btn-download-receipt');
    const downloadText = $('#btn-download-receipt-text');
    const downloadTextUnchanged = downloadText.html();

    download.click(function (event) {
        event.preventDefault();
        $(this).prop('disabled', true);
        downloadText.fadeOut('fast', function () {
            $(this).html('<i class="fas fa-spinner fa-spin text-primary no-tick"></i>').fadeIn('slow');
        })

        setTimeout(function () {
            downloadText.fadeOut('fast', function () {
                $(this).html('<i class="text-success fas fa-check no-tick"></i>').fadeIn('slow', function () {
                    html2canvas(document.querySelector('#capture')).then(function (canvas) {
                        SaveAs(canvas.toDataURL(), 'receipt.png');
                    });
                    setTimeout(function () {
                        downloadText.fadeOut('fast', function () {
                            $(this).html(downloadTextUnchanged).fadeIn('slow');
                            download.prop('disabled', false);
                        });
                    }, 2000)
                })
            })
        }, 1200)
    })

    function SaveAs(uri, filename) {
        var link = document.createElement('a');

        if (typeof link.download === 'string') {
            link.href = uri;
            link.download = filename;

            //Firefox requires the link to be in the body
            document.body.appendChild(link);

            //simulate click
            link.click();

            //remove the link when done
            document.body.removeChild(link);
        } else window.open(uri);
    }
})