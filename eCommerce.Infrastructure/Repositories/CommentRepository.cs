using eCommerce.Core.Entities;
using eCommerce.Core.Interfaces;
using eCommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Infrastructure.Repositories;

public class CommentRepository : GenericRepository<Comment>,  ICommentRepository
{
    public CommentRepository(AppDbContext context) : base(context) { }
    
    public async Task<Comment?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Where(c => c.Id == id && !EF.Property<bool>(c, "IsDeleted"))
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Comment>> GetCommentsByProductIdAsync(int productId)
    {
        return await _dbSet
            .Where(c => c.ProductId == productId && !EF.Property<bool>(c, "IsDeleted"))
            .Include(c => c.User)
            .ToListAsync();
    }

    public async Task<double> GetAverageRatingByProductIdAsync(int productId)
    {
        return await _dbSet
            .Where(c => c.ProductId == productId && !EF.Property<bool>(c, "IsDeleted"))
            .AverageAsync(c => (double?)c.Rating) ?? 0.0;
    }

    public async Task AddCommentAsync(Comment comment)
    {
        await _dbSet.AddAsync(comment);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCommentAsync(int id)
    {
        var comment = await _dbSet.FindAsync(id);
        if (comment != null)
        {
            var prop = typeof(Comment).GetProperty("IsDeleted");
            if (prop != null)
            {
                prop.SetValue(comment, true);
                _dbSet.Update(comment);
            }
            else
            {
                _dbSet.Remove(comment);
            }
            await _context.SaveChangesAsync();
        }
        }
    
}