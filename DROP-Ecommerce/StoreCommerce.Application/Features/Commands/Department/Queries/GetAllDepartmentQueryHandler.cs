using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetAllDepartmentQueryHandler(IRepository<Department> repository)
    : BaseGetAllHandler<GetAllDepartmentQuery, Department>(repository);
