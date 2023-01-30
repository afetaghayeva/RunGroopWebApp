using Microsoft.AspNetCore.Mvc;
using RunGroopWebApp.Interfaces;
using RunGroopWebApp.ViewModels;

namespace RunGroopWebApp.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;
        public UserController( IUserRepository userRepository)
        {
            _userRepository= userRepository;
        }
        [HttpGet("user")]
        public async Task<IActionResult> Index()
        {
            var users = await _userRepository.GetAllUsers();
            var result = new List<UserViewModel>();
            foreach (var user in users)
            {
                var userModel = new UserViewModel
                {
                    UserName = user.UserName,
                    Mileage = user.Mileage,
                    Pace = user.Pace,
                    Id = user.Id,
                    Image = user.ProfileImageUrl
                };
                result.Add(userModel);
            }
            return View(result);
        }

        public async Task<IActionResult> Detail(string id)
        {
            var user = await _userRepository.GetUserById(id);
            if (user == null) { return View("Error"); }
            var userModel = new UserDetailViewModel
            {
                Id = id,
                Mileage = user.Mileage,
                UserName = user.UserName,
                Pace = user.Pace
            };
            return View(userModel);
        }
    }
}
