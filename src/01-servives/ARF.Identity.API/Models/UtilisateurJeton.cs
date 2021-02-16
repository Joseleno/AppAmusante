using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ARF.Identity.API.Models
{
    public class UtilisateurJeton
    {
        public string Id { get; set; }
        public string Courriel { get; set; }
        public IEnumerable<UtilisateurClaim> Claims { get; set; }
    }
}
