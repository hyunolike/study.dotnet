using Microsoft.AspNetCore.Mvc;

namespace PracticalApps.Controllers;

public class HelloController : Controller
{
    public string Index() => "Hello World!";
}