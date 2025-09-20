
    let barreRecherche = document.getElementById("inputJeu");
    let affichageRecherche = document.getElementById("affichageRecherche");

    if (barreRecherche && affichageRecherche) {
        barreRecherche.addEventListener("input", rechercheJeux);
    } else {
        console.error("Élément #inputJeu ou #affichageRecherche introuvable !");
    }

    async function rechercheJeux() {
        const recherche = barreRecherche.value;
        const reponse = await fetch("/Catalogue/recherche?nom=" + encodeURIComponent(recherche));
        if (reponse.ok) {
            const Jeux = await reponse.json();
            affichageRecherche.innerHTML = "";
            Jeux.forEach(l => AfficherJeu(l));
        } else {
            console.error("Erreur lors de la récupération des jeux :", reponse.status);
        }
    }

    function AfficherJeu(jeu) {
        let l = document.createElement("div");
        let titre = document.createElement("a");
        titre.href = "/Catalogue/Detail?id=" + jeu.jeuid;
        titre.textContent = jeu.titre;
        l.appendChild(titre);
        affichageRecherche.appendChild(l);

        barreRecherche.addEventListener('mouseenter', show_menu);
        barreRecherche.addEventListener('mouseleave', hide_menu);
        affichageRecherche.addEventListener('mouseenter', show_menu);
        affichageRecherche.addEventListener('mouseleave', hide_menu);





}
function show_menu() {
    affichageRecherche.classList.add("actif");
}

function hide_menu() {
    affichageRecherche.classList.remove("actif");
}
