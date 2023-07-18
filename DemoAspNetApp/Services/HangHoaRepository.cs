using DemoAspNetApp.Data;
using DemoAspNetApp.Models;

namespace DemoAspNetApp.Services
{
    public class HangHoaRepository : IHangHoaRepository
    {
        private readonly DatabaseContext _context;
        private readonly ILoaiRepository _loaiRepository;
        public static int PAGE_SIZE { get; set; } = 5;

        public HangHoaRepository(DatabaseContext databaseContext, ILoaiRepository loaiRepository)
        {
            _context = databaseContext;
            _loaiRepository = loaiRepository;
        }

        public List<HangHoaModel> GetAll(string search, double? from, double? to, string sortBy, int page = 1)
        {
            var allProducts = _context.HangHoas.AsQueryable();

            #region Filtering
            if (!string.IsNullOrEmpty(search))
            {
                allProducts = allProducts.Where(hh => hh.TenHh.Contains(search));
            }
            if(from.HasValue)
            {
                allProducts = allProducts.Where(hh => hh.DonGia >= from);
            }
            if (to.HasValue)
            {
                allProducts = allProducts.Where(hh => hh.DonGia <= to);
            }
            #endregion

            #region Sorting
            //allProducts = allProducts.OrderBy(hh => hh.TenHh);

            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy)
                {
                    case "tenhh_desc": allProducts = allProducts.OrderByDescending(hh => hh.TenHh); 
                        break;
                    case "gia_asc":
                        allProducts = allProducts.OrderBy(hh => hh.DonGia);
                        break;
                    case "gia_desc":
                        allProducts = allProducts.OrderByDescending(hh => hh.DonGia);
                        break;
                }
            }
            #endregion

            #region Paging
            allProducts = allProducts.Skip((page - 1) * PAGE_SIZE).Take(PAGE_SIZE);
            #endregion


            var result = allProducts.Select(hh => new HangHoaModel { 
                MaHangHoa = hh.MaHh,
                TenHangHoa = hh.TenHh,
                MoTa = hh.MoTa,
                DonGia = hh.DonGia,
                MaLoai = hh.Loai.MaLoai,
                TenLoai = hh.Loai.TenLoai
            });

            return result.ToList();
        }

        public HangHoaVM Add(HangHoaModel hangHoa)
        {
            Loai loaiHangHoa = _context.Loais.SingleOrDefault(l => l.MaLoai == hangHoa.MaLoai);
            if(loaiHangHoa == null) {
                LoaiModel loai = new LoaiModel
                {
                    TenLoai = hangHoa.TenLoai,
                };
                _loaiRepository.Add(loai);
                loaiHangHoa = _context.Loais.SingleOrDefault(l => l.TenLoai == hangHoa.TenLoai);
            }
            var _hangHoa = new Data.HangHoa
            {
                MaHh = Guid.NewGuid(),
                TenHh = hangHoa.TenHangHoa,
                MoTa = hangHoa.MoTa,
                DonGia = hangHoa.DonGia,
                GiamGia = 1,
                Loai = loaiHangHoa,
            };
            _context.Add(_hangHoa);
            _context.SaveChanges();
            return new HangHoaVM
            {
                TenHangHoa = _hangHoa.TenHh,
                DonGia = _hangHoa.DonGia,
            };
        }
    }
}
