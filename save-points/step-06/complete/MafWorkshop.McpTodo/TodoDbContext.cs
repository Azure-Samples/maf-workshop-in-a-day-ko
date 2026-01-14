using Microsoft.EntityFrameworkCore;

namespace MafWorkshop.McpTodo;

// TodoItem
public class TodoItem
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCompleted { get; set; } = false;
}

// TodoDbContext
public class TodoDbContext(DbContextOptions<TodoDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets or sets the collection of to-do items in the database.
    /// </summary>
    public DbSet<TodoItem> TodoItems { get; set; } = null!;

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TodoItem>().ToTable("TodoItems")
                                       .HasKey(t => t.Id);
        modelBuilder.Entity<TodoItem>().Property(t => t.Id).ValueGeneratedOnAdd();
        modelBuilder.Entity<TodoItem>().Property(t => t.Text).IsRequired().HasMaxLength(255);
        modelBuilder.Entity<TodoItem>().Property(t => t.IsCompleted).IsRequired();
    }
}

// ITodoRepository
public interface ITodoRepository
{
    Task<TodoItem> AddTodoItemAsync(TodoItem todoItem);
    Task<IEnumerable<TodoItem>> GetAllTodoItemsAsync();
    Task<TodoItem> UpdateTodoItemAsync(TodoItem todoItem);
    Task<TodoItem> CompleteTodoItemAsync(TodoItem todoItem);
    Task<TodoItem> DeleteTodoItemAsync(int id);
}

// TodoRepository
public class TodoRepository(TodoDbContext db) : ITodoRepository
{
    public async Task<TodoItem> AddTodoItemAsync(TodoItem todoItem)
    {
        await db.TodoItems.AddAsync(todoItem).ConfigureAwait(false);
        await db.SaveChangesAsync().ConfigureAwait(false);

        return todoItem;
    }

    public async Task<IEnumerable<TodoItem>> GetAllTodoItemsAsync()
    {
        var items = await db.TodoItems.ToListAsync().ConfigureAwait(false);

        return items;
    }

    public async Task<TodoItem> UpdateTodoItemAsync(TodoItem todoItem)
    {
        var record = await db.TodoItems.SingleOrDefaultAsync(p => p.Id == todoItem.Id)
                                       .ConfigureAwait(false);
        if (record is null)
        {
            return default!;
        }

        record.Text = todoItem.Text;

        await db.TodoItems.Where(p => p.Id == todoItem.Id)
                          .ExecuteUpdateAsync(p => p.SetProperty(x => x.Text, todoItem.Text))
                          .ConfigureAwait(false);

        await db.SaveChangesAsync().ConfigureAwait(false);

        return record;
    }

    public async Task<TodoItem> CompleteTodoItemAsync(TodoItem todoItem)
    {
        var record = await db.TodoItems.SingleOrDefaultAsync(p => p.Id == todoItem.Id)
                                       .ConfigureAwait(false);
        if (record is null)
        {
            return default!;
        }

        record.IsCompleted = todoItem.IsCompleted;

        await db.TodoItems.Where(p => p.Id == todoItem.Id)
                          .ExecuteUpdateAsync(p => p.SetProperty(x => x.IsCompleted, todoItem.IsCompleted))
                          .ConfigureAwait(false);

        await db.SaveChangesAsync().ConfigureAwait(false);

        return record;
    }

    public async Task<TodoItem> DeleteTodoItemAsync(int id)
    {
        var record = await db.TodoItems.SingleOrDefaultAsync(p => p.Id == id)
                                       .ConfigureAwait(false);
        if (record is null)
        {
            return default!;
        }

        await db.TodoItems.Where(p => p.Id == id)
                          .ExecuteDeleteAsync()
                          .ConfigureAwait(false);
        await db.SaveChangesAsync().ConfigureAwait(false);

        return record;
    }
}