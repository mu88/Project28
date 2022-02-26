﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BusinessServices;
using DTO.LifePoint;
using Entities;
using Microsoft.EntityFrameworkCore;

// ReSharper disable All - There are so many R# issues due to the usage of EF Core.

// EF Core will handle object initialization
#pragma warning disable 8618

namespace Persistence;

// TODO mu88: Make this internal as soon as seeding of test data is not longer necessary
public class Storage : DbContext, IStorage
{
    private static string _imageDirectory = Path.Combine(UserDirectory, "images");
    private static string _dbDirectory = Path.Combine(UserDirectory, "db");
    private readonly IFileSystem _fileSystem;

    /// <inheritdoc />
    public Storage(DbContextOptions<Storage> options, IFileSystem fileSystem)
        : base(options) =>
        _fileSystem = fileSystem;

    /// <inheritdoc />
    public IQueryable<LifePoint> LifePoints => LifePointsInStorage;

    /// <inheritdoc />
    public IQueryable<Person> Persons => PersonsInStorage;

    public DbSet<LifePoint> LifePointsInStorage { get; set; }

    public DbSet<Person> PersonsInStorage { get; set; }

    internal static string DatabasePath => Path.Combine(_dbDirectory, "ThisIsYourLife.db");

    internal static string UserDirectory => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");

    /// <inheritdoc />
    public async Task<T?> FindAsync<T>(Guid id)
        where T : class => await Set<T>().FindAsync(id);

    /// <inheritdoc />
    public T? Find<T>(Guid id)
        where T : class =>
        Set<T>().Find(id);

    /// <inheritdoc />
    public async Task<T> AddItemAsync<T>(T itemToAdd)
        where T : class => (await Set<T>().AddAsync(itemToAdd)).Entity;

    /// <inheritdoc />
    public void RemoveItem<T>(T itemToDelete)
        where T : class => Set<T>().Remove(itemToDelete);

    /// <inheritdoc />
    public async Task SaveAsync() => await base.SaveChangesAsync();

    /// <inheritdoc />
    public async Task<Guid> StoreImageAsync(Person owner, ImageToCreate newImage)
    {
        // TODO mu88: Make images smaller

        var imageId = Guid.NewGuid();
        var filePathForImage = GetFilePathForImage(owner, imageId);
        await _fileSystem.CreateFileAsync(filePathForImage, newImage.Stream);

        return imageId;
    }

    /// <inheritdoc />
    public Stream GetImage(Guid ownerId, Guid imageId) => _fileSystem.OpenRead(GetFilePathForImage(ownerId, imageId));

    /// <inheritdoc />
    public void DeleteImage(Guid imageId) => _fileSystem.DeleteFile(Path.Combine(_imageDirectory, imageId.ToString()));

    public void EnsureStorageExists()
    {
        if (!Directory.Exists(_dbDirectory)) { Directory.CreateDirectory(_dbDirectory); }

        if (!File.Exists(DatabasePath))
        {
            Database.EnsureCreated();
            Database.Migrate();
        }
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LifePoint>().ToTable(nameof(LifePoint));
        modelBuilder.Entity<LifePoint>().HasKey(nameof(LifePoint.Id));
        modelBuilder.Entity<LifePoint>().Navigation(point => point.CreatedBy).AutoInclude();
        modelBuilder.Entity<Person>().ToTable(nameof(Person));
        modelBuilder.Entity<Person>().HasKey(nameof(LifePoint.Id));
    }

    private string GetFilePathForImage(Person owner, Guid imageId) => GetFilePathForImage(owner.Id, imageId);

    private string GetFilePathForImage(Guid ownerId, Guid imageId) => Path.Combine(_imageDirectory, ownerId.ToString(), imageId.ToString());
}