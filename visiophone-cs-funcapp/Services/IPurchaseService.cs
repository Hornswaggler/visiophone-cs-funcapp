using System.Collections.Generic;
using System.Threading.Tasks;
using vp.models;

namespace vp.services
{
    public interface IPurchaseService
    {
        Task<Purchase> AddPurchase(Purchase purchase);
        Task<List<Purchase>> GetPurchases(string accountId);
    }
}
