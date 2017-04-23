using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace Project.Controllers.Admin
{
    public class UserController : Controller
    {

        private HashSet<string> GetAdminsUserName(List<ApplicationUser> users, ProjectDbContext context)
        {
            var userManager = new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(context));

            var admins = new HashSet<string>();

            foreach (var user in users)
            {
                if (userManager.IsInRole(user.Id, "Admin"))
                {
                    admins.Add(user.UserName);
                };
            }

            return admins;
        }
        // GET: User
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            using (var database = new ProjectDbContext())
            {
                var users = database.Users.ToList();

                var admins = GetAdminsUserName(users, database);
                ViewBag.Admins = admins;

                return View(users);
            };
        }

        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new ProjectDbContext())
            {
                var user = database.Users.Where(u => u.Id == id).First();

                if (user==null)
                {
                   return HttpNotFound();
                }

                var viewModel = new EditUserViewModel();
                viewModel.User = user;
                viewModel.Roles = GetUserRoles(user, database);

                return View(viewModel);
            }
        }

        [HttpPost]
        public ActionResult Edit(string id, EditUserViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                using (var database = new ProjectDbContext())
                {

                    var user = database.Users.FirstOrDefault(u => u.Id == id);

                    if (user == null)
                    {
                        return HttpNotFound();
                    }

                    if (!string.IsNullOrEmpty(viewModel.Password))
                    {
                        var hesher = new PasswordHasher();
                        var passwordHasher = hesher.HashPassword(viewModel.Password);
                        user.PasswordHash = passwordHasher;
                    }

                    user.Email = viewModel.User.Email;
                    user.FullName = viewModel.User.FullName;
                    user.UserName = viewModel.User.Email;

                    SetUserRoles(viewModel, user, database);

                    database.Entry(user).State = EntityState.Modified;
                    database.SaveChanges();

                    return RedirectToAction("List");
                }              
            }
            return View(viewModel);
        }

        private void SetUserRoles(EditUserViewModel model, ApplicationUser user, ProjectDbContext database)
        {
            var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();

            foreach (var role in model.Roles)
            {
                if (role.IsSelected)
                {
                    userManager.AddToRole(user.Id, role.Name);
                }
                else if (!role.IsSelected)
                {
                    userManager.RemoveFromRole(user.Id, role.Name);
                }
            }
        }

        private IList<Role> GetUserRoles(ApplicationUser user, ProjectDbContext database)
        {
            var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();

            var roles = database.Roles.Select(r => r.Name).OrderBy(r => r).ToList();

            var userRoles = new List<Role>();

            foreach (var roleName in roles)
            {
                var role = new Role { Name = roleName };
                if (userManager.IsInRole(user.Id,roleName))
                {
                    role.IsSelected = true;
                }

                userRoles.Add(role);
            }

            return userRoles;
        }


    }
}