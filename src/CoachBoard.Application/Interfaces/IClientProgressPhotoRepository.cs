using CoachBoard.Domain.Entities;

namespace CoachBoard.Application.Interfaces;

public interface IClientProgressPhotoRepository : IRepository<ClientProgressPhoto>
{
    Task<IEnumerable<ClientProgressPhoto>> GetByClientAsync(int clientId);
    Task<ClientProgressPhoto?> GetByClientAndIdAsync(int clientId, int photoId);
}
