using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ARF.Identity.API.Models
{
    public class UtilisateurToken
    {
        public string Id { get; set; }
        public int Courriel { get; set; }
        public IEnumerable<UtilisateurClaim> MyProperty { get; set; }
    }
}
