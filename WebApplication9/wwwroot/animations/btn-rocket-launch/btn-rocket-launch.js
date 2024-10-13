$(document).ready(function () {
  const d = 40;
  document.querySelectorAll(".rocket-button").forEach((elem) => {
    elem.querySelectorAll(".default, .success > div").forEach((text) => {
      //charming(text);
      $(text).lettering();
      text.querySelectorAll("span").forEach((span, i) => {
        span.innerHTML = span.textContent == " " ? "&nbsp;" : span.textContent;
        span.style.setProperty("--d", i * d + "ms");
        span.style.setProperty(
          "--ds",
          text.querySelectorAll("span").length * d - d - i * d + "ms"
        );
      });
    });

    elem.addEventListener("click", (e) => {
      e.preventDefault();
      if (elem.classList.contains("animated")) {
        return;
        }
        $(elem).addClass("animated");
        $(elem).addClass("live").promise().done(() => {
            setTimeout(() => {
                $('#ovde ide navbar').fadeOut('slow');
                //$('#success').fadeOut('fast', function () {
                //    $(this).html('Please wait<i class="ml-1 ca-lightgreen fas fa-spinner fa-spin"></i>').css('display', 'inline-block').hide().fadeIn('slow', function () {
                //        setTimeout(() => {
                //            $(this).fadeOut('fast', function () {
                //                $(this).html('Launching<i class="ml-1 ca-lightgreen fas fa-spinner fa-spin"></i>').fadeIn('slow');
                //            })
                //        }, 1300);
                //    });
                //})
            }, 3000);
        })
    });
  });
});
