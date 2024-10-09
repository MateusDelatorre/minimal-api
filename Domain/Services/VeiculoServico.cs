using MinimalApi.Domain.Entities;
using MinimalApi.DTOs;
using MinimalApi.Infrastructure.Context;
using MinimalApi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MinimalApi.Domain.Services;

public class VehicleService : IVehicleService
{
    private readonly MyDbContext _contexto;
    public VehicleService(MyDbContext contexto)
    {
        _contexto = contexto;
    }

    public void Delete(Vehicle vehicle)
    {
        _contexto.Vehicles.Remove(vehicle);
        _contexto.SaveChanges();
    }

    public void Update(Vehicle vehicle)
    {
        _contexto.Vehicles.Update(vehicle);
        _contexto.SaveChanges();
    }

    public Vehicle? GetById(int id)
    {
        return _contexto.Vehicles.Where(v => v.Id == id).FirstOrDefault();
    }

    public void Create(Vehicle vehicle)
    {
        _contexto.Vehicles.Add(vehicle);
        _contexto.SaveChanges();
    }

    public List<Vehicle> GetAll(int? page = 1, string? name = null, string? brand = null)
    {
        var query = _contexto.Vehicles.AsQueryable();
        if(!string.IsNullOrEmpty(name))
        {
            query = query.Where(v => EF.Functions.Like(v.Name.ToLower(), $"%{name}%"));
        }

        int ItensPerPage = 10;

        if(page != null)
            query = query.Skip(((int)page - 1) * ItensPerPage).Take(ItensPerPage);

        return query.ToList();
    }
}