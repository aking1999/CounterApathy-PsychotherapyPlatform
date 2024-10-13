let sidebar = document.querySelector(".sidebar");
let closeBtn = document.querySelector("#btn");

closeBtn.addEventListener("click", () => {
    sidebar.classList.toggle("open");
    MenuBtnChange();
});

closeBtn.addEventListener("focusout", () => {
    if (sidebar.classList.contains('open')) {
        sidebar.classList.remove("open");
        MenuBtnChange();
    }
});

function MenuBtnChange() {
    if (sidebar.classList.contains("open")) {
        closeBtn.classList.remove("fa-bars");
    } else {
        closeBtn.classList.add("fa-bars");
    }
}

$('#href-sign-out').click(function (event) {
    event.preventDefault();
    $('#btn-sign-out').click();
});