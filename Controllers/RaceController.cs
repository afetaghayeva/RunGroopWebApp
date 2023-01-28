using Microsoft.AspNetCore.Mvc;
using RunGroopWebApp.Interfaces;
using RunGroopWebApp.Models;
using RunGroopWebApp.ViewModels;

namespace RunGroopWebApp.Controllers
{
    public class RaceController : Controller
    {
        private readonly IRaceRepository _raceRepository;
        private readonly IPhotoService _photoService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public RaceController(IRaceRepository raceRepository, IPhotoService photoService, IHttpContextAccessor contextAccessor)
        {
            _raceRepository = raceRepository;
            _photoService = photoService;
            _httpContextAccessor = contextAccessor;
        }
        public async Task<IActionResult> Index()
        {
            var races = await _raceRepository.GetAll();
            return View(races);
        }
        public async Task<IActionResult> Detail(int id)
        {
            var race = await _raceRepository.GetByIdAsync(id);
            return View(race);
        }
        public async Task<IActionResult> Create()
        {
            var curUserId = _httpContextAccessor.HttpContext?.User.GetUserId();
            var createRaceModel = new CreateRaceViewModel { AppUserId = curUserId };
            return View(createRaceModel);
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateRaceViewModel raceModel)
        {
            if (ModelState.IsValid)
            {
                var result = await _photoService.AddPhotoAsync(raceModel.Image);
                var race = new Race
                {
                    Title = raceModel.Title,
                    Description = raceModel.Description,
                    Image = result.Url.ToString(),
                    AppUserId=raceModel.AppUserId,
                    Address = new Address
                    {
                        City = raceModel.Address.City,
                        State = raceModel.Address.State,
                        Street = raceModel.Address.Street
                    }
                };
                _raceRepository.Add(race);
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "Photo Upload Failed");
            }
            return View(raceModel);
        }
        public async Task<IActionResult> Edit(int id)
        {
            var race = await _raceRepository.GetByIdAsync(id);
            if (race == null) return View("Error");
            var raceModel = new EditRaceViewModel
            {
                Title = race.Title,
                Description = race.Description,
                AddressId = race.AddressId,
                Address = race.Address,
                Url = race.Image,
                RaceCategory = race.RaceCategory
            };
            return View(raceModel);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(EditRaceViewModel raceModel, int id)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Failed to edit");
                return View("Edit", raceModel);
            }
            var userRace = await _raceRepository.GetByIdAsyncNoTracking(id);
            if (userRace != null)
            {
                try
                {
                    await _photoService.DeletePhotoAsync(userRace.Image);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Couldn't delete photo ");
                    return View("Edit", raceModel);
                }
                var photoResult = await _photoService.AddPhotoAsync(raceModel.Image);
                var race = new Race
                {
                    Id = id,
                    Title = raceModel.Title,
                    Description = raceModel.Description,
                    Address = raceModel.Address,
                    AddressId = raceModel.AddressId,
                    Image = photoResult.Url.ToString(),
                    RaceCategory = raceModel.RaceCategory
                };
                _raceRepository.Update(race);
                return RedirectToAction("Index");
            }
            else
            {
                return View(raceModel);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var raceDetail = await _raceRepository.GetByIdAsync(id);
            if (raceDetail == null) return View("Error");
            return View(raceDetail);
        }
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteRace(int id)
        {
            var raceDetail = await _raceRepository.GetByIdAsync(id);
            if (raceDetail == null) return View("Error");
            _raceRepository.Delete(raceDetail);
            return RedirectToAction("Index");
        }
    }
}
