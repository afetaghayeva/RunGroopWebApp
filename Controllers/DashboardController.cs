using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;
using RunGroopWebApp.Data;
using RunGroopWebApp.Interfaces;
using RunGroopWebApp.Models;
using RunGroopWebApp.ViewModels;

namespace RunGroopWebApp.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IDashboardRepository _dashboardRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPhotoService _photoServices;
        public DashboardController(IDashboardRepository dashboardRepository, IHttpContextAccessor httpContextAccessor, IPhotoService photoServices)
        {
            _dashboardRepository = dashboardRepository;
            _httpContextAccessor = httpContextAccessor;
            _photoServices = photoServices;
        }
        private void MapUserEdit(AppUser user, EditUserDashboardViewModel editUserModel, ImageUploadResult photoResult)
        {
            user.Id= editUserModel.Id;
            user.Pace=editUserModel.Pace;
            user.Mileage=editUserModel.Mileage;
            user.ProfileImageUrl = photoResult.Url.ToString();
            user.City= editUserModel.City;
            user.State=editUserModel.State;
        }
        public async Task<IActionResult> Index()
        {
            var userClubs = await _dashboardRepository.GetAllUserClubs();
            var userRaces = await _dashboardRepository.GetAllUserRaces();
            var userModel = new DashboardViewModel
            {
                Clubs = userClubs,
                Races = userRaces
            };
            return View(userModel);
        }

        public async Task<IActionResult> EditUserProfile()
        {
            var curUserId = _httpContextAccessor.HttpContext?.User.GetUserId();
            var user = await _dashboardRepository.GetUserById(curUserId);
            if (user == null) return View("Error");
            var editUserModel = new EditUserDashboardViewModel
            {
                Id = curUserId,
                Pace = user.Pace,
                Mileage = user.Mileage,
                ProfilImageUrl = user.ProfileImageUrl,
                City = user.City,
                State = user.State
            };
            return View(editUserModel);
        }
        [HttpPost]
        public async Task<IActionResult> EditUserProfile(EditUserDashboardViewModel editUserModel)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Failed to edit this user profile");
                return View(editUserModel);
            }
            var user = await _dashboardRepository.GetByIdNoTracking(editUserModel.Id);
            if (String.IsNullOrEmpty(user.ProfileImageUrl))
            {
                var photoResult = _photoServices.AddPhotoAsync(editUserModel.Image);
                MapUserEdit(user, editUserModel, await photoResult);
                _dashboardRepository.Update(user);
                return RedirectToAction("Index");
            }
            else
            {
                try
                {
                    _photoServices.DeletePhotoAsync(user.ProfileImageUrl);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Couldn't delete photo");
                }
                var photoResult = _photoServices.AddPhotoAsync(editUserModel.Image);
                MapUserEdit(user, editUserModel, await photoResult);
                _dashboardRepository.Update(user);
                return RedirectToAction("Index");
            }
        }

    }
}
