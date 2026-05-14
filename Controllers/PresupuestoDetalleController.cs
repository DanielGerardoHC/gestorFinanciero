using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using gestor_financiero.Models;

namespace gestor_financiero.Controllers
{
    public class PresupuestoDetalleController : Controller
    {
        private readonly FinanzasContext db = new FinanzasContext();

        // GET: PresupuestoDetalle
        public async Task<ActionResult> Index(int? idPresupuesto)
        {
            var query = db.PresupuestoDetalles
                .Include(d => d.Categoria)
                .Include(d => d.Presupuesto)
                .AsQueryable();

            if (idPresupuesto.HasValue)
                query = query.Where(d => d.IdPresupuesto == idPresupuesto.Value);

            ViewBag.IdPresupuesto = idPresupuesto;
            return View(await query.ToListAsync());
        }

        // GET: PresupuestoDetalle/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var detalle = await db.PresupuestoDetalles
                .Include(d => d.Categoria)
                .Include(d => d.Presupuesto)
                .FirstOrDefaultAsync(d => d.IdDetalle == id);
            if (detalle == null) return HttpNotFound();
            return View(detalle);
        }

        // GET: PresupuestoDetalle/Create
        public ActionResult Create(int? idPresupuesto)
        {
            ViewBag.IdPresupuesto = new SelectList(db.Presupuestos, "IdPresupuesto", "IdPresupuesto", idPresupuesto);
            ViewBag.IdCategoria = new SelectList(db.Categorias, "IdCategoria", "Nombre");
            return View(new PresupuestoDetalle { IdPresupuesto = idPresupuesto ?? 0 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "IdPresupuesto,IdCategoria,Estimado,Real")] PresupuestoDetalle detalle)
        {
            if (ModelState.IsValid)
            {
                db.PresupuestoDetalles.Add(detalle);
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new { idPresupuesto = detalle.IdPresupuesto });
            }
            ViewBag.IdPresupuesto = new SelectList(db.Presupuestos, "IdPresupuesto", "IdPresupuesto", detalle.IdPresupuesto);
            ViewBag.IdCategoria = new SelectList(db.Categorias, "IdCategoria", "Nombre", detalle.IdCategoria);
            return View(detalle);
        }

        // GET: PresupuestoDetalle/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var detalle = await db.PresupuestoDetalles.FindAsync(id);
            if (detalle == null) return HttpNotFound();
            ViewBag.IdPresupuesto = new SelectList(db.Presupuestos, "IdPresupuesto", "IdPresupuesto", detalle.IdPresupuesto);
            ViewBag.IdCategoria = new SelectList(db.Categorias, "IdCategoria", "Nombre", detalle.IdCategoria);
            return View(detalle);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "IdDetalle,IdPresupuesto,IdCategoria,Estimado,Real")] PresupuestoDetalle detalle)
        {
            if (ModelState.IsValid)
            {
                db.Entry(detalle).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new { idPresupuesto = detalle.IdPresupuesto });
            }
            ViewBag.IdPresupuesto = new SelectList(db.Presupuestos, "IdPresupuesto", "IdPresupuesto", detalle.IdPresupuesto);
            ViewBag.IdCategoria = new SelectList(db.Categorias, "IdCategoria", "Nombre", detalle.IdCategoria);
            return View(detalle);
        }

        // GET: PresupuestoDetalle/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var detalle = await db.PresupuestoDetalles
                .Include(d => d.Categoria)
                .Include(d => d.Presupuesto)
                .FirstOrDefaultAsync(d => d.IdDetalle == id);
            if (detalle == null) return HttpNotFound();
            return View(detalle);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var detalle = await db.PresupuestoDetalles.FindAsync(id);
            int idPresupuesto = detalle.IdPresupuesto;
            db.PresupuestoDetalles.Remove(detalle);
            await db.SaveChangesAsync();
            return RedirectToAction("Index", new { idPresupuesto });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
