using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ARF.Identity.API.Models
{
    public class UtilisateurLoginReponse
    {
        public string AccessToken { get; set; }
        public double Duree { get; set; }
        public UtilisateurJeton UtilisateurJeton { get; set; }
    }
}
