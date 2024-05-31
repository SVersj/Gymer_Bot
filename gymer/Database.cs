using gymer.Models;
using Microsoft.EntityFrameworkCore;
using gymer.Models;
using System.Collections.Generic;
using System.Linq;


public class Database : DbContext
{
    public DbSet<Exercise> Exercises { get; set; }
    public DbSet<Reminder> Reminders { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=fitnessBot.db");
    }

    public void AddExercise(Exercise exercise)
    {
        Exercises.Add(exercise);
        SaveChanges();
    }

    public List<Exercise> GetExercises(long userId)
    {
        return Exercises.Where(e => e.UserId == userId).ToList();
    }

    public void AddReminder(Reminder reminder)
    {
        Reminders.Add(reminder);
        SaveChanges();
    }

    public List<Reminder> GetReminders(long userId)
    {
        return Reminders.Where(r => r.UserId == userId).ToList();
    }
}
