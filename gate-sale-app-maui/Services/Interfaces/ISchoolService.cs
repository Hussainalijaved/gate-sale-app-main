using GateSale.Models;

namespace GateSale.Services.Interfaces
{
    public interface ISchoolService
    {
        // School Management
        Task<List<School>> GetAllSchoolsAsync();
        Task<School?> GetSchoolByIdAsync(string schoolId);
        Task<School?> GetCurrentSchoolAsync();
        Task<bool> SetCurrentSchoolAsync(string schoolId);
        Task<School?> GetSchoolByNameAsync(string name);

        // School Information
        Task<int> GetActiveStudentCountAsync(string schoolId);
        Task<List<string>> GetSchoolBuildingsAsync(string schoolId);
        Task<List<Grade>> GetSchoolGradesAsync(string schoolId);

        // School Verification
        Task<bool> IsSchoolVerifiedAsync(string schoolId);
        Task<bool> VerifySchoolAsync(string schoolId);

        // School Request
        Task<ApiResponse<bool>> RequestSchoolAsync(string schoolName, string city, string requesterEmail, string? contactPerson);
    }
}
