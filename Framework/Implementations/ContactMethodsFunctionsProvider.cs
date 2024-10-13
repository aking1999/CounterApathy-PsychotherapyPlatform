using Database.Models;
using Database.RepositoryImplementations;
using Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Implementations
{
    public class ContactMethodsFunctionsProvider : IContactMethodsFunctionsProvider
    {
        private readonly UnitOfWork _context;

        public ContactMethodsFunctionsProvider()
        {
            _context = new UnitOfWork(new LajsnaProbaContext());
        }

        public bool ContactMethodExistsInDatabase(string contactMethodId)
        {
            return _context.ContactMethods.GetById(contactMethodId) != null;
        }
    }
}
