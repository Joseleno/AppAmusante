using ARF.Identite.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ARF.Identite.API.Controllers
{
    [ApiController]
    [Route("api/identite")]
    public class AuthorizationController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AuthorizationController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }
        
        [HttpPost("nouveau-utilisateur")]
        public async Task<ActionResult> Inscrire(UtilisateurEnregistrementViewModel utilisateurEnregistrement)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var utilisateur = new IdentityUser
            {
                UserName = utilisateurEnregistrement.Courriel,
                Email = utilisateurEnregistrement.Courriel,
                EmailConfirmed = true
            };
            var resultat = await _userManager.CreateAsync(utilisateur, utilisateurEnregistrement.MotDePasse);

            if (!resultat.Succeeded)
            {
                return BadRequest();
            }
            await _signInManager.SignInAsync(utilisateur, false);
            return Ok();
        }

        [HttpPost("authentifier")]
        public async Task<ActionResult> Login(UtilisateurLoginViewModel utilisateurLogin)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var resultat = await _signInManager
                                 .PasswordSignInAsync(utilisateurLogin.Courriel,
                                 utilisateurLogin.MotDePasse,
                                 false,
                                 true);
            if (!resultat.Succeeded)
            {
                return BadRequest();
            }
            return Ok();
        }
    }
}
