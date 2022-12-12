using System.Threading.Tasks;
using vp.models;

namespace vp.Services
{
    public interface ICheckoutSessionService
    {
        Task<CheckoutSession> CreateCheckoutSession(CheckoutSession checkoutSession);
    }
}
