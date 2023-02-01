﻿using Domain.Entities;
using Domain.Interfaces;

namespace Infrastructure.Repositories
{
    /**
    * @Project ASP.NET Core 7.0
    * @Author: Nguyen Xuan Nhan
    * @Team: 4FT
    * @Copyright (C) 2023 4FT. All rights reserved
    * @License MIT
    * @Create date Mon 23 Jan 2023 00:00:00 AM +07
    */

    public class BranchRepository : GenericRepository<Branch>, IBranchRepository
    {
        public BranchRepository(ApplicationDbContext context) : base(context)
        {
        }

        public List<Branch> GetBranches() => GetAll().ToList();

        public List<Branch> GetBranchesByNames(string? name) => GetList(filter: x => name != null && x.name != null && x.name.Contains(name)).ToList();

        public Branch? GetBranchById(string? id) => id == null ? null : GetById(id);

        public void AddBranch(Branch branch) => Insert(branch);

        public void UpdateBranch(Branch branch, string id) => Update(branch, x => x.id == id);

        public void DeleteBranch(string id) => Delete(x => x.id == id);
    }
}