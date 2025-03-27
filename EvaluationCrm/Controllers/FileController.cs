using Microsoft.AspNetCore.Mvc;

namespace EvaluationCrm.Controllers
{
	public class FileController : Controller
	{
		// GET: FileController
		public ActionResult Index()
		{
			return View();
		}

		[HttpGet]
		public IActionResult Upload()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Upload(IFormFile file)
		{
			if (file != null && file.Length > 0)
			{
				// Créer le dossier uploads s'il n'existe pas
				var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
				Directory.CreateDirectory(uploadsFolder);

				// Générer un nom de fichier unique
				var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
				var filePath = Path.Combine(uploadsFolder, uniqueFileName);

				// Sauvegarder le fichier
				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await file.CopyToAsync(stream);
				}

				return RedirectToAction("Upload");
			}

			return View();
		}

	}
}
