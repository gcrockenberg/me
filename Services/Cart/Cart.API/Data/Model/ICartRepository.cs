namespace Me.Services.Cart.API.Model;

public interface ICartRepository
{
    Task<CustomerCart> GetCartAsync(string customerId);
    IEnumerable<string> GetUsers();
    Task<CustomerCart> UpdateCartAsync(CustomerCart cart);
    Task<bool> DeleteCartAsync(string id);
}