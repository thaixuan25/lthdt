using System;

namespace LTHDT2.Models
{
    /// <summary>
    /// Base Entity class - Áp dụng OOP: Encapsulation và Abstraction
    /// Abstract class không thể khởi tạo trực tiếp
    /// Chứa các properties chung cho tất cả entities
    /// </summary>
    public abstract class BaseEntity
    {
        // Private fields - Đóng gói dữ liệu (Encapsulation)
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
        /// Ngày tạo - Protected setter: chỉ class con truy cập được
        /// Thể hiện tính đóng gói (Encapsulation)
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
        /// Protected constructor - chỉ class con có thể gọi
        /// Khởi tạo giá trị mặc định
        /// </summary>
        protected BaseEntity()
        {
            _createdDate = DateTime.Now;
        }

        /// <summary>
        /// Virtual method - có thể override trong class con (Polymorphism)
        /// </summary>
        public virtual bool IsValid()
        {
            return Id >= 0;
        }
    }
}

