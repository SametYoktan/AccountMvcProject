using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using School.Models;
using System.Diagnostics;

namespace School.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SchoolContext _context;

        public HomeController(ILogger<HomeController> logger, SchoolContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var list=_context.Student.ToList();
            return View(list);
        }

        public async Task<ActionResult> Delete(int Id)
        {
            var student = await _context.Student.FindAsync(Id);
            _context.Remove(student);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Create(Student student)
        {
            if (student.Id==0)
            {
                await _context.AddAsync(student);
            }
            else
            {
                _context.Update(student);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

		[Authorize]// bunu koydugumuz get isteklerinde kullanýcý oturumu düþtüyse proram cs deki logpath e gider
		//[AllowAnonymous] buda dahil etmemek için
		public IActionResult Student(int? Id)
        {
            Student student;
            if (Id.HasValue)
            {
                student = _context.Student.Find(Id);
            }
            else
            {
                student=new Student();
            }
            return View(student);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
