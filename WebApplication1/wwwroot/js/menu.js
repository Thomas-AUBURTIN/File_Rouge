let icone = document.getElementById("menu");
let bloc = document.getElementById("bloc");

if (icone && bloc) {
    icone.addEventListener('mouseenter', show_menu);
    icone.addEventListener('mouseleave', hide_menu);
    bloc.addEventListener('mouseenter', show_menu);
    bloc.addEventListener('mouseleave', hide_menu);
} else {
    console.error("Élément #menu ou #bloc introuvable !");
}

function show_menu() {
    bloc.classList.add("actif");
}

function hide_menu() {
    bloc.classList.remove("actif");
}
