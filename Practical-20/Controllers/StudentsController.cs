using System;
using Microsoft.AspNetCore.Mvc;
using Practical_20.Models;
using Practical_20.Repositories;

namespace ComprehensiveApp.Controllers
{
    public class StudentsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<StudentsController> _logger;

        public StudentsController(IUnitOfWork unitOfWork, ILogger<StudentsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        // GET: Students
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Getting all students");
            var students = await _unitOfWork.GetRepository<Student>().GetAllAsync();
            return View(students);
        }
        // GET: Students/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Details requested without ID");
                return NotFound();
            }

            var student = await _unitOfWork.GetRepository<Student>().GetByIdAsync(id.Value);
            if (student == null)
            {
                _logger.LogWarning($"Student with ID {id} not found");
                return NotFound();
            }

            return View(student);
        }
        // GET: Students/Create
        public IActionResult Create()
        {
            return View();
        }
        // POST: Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Email,EnrollmentDate")] Student student)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _unitOfWork.GetRepository<Student>().AddAsync(student);
                    await _unitOfWork.SaveChangesAsync();

                    _logger.LogInformation($"Student {student.Name} created successfully");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating student");
                    ModelState.AddModelError("", "An error occurred while creating the student.");
                }
            }
            return View(student);
        }
        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _unitOfWork.GetRepository<Student>().GetByIdAsync(id.Value);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }
        // POST: Students/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email,EnrollmentDate")] Student student)
        {
            if (id != student.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _unitOfWork.GetRepository<Student>().Update(student);
                    await _unitOfWork.SaveChangesAsync();

                    _logger.LogInformation($"Student {student.Name} updated successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error updating student with ID {id}");
                    if (!await StudentExists(student.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ModelState.AddModelError("", "An error occurred while updating the student.");
                        return View(student);
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }
        // GET: Students/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _unitOfWork.GetRepository<Student>().GetByIdAsync(id.Value);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }
        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _unitOfWork.GetRepository<Student>().GetByIdAsync(id);
            if (student != null)
            {
                _unitOfWork.GetRepository<Student>().Remove(student);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Student {student.Name} deleted successfully");
            }
            return RedirectToAction(nameof(Index));
        }
        private async Task<bool> StudentExists(int id)
        {
            var student = await _unitOfWork.GetRepository<Student>().GetByIdAsync(id);
            return student != null;
        }
        public IActionResult ThrowTestException()
        {
            throw new Exception("This is a test exception from StudentsController.");
        }

    }
}
