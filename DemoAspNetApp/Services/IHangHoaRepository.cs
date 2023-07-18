using DemoAspNetApp.Models;

namespace DemoAspNetApp.Services
{
    public interface IHangHoaRepository
    {
        List<HangHoaModel> GetAll(string search, double? from, double? to, string sortBy, int page = 1);
        HangHoaVM Add(HangHoaModel hangHoa);
    }
}
