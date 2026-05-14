using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using gestor_financiero.Models;

namespace gestor_financiero.Controllers
{
    public class ResumenAnualController : Controller
    {
        private readonly FinanzasContext db = new FinanzasContext();

        // GET: ResumenAnual
        public async Task<ActionResult> Index()
        {
            var resumenes = db.ResumenesAnuales
                .Include(r => r.Usuario)
                .OrderByDescending(r => r.Anio);
            return View(await resumenes.ToListAsync());
        }

        // GET: ResumenAnual/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var resumen = await db.ResumenesAnuales.Include(r => r.Usuario)
                .FirstOrDefaultAsync(r => r.IdResumen == id);
            if (resumen == null) return HttpNotFound();
            return View(resumen);
        }

        // GET: ResumenAnual/Create
        public ActionResult Create()
        {
            ViewBag.IdUsuario = new SelectList(db.Usuarios, "IdUsuario", "Nombre");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "IdUsuario,Anio,Ingresos,Ahorros,GastosFijos,GastosVariables,Deudas")] ResumenAnual resumen)
        {
            bool existe = await db.ResumenesAnuales.AnyAsync(r =>
                r.IdUsuario == resumen.IdUsuario && r.Anio == resumen.Anio);
            if (existe)
                ModelState.AddModelError("", "Ya existe un resumen para ese usuario en ese año.");

            if (ModelState.IsValid)
            {
                db.ResumenesAnuales.Add(resumen);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.IdUsuario = new SelectList(db.Usuarios, "IdUsuario", "Nombre", resumen.IdUsuario);
            return View(resumen);
        }

        // GET: ResumenAnual/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var resumen = await db.ResumenesAnuales.FindAsync(id);
            if (resumen == null) return HttpNotFound();
            ViewBag.IdUsuario = new SelectList(db.Usuarios, "IdUsuario", "Nombre", resumen.IdUsuario);
            return View(resumen);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "IdResumen,IdUsuario,Anio,Ingresos,Ahorros,GastosFijos,GastosVariables,Deudas")] ResumenAnual resumen)
        {
            if (ModelState.IsValid)
            {
                db.Entry(resumen).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.IdUsuario = new SelectList(db.Usuarios, "IdUsuario", "Nombre", resumen.IdUsuario);
            return View(resumen);
        }

        // GET: ResumenAnual/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var resumen = await db.ResumenesAnuales.Include(r => r.Usuario)
                .FirstOrDefaultAsync(r => r.IdResumen == id);
            if (resumen == null) return HttpNotFound();
            return View(resumen);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var resumen = await db.ResumenesAnuales.FindAsync(id);
            db.ResumenesAnuales.Remove(resumen);
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
