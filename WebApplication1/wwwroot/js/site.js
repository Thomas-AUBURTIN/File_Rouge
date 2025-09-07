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


document.getElementById('formRecherche').addEventListener('submit', function (event) {
    event.preventDefault(); // Empêche le rechargement[6][14].

    // Récupère la valeur saisie
    const nomJeu = document.getElementById('inputJeu').value.trim();

    // On prépare le paramètre (GET recommandé pour une recherche simple)
    fetch(`/Catalogue/Recherche?nom=${encodeURIComponent(nomJeu)}`) // Adapte l’URL à ton API
        .then(response => {
            if (!response.ok) throw new Error('Jeu non trouvé');
            return response.json();
        })
        .then(data => {
            // Si data est un int, aucune propriété à lire
            if (typeof data === "number") {
                window.location.href = `/catalogue/Detail?id=${data}`;
            } else {
                alert("Aucun jeu trouvé");
            }
        })
        .catch(error => {
            alert('Erreur lors de la recherche : ' + error.message);
        });
});

