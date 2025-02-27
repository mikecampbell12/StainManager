using StainManager.Domain.Common;
using StainManager.Domain.Textures;

namespace StainManager.Infrastructure.Repositories;

public class TextureRepository(
    ApplicationDbContext context)
    : ITextureRepository
{
    public async Task<List<Texture>> GetAllTexturesAsync(bool isActive = true)
    {
        var result = await context.Textures
            .Where(c => c.IsActive == isActive)
            .ToListAsync();
        
        return result;
    }

    public async Task<PaginatedList<Texture>> GetTexturesForManagementAsync(string? searchQuery = "",
        int pageNumber = 1,
        int pageSize = 10,
        bool isActive = true,
        Sort? sort = null,
        List<Filter>? filters = null)
    {
        var query = context.Textures
            .Where(c => c.IsActive == isActive)
            .OrderBy(c => c.Name)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchQuery))
            query = query.Where(c =>
                c.Name.Contains(searchQuery));

        query = query.ApplySorting(sort);
        query = query.ApplyFilters(filters);

        var result = await query.PaginatedListAsync(pageNumber, pageSize);

        return result;
    }

    public async Task<Texture?> GetTextureByIdAsync(
        int id,
        bool includeInactive = false)
    {
        var textureQuery = context.Textures
            .Where(c => c.Id == id);

        if (!includeInactive)
            textureQuery = textureQuery.Where(c => c.IsActive);

        var texture = await textureQuery
            .FirstOrDefaultAsync();

        return texture;
    }

    public async Task<Texture> CreateTextureAsync(Texture texture)
    {
        context.Textures.Add(texture);
        
        await context.SaveChangesAsync();
        
        return texture;
    }

    public async Task<bool> UpdateTextureImageLocationsAsync(
        int id,
        string? fullImageLocation,
        string? thumbnailLocation)
    {
        var textureToUpdate = await GetTextureByIdAsync(id);

        Guard.Against.NotFound(id, textureToUpdate);
        
        textureToUpdate.FullImageLocation = fullImageLocation;
        textureToUpdate.ThumbnailImageLocation = thumbnailLocation;

        return await context.SaveChangesAsync() > 0;
    }

    public async Task<Texture> UpdateTextureAsync(Texture texture)
    {
        var textureToUpdate = await GetTextureByIdAsync(texture.Id);
        
        Guard.Against.NotFound(texture.Id, textureToUpdate);
        
        textureToUpdate.UpdatedBy = "System";
        textureToUpdate.UpdatedDateTime = DateTime.UtcNow;
        textureToUpdate.Name = texture.Name;

        await context.SaveChangesAsync();
        
        return textureToUpdate;
    }

    public async Task<bool> DeleteTextureAsync(int id)
    {
        var textureToDelete = await GetTextureByIdAsync(id);

        Guard.Against.NotFound(id, textureToDelete);

        textureToDelete.IsActive = false;
        textureToDelete.UpdatedBy = "System";
        textureToDelete.UpdatedDateTime = DateTime.UtcNow;

        return await context.SaveChangesAsync() > 0;    
    }

    public async Task<bool> RestoreTextureAsync(int id)
    {
        var textureToDelete = await GetTextureByIdAsync(id, true);

        Guard.Against.NotFound(id, textureToDelete);

        textureToDelete.IsActive = true;
        textureToDelete.UpdatedBy = "System";
        textureToDelete.UpdatedDateTime = DateTime.UtcNow;

        return await context.SaveChangesAsync() > 0;    
    }
}