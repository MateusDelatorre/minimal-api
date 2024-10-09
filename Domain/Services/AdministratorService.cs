using MinimalApi.Domain.Entities;
using MinimalApi.DTOs;
using MinimalApi.Infrastructure.Context;
using MinimalApi.Domain.Interfaces;

namespace MinimalApi.Domain.Services;

public class AdministratorService : IAdministratorService
{
    private readonly MyDbContext _contexto;
    public AdministratorService(MyDbContext contexto)
    {
        _contexto = contexto;
    }

    public Administrator? GetById(int id)
    {
        return _contexto.Administrators.Where(v => v.Id == id).FirstOrDefault();
    }

    public Administrator Create(Administrator administrators)
    {
        _contexto.Administrators.Add(administrators);
        _contexto.SaveChanges();

        return administrators;
    }

    public Administrator? Authenticate(LoginDTO loginDTO)
    {
        var adm = _contexto.Administrators.Where(a => a.Email == loginDTO.Email && a.Password == loginDTO.Password).FirstOrDefault();
        return adm;
    }

    public List<Administrator> GetAll(int? page)
    {
        var query = _contexto.Administrators.AsQueryable();

        int ItensPerPage = 10;

        if(page != null)
            query = query.Skip(((int)page - 1) * ItensPerPage).Take(ItensPerPage);

        return query.ToList();
    }
}