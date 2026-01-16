using System;
using System.Linq;
using LTHDT2.DataAccess.Repositories;
using LTHDT2.Models;
using LTHDT2.Utils;

namespace LTHDT2.Services
{
    /// <summary>
    /// Headcount Service - Quản lý định biên nhân sự
    /// Kế thừa BaseService
    /// </summary>
    public class HeadcountService : BaseService, IHeadcountService
    {
        private readonly HeadcountRepository _headcountRepository;
        private readonly DepartmentRepository _departmentRepository;
        private readonly PositionRepository _positionRepository;

        public HeadcountService()
        {
            _headcountRepository = new HeadcountRepository();
            _departmentRepository = new DepartmentRepository();
            _positionRepository = new PositionRepository();
        }

        /// <summary>
        /// Kiểm tra định biên khi tạo tin tuyển dụng
        /// </summary>
        public HeadcountResult CheckHeadcount(int departmentId, int positionId, int requestedCount)
        {
            try
            {
                Log.Error($"Checking headcount: Dept={departmentId}, Pos={positionId}, Requested={requestedCount}");

                var currentYear = DateTime.Now.Year;
                var headcount = _headcountRepository.GetAll()
                    .FirstOrDefault(h => h.DepartmentId == departmentId 
                                      && h.PositionId == positionId 
                                      && h.Year == currentYear);

                if (headcount == null)
                {
                    return new HeadcountResult
                    {
                        IsApproved = false,
                        Remaining = 0,
                        Requested = requestedCount,
                        Approved = 0,
                        Filled = 0,
                        Pending = requestedCount,
                        Message = $"Chưa có định biên cho phòng ban và vị trí này trong năm {currentYear}"
                    };
                }

                int remaining = headcount.ApprovedCount - headcount.FilledCount;
                bool isApproved = remaining >= requestedCount;

                return new HeadcountResult
                {
                    IsApproved = isApproved,
                    Remaining = remaining,
                    Requested = requestedCount,
                    Approved = headcount.ApprovedCount,
                    Filled = headcount.FilledCount,
                    Pending = remaining - requestedCount,
                    Message = isApproved 
                        ? $"OK: Đủ định biên (còn {remaining}/{headcount.ApprovedCount})" 
                        : $"CẢNH BÁO: Vượt định biên! Chỉ còn {remaining}/{headcount.ApprovedCount} vị trí"
                };
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR checking headcount: {ex.Message}");
                return new HeadcountResult
                {
                    IsApproved = false,
                    Message = $"Lỗi kiểm tra định biên: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Lấy số lượng còn lại có thể tuyển
        /// </summary>
        public int GetRemainingHeadcount(int departmentId, int positionId, int year)
        {
            try
            {
                var headcount = _headcountRepository.GetAll()
                    .FirstOrDefault(h => h.DepartmentId == departmentId 
                                      && h.PositionId == positionId 
                                      && h.Year == year);

                if (headcount == null)
                    return 0;

                return Math.Max(0, headcount.ApprovedCount - headcount.FilledCount);
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR getting remaining headcount: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Phê duyệt định biên (Chỉ Admin/HRManager)
        /// </summary>
        public bool ApproveHeadcount(int departmentId, int positionId, int count, int year, int approvedBy)
        {
            try
            {
                // Kiểm tra quyền
                if (!HasPermission("HRManager"))
                {
                    Log.Error("ERROR: User không có quyền phê duyệt định biên");
                    return false;
                }

                // Kiểm tra xem đã có định biên chưa
                var existing = _headcountRepository.GetAll()
                    .FirstOrDefault(h => h.DepartmentId == departmentId 
                                      && h.PositionId == positionId 
                                      && h.Year == year);

                if (existing != null)
                {
                    // Cập nhật
                    existing.ApprovedCount = count;
                    existing.UpdatedDate = DateTime.Now;
                    return _headcountRepository.Update(existing);
                }
                else
                {
                    // Tạo mới
                    var headcount = new Headcount
                    {
                        DepartmentId = departmentId,
                        PositionId = positionId,
                        Year = year,
                        ApprovedCount = count,
                        FilledCount = 0,
                        CreatedDate = DateTime.Now
                    };

                    var id = _headcountRepository.Add(headcount);
                    Log.Error($"Created headcount ID={id}");
                    return id > 0;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR approving headcount: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Cập nhật số lượng đã tuyển
        /// </summary>
        public bool UpdateFilledCount(int departmentId, int positionId, int year, int newFilledCount)
        {
            try
            {
                var headcount = _headcountRepository.GetAll()
                    .FirstOrDefault(h => h.DepartmentId == departmentId 
                                      && h.PositionId == positionId 
                                      && h.Year == year);

                if (headcount == null)
                {
                    Log.Error($"ERROR: Headcount not found for Dept={departmentId}, Pos={positionId}, Year={year}");
                    return false;
                }

                if (newFilledCount > headcount.ApprovedCount)
                {
                    Log.Error($"WARNING: Filled count ({newFilledCount}) > Approved count ({headcount.ApprovedCount})");
                }

                headcount.FilledCount = newFilledCount;
                headcount.UpdatedDate = DateTime.Now;

                return _headcountRepository.Update(headcount);
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR updating filled count: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Tăng số lượng đã tuyển (khi hiring thành công)
        /// </summary>
        public bool IncrementFilledCount(int departmentId, int positionId, int year, int incrementBy = 1)
        {
            try
            {
                var headcount = _headcountRepository.GetAll()
                    .FirstOrDefault(h => h.DepartmentId == departmentId 
                                      && h.PositionId == positionId 
                                      && h.Year == year);

                if (headcount == null)
                {
                    Log.Error($"ERROR: Headcount not found");
                    return false;
                }

                headcount.FilledCount += incrementBy;
                headcount.UpdatedDate = DateTime.Now;

                Log.Error($"Incremented filled count: {headcount.FilledCount - incrementBy} -> {headcount.FilledCount}");
                return _headcountRepository.Update(headcount);
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR incrementing filled count: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Lấy báo cáo tổng hợp định biên
        /// </summary>
        public HeadcountSummary GetHeadcountSummary(int year)
        {
            try
            {
                var headcounts = _headcountRepository.GetAll()
                    .Where(h => h.Year == year)
                    .ToList();

                return new HeadcountSummary
                {
                    Year = year,
                    TotalApproved = headcounts.Sum(h => h.ApprovedCount),
                    TotalFilled = headcounts.Sum(h => h.FilledCount),
                    TotalRemaining = headcounts.Sum(h => h.ApprovedCount - h.FilledCount),
                    DepartmentCount = headcounts.Select(h => h.DepartmentId).Distinct().Count(),
                    PositionCount = headcounts.Select(h => h.PositionId).Distinct().Count()
                };
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR getting summary: {ex.Message}");
                return new HeadcountSummary { Year = year };
            }
        }
    }

    /// <summary>
    /// Class tổng hợp báo cáo định biên
    /// </summary>
    public class HeadcountSummary
    {
        public int Year { get; set; }
        public int TotalApproved { get; set; }
        public int TotalFilled { get; set; }
        public int TotalRemaining { get; set; }
        public int DepartmentCount { get; set; }
        public int PositionCount { get; set; }
        public double FillRate => TotalApproved > 0 ? (double)TotalFilled / TotalApproved * 100 : 0;
    }
}

