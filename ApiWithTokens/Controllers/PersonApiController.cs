using System.Collections.Generic;
using System.Web.Http;
using ApiWithTokens.Models;

namespace ApiWithTokens.Controllers
{
    [Authorize]
    public class PersonApiController : ApiController
    {
        public IList<Person> Get()
        {
            return new PersonDatabase();
        }

    }
}
