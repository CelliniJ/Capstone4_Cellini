using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Capstone4_CelliniRedux.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Capstone4_CelliniRedux.Controllers
{
    public class HomeController : Controller
    {

        private readonly UserDbContext _userdb;

        public HomeController(UserDbContext userdb)
        {
            _userdb = userdb;
        }

        public IActionResult Index()
        {
            return View();
        }

        //Shows the database to the user.
        [Authorize]
        public IActionResult ShowList()
        {
            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            List<ToDo> toDo = _userdb.ToDo.Where(X => X.UserId == id).ToList();
            return View(toDo);
        }

        //Displays info in the view to the user.
        [HttpGet]
        public IActionResult AddToDo()
        {
            return View();
        }

        //Receives information from the view. Let's you add new tasks to the database.
        [HttpPost]
        public IActionResult AddToDo(ToDo newToDo)
        {
            newToDo.UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (ModelState.IsValid)
            {
                _userdb.ToDo.Add(newToDo);
                _userdb.SaveChanges();
            }
            return RedirectToAction("ShowList");
        }

        //Brings the task we want to edit to the user.
        public IActionResult EditToDo(int id)
        {
            ToDo editedToDo = _userdb.ToDo.Find(id);
            if (editedToDo == null)
            {
                return RedirectToAction("ShowList");
            }
            else
            {
                return View(editedToDo);
            }
        }

        //Let's you delete tasks from the database.
        public IActionResult DeleteToDo(int id)
        {
            var toDoFind = _userdb.ToDo.Find(id);
            if(toDoFind != null)
            {
                _userdb.ToDo.Remove(toDoFind);
                _userdb.SaveChanges();
            }
            return RedirectToAction("ShowList");
        }

        //Updates the marking of complete on tasks.
        public IActionResult ChangeComplete(int id)
        {
            var toDo = _userdb.ToDo.Find(id);
            if (toDo != null)
            {
                toDo.Complete = true;
                _userdb.ToDo.Update(toDo);
                _userdb.SaveChanges();
            }
            return RedirectToAction("ShowList");
        }

        //Let's you filter tasks by completion.
        public IActionResult SearchCompletion(bool status)
        {
            List<ToDo> searchList = new List<ToDo>();
            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var tasklist = _userdb.ToDo.Where(x => x.UserId == id).ToList();
            foreach (ToDo task in tasklist)
            {
                if (task.Complete == status)
                {
                    searchList.Add(task);
                }
                else if (task.Complete == null && status == false)
                {
                    searchList.Add(task);
                }
            }

            return View("ShowList", searchList);
        }

        //Let's you filter tasks by date.
        public IActionResult SearchDate(DateTime status)
        {
            List<ToDo> searchList = new List<ToDo>();
            string id = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var tasklist = _userdb.ToDo.Where(x => x.UserId == id).ToList();
            foreach (ToDo task in tasklist)
            {
                if (task.DueDate <= status)
                {
                    searchList.Add(task);
                }
            }

            return View("ShowList", searchList);
        }

        //Saves the changes made to tasks.
        public IActionResult Save(ToDo updatedToDo)
        {
            ToDo dbuser = _userdb.ToDo.Find(updatedToDo.Id);

            dbuser.Description = updatedToDo.Description;
            dbuser.DueDate = updatedToDo.DueDate;
            dbuser.Complete = updatedToDo.Complete;

            _userdb.Entry(dbuser).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            _userdb.Update(dbuser);
            _userdb.SaveChanges();

            return RedirectToAction("ShowList");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
