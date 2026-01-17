using System;
using System.Windows.Forms;
using LTHDT2.Forms;
using LTHDT2.DataAccess;
using LTHDT2.Utils;

namespace LTHDT2;

static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--setup")
        {
            Console.WriteLine("Chế độ Setup - Console Utility");
            Console.WriteLine();
            HashPasswordConsole.RunInteractiveMenu();
            return;
        }

        ApplicationConfiguration.Initialize();

        try
        {
            if (!DatabaseConnection.TestConnection(out string errorMessage))
            {
                MessageBox.Show(
                    $"Không thể kết nối database!\n\nLỗi: {errorMessage}\n\n" +
                    "Vui lòng kiểm tra:\n" +
                    "1. MySQL Server đã chạy chưa?\n" +
                    "2. Connection string trong App.config\n" +
                    "3. Database đã được tạo chưa?\n\n" +
                    "💡 TIP: Chạy ứng dụng với tham số --setup để tạo user:\n" +
                    "   dotnet run --setup",
                    "Lỗi Database",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            using (var loginForm = new LoginForm())
            {
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    Application.Run(new MainForm());
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Lỗi khởi động ứng dụng:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                "Lỗi",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }
}