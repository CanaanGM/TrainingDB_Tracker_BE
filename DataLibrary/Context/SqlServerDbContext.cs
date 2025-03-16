using Microsoft.EntityFrameworkCore;

namespace DataLibrary.Context;

public class SqlServerDbContext : DbContext
{
	public SqlServerDbContext(DbContextOptions<SqlServerDbContext> options) : base(options)
	{
		
	}
}