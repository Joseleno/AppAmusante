using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ARF.Identite.API.Models
{
    public class UtilisateurEnregistrementViewModel
    {
        [Required(ErrorMessage ="Le champ {0} est requis.")]
        [EmailAddress(ErrorMessage = "Le champ {0} est en format invalide")]
        public string Courriel { get; set; }

        [Required(ErrorMessage = "Le champ {0} est requis.")]
        [StringLength(20, ErrorMessage = "Le champ {0} doit etre ao moins 6 caractes", MinimumLength =6)]
        public string MotDePasse { get; set; }
        
        [Compare("MotDePasse", ErrorMessage = "Les mots de passe ne sont pas les mêmes")]
        public string MotDePasseconfirmation { get; set; }

        
    }
}
