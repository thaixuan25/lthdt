using System;
using System.Collections.Generic;
using LTHDT2.Models;

namespace LTHDT2.DataAccess.Repositories
{
    /// <summary>
    /// Generic Repository Interface - Áp dụng Abstraction
    /// Định nghĩa "contract" cho tất cả Repository
    /// T phải là BaseEntity hoặc class kế thừa từ BaseEntity
    /// </summary>
    /// <typeparam name="T">Entity type phải kế thừa từ BaseEntity</typeparam>
    public interface IRepository<T> where T : BaseEntity
    {
        /// <summary>
        /// Lấy entity theo ID
        /// </summary>
        T? GetById(int id);

        /// <summary>
        /// Lấy tất cả entities
        /// </summary>
        IEnumerable<T> GetAll();

        /// <summary>
        /// Thêm mới entity
        /// </summary>
        /// <returns>ID của entity vừa thêm</returns>
        int Add(T entity);

        /// <summary>
        /// Cập nhật entity
        /// </summary>
        /// <returns>True nếu thành công</returns>
        bool Update(T entity);

        /// <summary>
        /// Xóa entity theo ID
        /// </summary>
        /// <returns>True nếu thành công</returns>
        bool Delete(int id);

        /// <summary>
        /// Tìm kiếm entities theo điều kiện
        /// </summary>
        IEnumerable<T> Find(Func<T, bool> predicate);

        /// <summary>
        /// Đếm số lượng entities
        /// </summary>
        int Count();

        /// <summary>
        /// Kiểm tra entity có tồn tại không
        /// </summary>
        bool Exists(int id);
    }
}

