using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using gestor_financiero.Models;

// NOTA: Para que las acciones que retornan PDF funcionen, instalar Rotativa via NuGet:
//   Install-Package Rotativa -Version 1.7.3
// Una vez instalado, descomentar los "using Rotativa;" y las acciones marcadas como PDF
// (las versiones "Html" siguen funcionando sin Rotativa, son vistas imprimibles).

namespace gestor_financiero.Controllers
{
    [Authorize]
    public class ReportesController : Controller
    {
        private readonly FinanzasContext db = new FinanzasContext();

        public ActionResult Index() => View();

        // -- Reporte de Transacciones --
        public async Task<ActionResult> Transacciones(DateTime? desde, DateTime? hasta)
        {
            var userId = int.Parse(User.Identity.Name);
            var d = desde ?? new DateTime(DateTime.Today.Year, 1, 1);
            var h = hasta ?? DateTime.Today;
            var tx = await db.Transacciones
                .Include(t => t.Categoria).Include(t => t.Usuario)
                .Where(t => t.IdUsuario == userId && t.Fecha >= d && t.Fecha <= h)
                .OrderBy(t => t.Fecha)
                .ToListAsync();
            ViewBag.Desde = d; ViewBag.Hasta = h;
            return View(tx);
        }

        // public ActionResult TransaccionesPdf(DateTime? desde, DateTime? hasta)
        // {
        //     return new Rotativa.ViewAsPdf("Transacciones", ...) { FileName = "Transacciones.pdf" };
        // }

        // -- Reporte de Deudas --
        public async Task<ActionResult> Deudas()
        {
            var userId = int.Parse(User.Identity.Name);
            var deudas = await db.Deudas
                .Include(d => d.Pagos)
                .Where(d => d.IdUsuario == userId)
                .ToListAsync();
            return View(deudas);
        }

        // -- Reporte de Presupuesto --
        public async Task<ActionResult> Presupuesto(int? id)
        {
            if (!id.HasValue)
            {
                var primero = await db.Presupuestos.OrderByDescending(p => p.Anio).ThenByDescending(p => p.Mes).FirstOrDefaultAsync();
                if (primero == null) return RedirectToAction("Index");
                id = primero.IdPresupuesto;
            }
            var p = await db.Presupuestos
                .Include(x => x.Usuario)
                .Include(x => x.Detalles.Select(d => d.Categoria))
                .FirstOrDefaultAsync(x => x.IdPresupuesto == id);
            return View(p);
        }

        // -- Reporte Resumen Anual --
        public async Task<ActionResult> ResumenAnual()
        {
            var userId = int.Parse(User.Identity.Name);
            var resumen = await db.ResumenesAnuales
                .Include(r => r.Usuario)
                .Where(r => r.IdUsuario == userId)
                .OrderByDescending(r => r.Anio)
                .ToListAsync();
            return View(resumen);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
