using System.Collections.Generic;
using System.Threading.Tasks;

namespace vp.services
{
    public interface IValidationService
    {
        Task<bool> InitializeValidationService();
        Task<Dictionary<string, string>> ValidateEntity(object entity, string validatorName);
    }
}
