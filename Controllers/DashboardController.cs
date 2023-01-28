using Microsoft.AspNetCore.Mvc;
using RunGroopWebApp.Data;
using RunGroopWebApp.Interfaces;
using RunGroopWebApp.ViewModels;

namespace RunGroopWebApp.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IDashboardRepository _dashboardRepository;
        public DashboardController(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
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
    }
}
