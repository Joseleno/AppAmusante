using ARF.Identite.API.Models;
using ARF.Identity.API.Extensions;
using ARF.Identity.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ARF.Identite.API.Controllers
{
    [ApiController]
    [Route("api/identite")]
    public class AuthorizationController : Controller
    {
        
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        private readonly AppSettings _appSettings;

        public AuthorizationController(SignInManager<IdentityUser> signInManager,
                                       UserManager<IdentityUser> userManager, 
                                       IOptions<AppSettings> appSettings)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _appSettings = appSettings.Value;
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
            //return Ok(GenererJeton(utilisateurEnregistrement.Courriel));
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
            //return Ok(await GenererJeton(utilisateurLogin.Courriel));
        }

        public async Task<UtilisateurLoginReponse> GenererJeton(string courriel)
        {
            var utilisateur = await _userManager.FindByEmailAsync(courriel);
            var utilisateurClaims = await _userManager.GetClaimsAsync(utilisateur);
            var utilisateurRoles = await _userManager.GetRolesAsync(utilisateur);

            utilisateurClaims.Add(new Claim(JwtRegisteredClaimNames.Sub, utilisateur.Id));
            utilisateurClaims.Add(new Claim(JwtRegisteredClaimNames.Email, utilisateur.Email));

            //ID Token
            utilisateurClaims.Add(new Claim(JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString()));

            //Date creation Token
            utilisateurClaims.Add(new Claim(JwtRegisteredClaimNames.Iat,
                ToUnixEpochDate(DateTime.UtcNow).ToString(),
                ClaimValueTypes.Integer64));

            //Date expiration Token
            utilisateurClaims.Add(new Claim(JwtRegisteredClaimNames.Nbf,
                ToUnixEpochDate(DateTime.UtcNow).ToString()));

            //ajouter des roles comment claims
            foreach (var utilisateurRole in utilisateurRoles)
            {
                utilisateurClaims.Add(new Claim("role", utilisateurRole));
            }

            var identityClaims = new ClaimsIdentity();
            identityClaims.AddClaims(utilisateurClaims);

            var tokenHandler = new JwtSecurityTokenHandler();
            var cle = Encoding.ASCII.GetBytes(_appSettings.Cle);

            // creer le jeton
            var jeton = tokenHandler.CreateToken(
                new SecurityTokenDescriptor
                {
                    Issuer = _appSettings.Emetteur,
                    Audience = _appSettings.Couverture,
                    Subject = identityClaims,
                    Expires = DateTime.UtcNow.AddHours(_appSettings.Duree),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(cle),
                        SecurityAlgorithms.HmacSha256Signature)
                });

            //Obtenir jeton
            var encodedJeton = tokenHandler.WriteToken(jeton);

            var reponse = new UtilisateurLoginReponse
            {
                AccessToken = encodedJeton,
                Duree = TimeSpan.FromHours(_appSettings.Duree).TotalSeconds,
                UtilisateurJeton = new UtilisateurJeton
                {
                    Id = utilisateur.Id,
                    Courriel = utilisateur.Email,
                    Claims = utilisateurClaims.Select(c =>
                                new UtilisateurClaim { Type = c.Type, Value = c.Value })
                }
            };

            return reponse;
        }

        private static long ToUnixEpochDate(DateTime date)
            => (long)Math.Round((date.ToUniversalTime() -
                new DateTimeOffset(1960, 1, 1, 0, 0, 0, TimeSpan.Zero))
                .TotalSeconds);
    }
}