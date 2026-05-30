using System.Data.Entity;
using System.Threading.Tasks;
using System.Web.Mvc;
using gestor_financiero.Models;

namespace gestor_financiero.Controllers
{
    /// <summary>
    /// Controlador de "Mi perfil". NO administra todos los usuarios.
    /// El usuario solo puede ver y editar sus propios datos.
    /// Para borrar la cuenta o cambiar contraseña: AccountController.
    /// </summary>
    public class UsuarioController : BaseAuthController
    {
        private readonly FinanzasContext db = new FinanzasContext();

        // GET: Usuario  -> Mi perfil
        public async Task<ActionResult> Index()
        {
            var miUsuario = await db.Usuarios.FindAsync(CurrentUserId);
            if (miUsuario == null) return HttpNotFound();
            return View(miUsuario);
        }

        // GET: Usuario/Details/<id ignorado>
        // Siempre muestra el del usuario actual, sin importar el id que pase la URL
        public async Task<ActionResult> Details()
        {
            var miUsuario = await db.Usuarios.FindAsync(CurrentUserId);
            if (miUsuario == null) return HttpNotFound();
            return View(miUsuario);
        }

        // GET: Usuario/Edit -> editar mi perfil
        public async Task<ActionResult> Edit()
        {
            var miUsuario = await db.Usuarios.FindAsync(CurrentUserId);
            if (miUsuario == null) return HttpNotFound();
            return View(miUsuario);
        }

        // POST: Usuario/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Nombre,Email")] Usuario form)
        {
            // Cargamos SIEMPRE el del usuario actual, ignoramos cualquier IdUsuario
            // que venga en el form (anti-tampering).
            var miUsuario = await db.Usuarios.FindAsync(CurrentUserId);
            if (miUsuario == null) return HttpNotFound();

            if (ModelState.IsValid)
            {
                miUsuario.Nombre = form.Nombre;
                miUsuario.Email = form.Email;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(miUsuario);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
