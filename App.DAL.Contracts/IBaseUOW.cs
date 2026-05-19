namespace App.DAL.Contracts;

// Lecture 21 exact: IBaseUOW owns only the save operations
public interface IBaseUOW
{
    Task<int> SaveChangesAsync();
    int SaveChanges();
}
