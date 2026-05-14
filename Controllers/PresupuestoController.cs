using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using gestor_financiero.Models;

namespace gestor_financiero.Controllers
{
    public class PresupuestoController : Controller
    {
        private readonly FinanzasContext db = new FinanzasContext();

        // GET: Presupuesto
        public async Task<ActionResult> Index()
        {
            var presupuestos = db.Presupuestos
                .Include(p => p.Usuario)
                .OrderByDescending(p => p.Anio).ThenByDescending(p => p.Mes);
            return View(await presupuestos.ToListAsync());
        }

        // GET: Presupuesto/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var presupuesto = await db.Presupuestos
                .Include(p => p.Usuario)
                .Include(p => p.Detalles.Select(d => d.Categoria))
                .FirstOrDefaultAsync(p => p.IdPresupuesto == id);
            if (presupuesto == null) return HttpNotFound();
            return View(presupuesto);
        }

        // GET: Presupuesto/Create
        public ActionResult Create()
        {
            ViewBag.IdUsuario = new SelectList(db.Usuarios, "IdUsuario", "Nombre");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "IdUsuario,Anio,Mes")] Presupuesto presupuesto)
        {
            // No permitir presupuestos duplicados para el mismo usuario/año/mes
            bool existe = await db.Presupuestos.AnyAsync(p =>
                p.IdUsuario == presupuesto.IdUsuario &&
                p.Anio == presupuesto.Anio &&
                p.Mes == presupuesto.Mes);
            if (existe)
                ModelState.AddModelError("", "Ya existe un presupuesto para ese usuario en ese mes/año.");

            if (ModelState.IsValid)
            {
                db.Presupuestos.Add(presupuesto);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.IdUsuario = new SelectList(db.Usuarios, "IdUsuario", "Nombre", presupuesto.IdUsuario);
            return View(presupuesto);
        }

        // GET: Presupuesto/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var presupuesto = await db.Presupuestos.FindAsync(id);
            if (presupuesto == null) return HttpNotFound();
            ViewBag.IdUsuario = new SelectList(db.Usuarios, "IdUsuario", "Nombre", presupuesto.IdUsuario);
            return View(presupuesto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "IdPresupuesto,IdUsuario,Anio,Mes")] Presupuesto presupuesto)
        {
            if (ModelState.IsValid)
            {
                db.Entry(presupuesto).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.IdUsuario = new SelectList(db.Usuarios, "IdUsuario", "Nombre", presupuesto.IdUsuario);
            return View(presupuesto);
        }

        // GET: Presupuesto/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var presupuesto = await db.Presupuestos.Include(p => p.Usuario)
                .FirstOrDefaultAsync(p => p.IdPresupuesto == id);
            if (presupuesto == null) return HttpNotFound();
            return View(presupuesto);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var presupuesto = await db.Presupuestos.FindAsync(id);
            db.Presupuestos.Remove(presupuesto);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
