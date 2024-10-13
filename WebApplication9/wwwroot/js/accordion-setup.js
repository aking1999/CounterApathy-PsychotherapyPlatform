$(document).ready(function () {
    let accordions = $('.accordion');

    $.each(accordions, function (i) {
        accordions[i].setAttribute('id', `accordion-${i}`)
        let accordionItems = $(accordions[i]).find('.accordion-item');

        $.each(accordionItems, function (j) {
            let item = $(accordionItems[j]);
            let cardHeader = item.find('.card-header');
            let cardHeaderBtn = cardHeader.find('button')
            cardHeader.attr('id', `accordion-heading-${i}-${j}`);
            cardHeaderBtn
                .attr('data-target', `#collapse-${i}-${j}`)
                .attr('aria-controls', `collapse-${i}-${j}`);

            let collapse = item.find('.collapse');
            collapse
                .attr('id', `collapse-${i}-${j}`)
                .attr('aria-labelledby', `accordion-heading-${i}-${j}`)
                .attr('data-parent', `#accordion-${i}`);

            cardHeaderBtn.attr('aria-expanded', 'false');
            collapse.removeClass('show');

            //if (i === 0 && j === 0) {
            //    cardHeaderBtn.attr('aria-expanded', 'true');
            //    //collapse.addClass('show'); always have the first open
            //} else {
            //    cardHeaderBtn.attr('aria-expanded', 'false');
            //    collapse.removeClass('show');
            //}
        })
    })
});