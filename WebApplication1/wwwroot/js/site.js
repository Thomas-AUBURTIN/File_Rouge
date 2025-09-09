// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function DemanderConfirmation(event) {
    let rep = confirm("Voulez vous vraiment supprimer ce jeux ?");
    if (rep != true) {
        event.preventDefault();
    }
}

let formsSupprLivre = document.getElementsByClassName("formSupprJeux");
Array.from(formsSupprLivre).forEach(form => {
    form.addEventListener("submit", DemanderConfirmation)
});



   

