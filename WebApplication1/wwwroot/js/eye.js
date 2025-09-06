const eye = document.getElementById("eye");
const psw = document.getElementById("Utilisateur_motdePasse");
eye.addEventListener('mouseenter', function () {
psw.type = "text";
});
eye.addEventListener('mouseout', function () {
psw.type = "password";
});