using System;

namespace LTHDT2.Models
{
    /// <summary>
    /// Base Entity class
    /// </summary>
    public abstract class BaseEntity
    {
        // Private fields
        private int _id;
        private DateTime _createdDate;
        private DateTime? _updatedDate;

        /// <summary>
        /// Primary Key - Validation trong setter
        /// </summary>
        public int Id
        {
            get => _id;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Id không thể âm");
                _id = value;
            }
        }

        /// <summary>
        /// Ngày tạo
        /// </summary>
        public DateTime CreatedDate
        {
            get => _createdDate;
            set => _createdDate = value;
        }

        /// <summary>
        /// Ngày cập nhật gần nhất
        /// </summary>
        public DateTime? UpdatedDate
        {
            get => _updatedDate;
            set => _updatedDate = value;
        }

        /// <summary>
        /// Khởi tạo giá trị mặc định
        /// </summary>
        protected BaseEntity()
        {
            _createdDate = DateTime.Now;
        }

        /// <summary>
        /// Virtual method
        /// </summary>
        public virtual bool IsValid()
        {
            return Id >= 0;
        }
    }
}

