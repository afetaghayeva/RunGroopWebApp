using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RunGroopWebApp.Data;
using RunGroopWebApp.Interfaces;
using RunGroopWebApp.Models;
using RunGroopWebApp.ViewModels;

namespace RunGroopWebApp.Controllers
{
    public class ClubController : Controller
    {
        private readonly IClubRepository _clubRepository;
        private readonly IPhotoService _photoService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ClubController(IClubRepository clubRepository,IPhotoService photoService, IHttpContextAccessor httpContextAccessor)
        {
            _clubRepository = clubRepository;
            _photoService = photoService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> Index()
        {
            var clubs = await _clubRepository.GetAll();
            return View(clubs);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var club = await _clubRepository.GetByIdAsync(id);
            return View(club);
        }
        public async Task<IActionResult> Create()
        {
            var curUserId = _httpContextAccessor.HttpContext?.User.GetUserId();
            var createClubModel = new CreateClubViewModel { AppUserId = curUserId };
            return View(createClubModel);
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateClubViewModel clubModel)
        {
            if (ModelState.IsValid)
            {
                var result = await _photoService.AddPhotoAsync(clubModel.Image);
                var club = new Club
                {
                    Title = clubModel.Title,
                    Description = clubModel.Description,
                    Image = result.Url.ToString(),
                    AppUserId=clubModel.AppUserId,
                    Address = new Address
                    {
                        City = clubModel.Address.City,
                        State = clubModel.Address.State,
                        Street = clubModel.Address.Street
                    }
                };
                _clubRepository.Add(club);
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "Photo upload failed");
            }
            return View(clubModel);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var club = await _clubRepository.GetByIdAsync(id);
            if(club==null) return View("Error");
            var clubModel = new EditClubViewModel
            {
                Title = club.Title,
                Description = club.Description,
                AddressId = club.AddressId,
                Address = club.Address,
                Url = club.Image,
                ClubCategory = club.ClubCategory
            };
            return View(clubModel);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(EditClubViewModel clubModel, int id)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Failed to edit");
                return View("Edit", clubModel);
            }
            var userClub=await _clubRepository.GetByIdAsyncNoTracking(id);
            if (userClub != null)
            {
                try
                {
                    await _photoService.DeletePhotoAsync(userClub.Image);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Couldn't delete photo ");
                    return View("Edit", clubModel);
                }
                var photoResult = await _photoService.AddPhotoAsync(clubModel.Image);
                var club = new Club
                {
                    Id = id,
                    Title = clubModel.Title,
                    Description = clubModel.Description,
                    Address = clubModel.Address,
                    AddressId = clubModel.AddressId,
                    Image = photoResult.Url.ToString(),
                    ClubCategory=userClub.ClubCategory
                };
                _clubRepository.Update(club);
                return RedirectToAction("Index");
            }
            else
            {
                return View(clubModel);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var clubDetail = await _clubRepository.GetByIdAsync(id);
            if (clubDetail == null) return View("Error");
            return View(clubDetail);
        }
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteClub(int id)
        {
            var clubDetail = await _clubRepository.GetByIdAsync(id);
            if (clubDetail == null) return View("Error");
            _clubRepository.Delete(clubDetail);
            return RedirectToAction("Index");
        }
    }
}
