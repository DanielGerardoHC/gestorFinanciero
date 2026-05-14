using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using gestor_financiero.Models;

namespace gestor_financiero.Controllers
{
    public class DeudaController : Controller
    {
        private readonly FinanzasContext db = new FinanzasContext();

        // GET: Deuda
        public async Task<ActionResult> Index(int? idUsuario)
        {
            var query = db.Deudas.Include(d => d.Usuario).AsQueryable();
            if (idUsuario.HasValue) query = query.Where(d => d.IdUsuario == idUsuario.Value);

            ViewBag.IdUsuario = new SelectList(db.Usuarios, "IdUsuario", "Nombre", idUsuario);
            return View(await query.OrderByDescending(d => d.SaldoActual).ToListAsync());
        }

        // GET: Deuda/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var deuda = await db.Deudas
                .Include(d => d.Usuario)
                .Include(d => d.Pagos)
                .FirstOrDefaultAsync(d => d.IdDeuda == id);
            if (deuda == null) return HttpNotFound();
            return View(deuda);
        }

        // GET: Deuda/Create
        public ActionResult Create()
        {
            ViewBag.IdUsuario = new SelectList(db.Usuarios, "IdUsuario", "Nombre");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "IdUsuario,Nombre,SaldoActual,TasaInteres,PagoMinimo,Notas")] Deuda deuda)
        {
            if (deuda.SaldoActual < 0)
                ModelState.AddModelError("SaldoActual", "El saldo no puede ser negativo.");

            if (ModelState.IsValid)
            {
                db.Deudas.Add(deuda);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.IdUsuario = new SelectList(db.Usuarios, "IdUsuario", "Nombre", deuda.IdUsuario);
            return View(deuda);
        }

        // GET: Deuda/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var deuda = await db.Deudas.FindAsync(id);
            if (deuda == null) return HttpNotFound();
            ViewBag.IdUsuario = new SelectList(db.Usuarios, "IdUsuario", "Nombre", deuda.IdUsuario);
            return View(deuda);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "IdDeuda,IdUsuario,Nombre,SaldoActual,TasaInteres,PagoMinimo,Notas")] Deuda deuda)
        {
            if (ModelState.IsValid)
            {
                db.Entry(deuda).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.IdUsuario = new SelectList(db.Usuarios, "IdUsuario", "Nombre", deuda.IdUsuario);
            return View(deuda);
        }

        // GET: Deuda/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var deuda = await db.Deudas.Include(d => d.Usuario)
                .FirstOrDefaultAsync(d => d.IdDeuda == id);
            if (deuda == null) return HttpNotFound();
            return View(deuda);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var deuda = await db.Deudas.FindAsync(id);
            db.Deudas.Remove(deuda);
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
