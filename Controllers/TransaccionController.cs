using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using gestor_financiero.Models;

namespace gestor_financiero.Controllers
{
    public class TransaccionController : Controller
    {
        private readonly FinanzasContext db = new FinanzasContext();

        // GET: Transaccion
        public async Task<ActionResult> Index(int? idUsuario, DateTime? desde, DateTime? hasta)
        {
            var query = db.Transacciones
                .Include(t => t.Categoria)
                .Include(t => t.Usuario)
                .AsQueryable();

            if (idUsuario.HasValue) query = query.Where(t => t.IdUsuario == idUsuario.Value);
            if (desde.HasValue)     query = query.Where(t => t.Fecha >= desde.Value);
            if (hasta.HasValue)     query = query.Where(t => t.Fecha <= hasta.Value);

            ViewBag.IdUsuario = new SelectList(db.Usuarios, "IdUsuario", "Nombre", idUsuario);
            ViewBag.Desde = desde?.ToString("yyyy-MM-dd");
            ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd");

            return View(await query.OrderByDescending(t => t.Fecha).ToListAsync());
        }

        // GET: Transaccion/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var transaccion = await db.Transacciones
                .Include(t => t.Categoria)
                .Include(t => t.Usuario)
                .FirstOrDefaultAsync(t => t.IdTransaccion == id);
            if (transaccion == null) return HttpNotFound();
            return View(transaccion);
        }

        // GET: Transaccion/Create
        public ActionResult Create()
        {
            ViewBag.IdUsuario = new SelectList(db.Usuarios, "IdUsuario", "Nombre");
            ViewBag.IdCategoria = new SelectList(db.Categorias, "IdCategoria", "Nombre");
            return View(new Transaccion { Fecha = DateTime.Today });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "IdUsuario,IdCategoria,Monto,Fecha,Notas")] Transaccion transaccion)
        {
            if (transaccion.Monto <= 0)
                ModelState.AddModelError("Monto", "El monto debe ser mayor que cero.");

            if (ModelState.IsValid)
            {
                db.Transacciones.Add(transaccion);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.IdUsuario = new SelectList(db.Usuarios, "IdUsuario", "Nombre", transaccion.IdUsuario);
            ViewBag.IdCategoria = new SelectList(db.Categorias, "IdCategoria", "Nombre", transaccion.IdCategoria);
            return View(transaccion);
        }

        // GET: Transaccion/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var transaccion = await db.Transacciones.FindAsync(id);
            if (transaccion == null) return HttpNotFound();
            ViewBag.IdUsuario = new SelectList(db.Usuarios, "IdUsuario", "Nombre", transaccion.IdUsuario);
            ViewBag.IdCategoria = new SelectList(db.Categorias, "IdCategoria", "Nombre", transaccion.IdCategoria);
            return View(transaccion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "IdTransaccion,IdUsuario,IdCategoria,Monto,Fecha,Notas")] Transaccion transaccion)
        {
            if (ModelState.IsValid)
            {
                db.Entry(transaccion).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.IdUsuario = new SelectList(db.Usuarios, "IdUsuario", "Nombre", transaccion.IdUsuario);
            ViewBag.IdCategoria = new SelectList(db.Categorias, "IdCategoria", "Nombre", transaccion.IdCategoria);
            return View(transaccion);
        }

        // GET: Transaccion/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var transaccion = await db.Transacciones
                .Include(t => t.Categoria)
                .Include(t => t.Usuario)
                .FirstOrDefaultAsync(t => t.IdTransaccion == id);
            if (transaccion == null) return HttpNotFound();
            return View(transaccion);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var transaccion = await db.Transacciones.FindAsync(id);
            db.Transacciones.Remove(transaccion);
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
