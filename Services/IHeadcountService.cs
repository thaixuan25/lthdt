namespace LTHDT2.Services
{
    /// <summary>
    /// Interface Headcount Service
    /// </summary>
    public interface IHeadcountService
    {
        /// <summary>
        /// Kiểm tra định biên khi tạo tin tuyển dụng
        /// </summary>
        HeadcountResult CheckHeadcount(int departmentId, int positionId, int requestedCount);

        /// <summary>
        /// Lấy số lượng còn lại có thể tuyển
        /// </summary>
        int GetRemainingHeadcount(int departmentId, int positionId, int year);

        /// <summary>
        /// Phê duyệt định biên
        /// </summary>
        bool ApproveHeadcount(int departmentId, int positionId, int count, int year, int approvedBy);

        /// <summary>
        /// Cập nhật số lượng đã tuyển
        /// </summary>
        bool UpdateFilledCount(int departmentId, int positionId, int year, int newFilledCount);
    }

    /// <summary>
    /// Result class cho việc kiểm tra định biên
    /// </summary>
    public class HeadcountResult
    {
        public bool IsApproved { get; set; }
        public int Remaining { get; set; }
        public int Requested { get; set; }
        public int Approved { get; set; }
        public int Filled { get; set; }
        public int Pending { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}

