using System;

namespace LTHDT2.Models
{
    /// <summary>
    /// Model Headcount - Định biên nhân sự
    /// </summary>
    public class Headcount : BaseEntity
    {
        private int _departmentId;
        private int _positionId;
        private int _approvedCount;
        private int _filledCount;
        private int _year;
        private DateTime _approvedDate;
        private int _approvedBy;

        public int DepartmentId
        {
            get => _departmentId;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Phải chọn phòng ban");
                _departmentId = value;
            }
        }

        public int PositionId
        {
            get => _positionId;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Phải chọn vị trí");
                _positionId = value;
            }
        }

        public int ApprovedCount
        {
            get => _approvedCount;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Số lượng phê duyệt không thể âm");
                _approvedCount = value;
            }
        }

        public int FilledCount
        {
            get => _filledCount;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Số lượng đã tuyển không thể âm");
                if (value > _approvedCount)
                    throw new ArgumentException("Số lượng đã tuyển không thể vượt số lượng phê duyệt");
                _filledCount = value;
            }
        }

        public int Year
        {
            get => _year;
            set
            {
                if (value < 2000 || value > 2100)
                    throw new ArgumentException("Năm không hợp lệ");
                _year = value;
            }
        }

        public DateTime ApprovedDate
        {
            get => _approvedDate;
            set => _approvedDate = value;
        }

        public int ApprovedBy
        {
            get => _approvedBy;
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Phải có người phê duyệt");
                _approvedBy = value;
            }
        }

        // ========== ALIAS PROPERTIES ==========
        
        /// <summary>
        /// Alias: FilledCount = CurrentCount (để tương thích)
        /// </summary>
        public int CurrentCount
        {
            get => _filledCount;
            set => _filledCount = value;
        }

        // Navigation properties
        public Department? Department { get; set; }
        public Position? Position { get; set; }
        public Employee? Approver { get; set; }

        public Headcount()
        {
            _year = DateTime.Now.Year;
            _approvedDate = DateTime.Now;
        }

        public int GetRemainingCount()
        {
            return Math.Max(0, _approvedCount - _filledCount);
        }

        public bool HasRemaining()
        {
            return _filledCount < _approvedCount;
        }

        public decimal GetFilledPercentage()
        {
            if (_approvedCount == 0) return 0;
            return (decimal)_filledCount / _approvedCount * 100;
        }
    }
}

