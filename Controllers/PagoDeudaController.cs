using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using gestor_financiero.Models;

namespace gestor_financiero.Controllers
{
    public class PagoDeudaController : Controller
    {
        private readonly FinanzasContext db = new FinanzasContext();

        // GET: PagoDeuda
        public async Task<ActionResult> Index(int? idDeuda)
        {
            var query = db.PagosDeuda.Include(p => p.Deuda).AsQueryable();
            if (idDeuda.HasValue) query = query.Where(p => p.IdDeuda == idDeuda.Value);

            ViewBag.IdDeuda = idDeuda;
            return View(await query.OrderByDescending(p => p.Fecha).ToListAsync());
        }

        // GET: PagoDeuda/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var pago = await db.PagosDeuda.Include(p => p.Deuda)
                .FirstOrDefaultAsync(p => p.IdPago == id);
            if (pago == null) return HttpNotFound();
            return View(pago);
        }

        // GET: PagoDeuda/Create
        public ActionResult Create(int? idDeuda)
        {
            ViewBag.IdDeuda = new SelectList(db.Deudas, "IdDeuda", "Nombre", idDeuda);
            return View(new PagoDeuda { Fecha = DateTime.Today, IdDeuda = idDeuda ?? 0 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "IdDeuda,Fecha,PagoMinimo,PagoExtra,Interes,Capital,SaldoRestante")] PagoDeuda pago)
        {
            if (ModelState.IsValid)
            {
                // Si no viene saldo_restante, recalcular contra el saldo actual de la deuda
                if (!pago.SaldoRestante.HasValue)
                {
                    var deuda = await db.Deudas.FindAsync(pago.IdDeuda);
                    if (deuda != null)
                        pago.SaldoRestante = deuda.SaldoActual - pago.Capital;
                }

                db.PagosDeuda.Add(pago);

                // Actualizar saldo actual de la deuda si calculamos uno nuevo
                if (pago.SaldoRestante.HasValue)
                {
                    var deuda = await db.Deudas.FindAsync(pago.IdDeuda);
                    if (deuda != null)
                    {
                        deuda.SaldoActual = pago.SaldoRestante.Value;
                        db.Entry(deuda).State = EntityState.Modified;
                    }
                }

                await db.SaveChangesAsync();
                return RedirectToAction("Index", new { idDeuda = pago.IdDeuda });
            }
            ViewBag.IdDeuda = new SelectList(db.Deudas, "IdDeuda", "Nombre", pago.IdDeuda);
            return View(pago);
        }

        // GET: PagoDeuda/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var pago = await db.PagosDeuda.FindAsync(id);
            if (pago == null) return HttpNotFound();
            ViewBag.IdDeuda = new SelectList(db.Deudas, "IdDeuda", "Nombre", pago.IdDeuda);
            return View(pago);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "IdPago,IdDeuda,Fecha,PagoMinimo,PagoExtra,Interes,Capital,SaldoRestante")] PagoDeuda pago)
        {
            if (ModelState.IsValid)
            {
                db.Entry(pago).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new { idDeuda = pago.IdDeuda });
            }
            ViewBag.IdDeuda = new SelectList(db.Deudas, "IdDeuda", "Nombre", pago.IdDeuda);
            return View(pago);
        }

        // GET: PagoDeuda/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var pago = await db.PagosDeuda.Include(p => p.Deuda)
                .FirstOrDefaultAsync(p => p.IdPago == id);
            if (pago == null) return HttpNotFound();
            return View(pago);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var pago = await db.PagosDeuda.FindAsync(id);
            int idDeuda = pago.IdDeuda;
            db.PagosDeuda.Remove(pago);
            await db.SaveChangesAsync();
            return RedirectToAction("Index", new { idDeuda });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
