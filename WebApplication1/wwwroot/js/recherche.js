const delay = ms => new Promise(res => setTimeout(res, ms));
// Write your JavaScript code.

async function rechercheJeux() {
    const recherche = document.getElementById("inputJeu").value; // Correction de la syntaxe
    const reponse = await fetch("http://localhost:5248/Catalogue/recherche?nom=" + encodeURIComponent(recherche)); // Ajout d'encodeURIComponent
    if (reponse.ok) {
    const Jeux = await reponse.json();
    document.getElementById("affichageRecherche").innerHTML = "";
        Jeux.forEach(l => AfficherJeu(l));
    } else {
        console.error("Erreur lors de la récupération des jeux :", reponse.status);

    }
    if (barreRecherche.value !== "") {
        document.getElementById("affichageRecherche").style.display = "block";
    } else {
        document.getElementById("affichageRecherche").style.display = "none";
    }
}

function AfficherJeu(jeu) {
    let l = document.createElement("div");
    let titre = document.createElement("a");
    titre.href = "http://localhost:5248/Catalogue/Detail?id=" + jeu.jeuid;
    titre.textContent = jeu.titre;
    l.appendChild(titre);
    document.getElementById("affichageRecherche").appendChild(l);

}

let barreRecherche = document.getElementById("inputJeu");
barreRecherche.addEventListener("input", rechercheJeux);





