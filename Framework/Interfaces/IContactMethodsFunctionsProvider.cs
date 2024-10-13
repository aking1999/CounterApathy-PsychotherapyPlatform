using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Interfaces
{
    public interface IContactMethodsFunctionsProvider
    {
        bool ContactMethodExistsInDatabase(string contactMethodId);
    }
}
