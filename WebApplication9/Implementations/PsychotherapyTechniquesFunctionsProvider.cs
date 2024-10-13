using Database.Models;
using Database.RepositoryImplementations;
using System;
using System.Collections.Generic;
using System.Linq;
using WebApplication9.Interfaces;
using WebApplication9.ViewModels;

namespace WebApplication9.Implementations
{
    public class PsychotherapyTechniquesFunctionsProvider : IPsychotherapyTechniquesFunctionsProvider
    {
        private readonly UnitOfWork _context;

        public PsychotherapyTechniquesFunctionsProvider()
        {
            _context = new UnitOfWork(new LajsnaProbaContext());
        }
    }
}
