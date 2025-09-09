async function rechercheJeux() {
    recherche = document.getElementById("inputJeu").value;
    const reponse = await fetch("http://localhost:5248/Catalogues/recherche?nom=" + recherche);
    const Jeux = await reponse.json();
    document.getElementById("affichageRecherche").innerHTML = "";
    Jeux.forEach(l => AfficherJeu(l));
}

function Afficherjeu(Jeux) {
    let l = document.createElement("div");
    let titre = document.createElement("a");
    titre.href = "http://localhost:5248/Catalogues/Detail/" + Jeux.jeuid;
    titre.textContent = livre.titre;
    l.appendChild(titre);
    document.getElementById("affichageRecherche").appendChild(l);
}

let barreRecherche = document.getElementById("inputJeu");
barreRecherche.addEventListener("input", rechercheLivre);


async function toggleRecherche() {
    let divRecherche = document.getElementById("affichageRecherche");
    if (divRecherche.classList.contains("actif")) {
        await delay(500);
    }
    divRecherche.classList.toggle("actif");
}

barreRecherche.addEventListener("focus", toggleRecherche);
barreRecherche.addEventListener("focusout", toggleRecherche);