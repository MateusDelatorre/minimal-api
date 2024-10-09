using MinimalApi.Domain.Entities;
using MinimalApi.DTOs;

namespace MinimalApi.Domain.Interfaces;

public interface IAdministratorService
{
    Administrator? Authenticate(LoginDTO loginDTO);
    Administrator? Create(Administrator administrators);
    List<Administrator> GetAll(int? page);
    Administrator? GetById(int id);
}