///image profil
//recuperation l'elment html img image-profil et input type file pp
const img_profil = document.getElementById('img-profil');
const input_profil = document.getElementById('pp');

//ajout d'un listenner sur l'input file quand les fichier change
input_profil.addEventListener('change', change_img);

/*
function pour changer la source de la balise image suivant le changement fait dans l'input file
 */
function change_img() {
    //recuperation du chichier dans l'input
    var file = input_profil.files[0];
    //creation de l'objet lecteur
    var reader = new FileReader();
    //creation d'une ecoute sur l'objet lecteur quand celuio si a ete charger
    reader.onload = function (e) {
        //on midifie la source de l'élément image par la propriré result du parent target de l'objet e de type Progressevent
        img_profil.onload = function () {
            img_profil.src = e.target.result;
        }

    }
    //on charge le ficher dans le lecteur pour appliquer le onload
    reader.readAsDataURL(file);
    console.log("ca marche ou pas ");
}
///fin image profil