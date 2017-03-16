using BS.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BS.Common.Dao
{
    public interface IModulesDAO : IBaseDAO
    {
        IList<Entity> GetUserModules(string appName, string userLogin);
    }
}
