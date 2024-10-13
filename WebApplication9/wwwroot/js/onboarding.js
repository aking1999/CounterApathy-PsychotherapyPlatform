$(document).ready(function () {
    var walkthrough;
    walkthrough = {
        index: 0,
        nextScreen: function () {
            if (this.index < this.indexMax()) {
                this.index++;
                return this.updateScreen();
            }
        },
        prevScreen: function () {
            if (this.index > 0) {
                this.index--;
                return this.updateScreen();
            }
        },
        updateScreen: function () {
            this.reset();
            this.goTo(this.index);
            return this.setBtns();
        },
        setBtns: function () {
            var $lastBtn, $nextBtn, $prevBtn;
            $nextBtn = $(".next-screen");
            $prevBtn = $(".prev-screen");
            $lastBtn = $(".finish");
            if (walkthrough.index === walkthrough.indexMax()) {
                $nextBtn.prop("disabled", true);
                $prevBtn.prop("disabled", false);
                return $lastBtn.addClass("active").prop("disabled", false);
            } else if (walkthrough.index === 0) {
                $nextBtn.prop("disabled", false);
                $prevBtn.prop("disabled", true);
                return $lastBtn.removeClass("active").prop("disabled", true);
            } else {
                $nextBtn.prop("disabled", false);
                $prevBtn.prop("disabled", false);
                return $lastBtn.removeClass("active").prop("disabled", true);
            }
        },
        goTo: function (index) {
            $(".screen").eq(index).addClass("active");
            return $(".dot").eq(index).addClass("active");
        },
        reset: function () {
            return $(".screen, .dot").removeClass("active");
        },
        indexMax: function () {
            return $(".screen").length - 1;
        }
    };

    $(".next-screen").click(function () {
        return walkthrough.nextScreen();
    });
    $(".prev-screen").click(function () {
        return walkthrough.prevScreen();
    });

    return $(document).keydown(function (e) {
        //ovo mozda pravi gresku da ne moze da se kuca dok je onboarding prikazan
        if ($("div.modal").data('bs.modal')?._isShown) {
            switch (e.which) {
                case 37:
                    walkthrough.prevScreen();
                    break;
                case 39:
                    walkthrough.nextScreen();
                    break;
                default:
                    return;
            }
            e.preventDefault();
        }
    });
});