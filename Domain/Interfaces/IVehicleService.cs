
using MinimalApi.Domain.Entities;
using MinimalApi.DTOs;

namespace MinimalApi.Domain.Interfaces;

public interface IVehicleService
{
    List<Vehicle> GetAll(int? page = 1, string? name = null, string? brand = null);
    Vehicle? GetById(int id);
    void Create(Vehicle vehicle);
    void Update(Vehicle vehicle);
    void Delete(Vehicle vehicle);
}