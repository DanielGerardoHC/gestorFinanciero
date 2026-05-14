using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;
using gestor_financiero.Models;
using gestor_financiero.ViewModels;

namespace gestor_financiero.Controllers
{
    public class AccountController : Controller
    {
        private readonly FinanzasContext db = new FinanzasContext();

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid) return View(model);

            var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (usuario == null || !Crypto.VerifyHashedPassword(usuario.PasswordHash, model.Password))
            {
                ModelState.AddModelError("", "Correo o contraseña incorrectos.");
                return View(model);
            }

            FormsAuthentication.SetAuthCookie(usuario.IdUsuario.ToString(), model.RememberMe);
            Session["UserName"] = usuario.Nombre;
            Session["UserId"] = usuario.IdUsuario;

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public ActionResult Register() => View();

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (await db.Usuarios.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Ya existe una cuenta con ese correo.");
                return View(model);
            }

            var usuario = new Usuario
            {
                Nombre = model.Nombre,
                Email = model.Email,
                PasswordHash = Crypto.HashPassword(model.Password),
                FechaRegistro = System.DateTime.Now
            };
            db.Usuarios.Add(usuario);
            await db.SaveChangesAsync();

            FormsAuthentication.SetAuthCookie(usuario.IdUsuario.ToString(), false);
            Session["UserName"] = usuario.Nombre;
            Session["UserId"] = usuario.IdUsuario;
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
