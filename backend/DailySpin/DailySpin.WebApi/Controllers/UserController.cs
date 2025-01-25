using DailySpin.Application;
using System.Diagnostics.CodeAnalysis;

namespace DailySpin.WebApi;

public class UserController : IController
{
    [HttpGet]
    [Authorize("User")] 
    [Route("/api/user/me")]
    public object[] GetCurrentUser()
    {
        return
        [
            new 
            {
                id = "1",
                name = "John Doe",
                email = "john.doe@example.com",
                role = "User"
            },
            
            new
            {
                id = "2",
                name = "Mark Lie",
                email = "mark.lye@example.com",
                role = "User"
            }
        ];
    }

    [HttpGet]
    [Authorize("User")]
    [Route("/home")]
    public IActionResult RenderHomePage()
    {
        return this.View("index");
    }
}
