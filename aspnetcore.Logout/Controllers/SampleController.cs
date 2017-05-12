using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Builder;
using System.Security.Principal;
using System.Diagnostics;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace aspnetcore.Logout.Controllers
{
    [Route("api/[controller]")]
    public class SampleController : Controller
    {
        SessionOptions sessionOptions;

        public SampleController(IOptions<SessionOptions> sessionOptions)
        {
            this.sessionOptions = sessionOptions.Value;
        }
        [HttpGet]
        [Route("GetUserData")]
        public IActionResult GetUserData()
        {
            Dictionary<string, object> ret = new Dictionary<string, object>();
            ret.Add("Context", this.Request.HttpContext.User.Identity.Name);
            WindowsIdentity wi = WindowsIdentity.GetCurrent();
            var currentThread = System.Threading.Thread.CurrentThread;

            ret.Add("WindowsIdentity", wi.Name);
            ret.Add("ThreadId", currentThread.ManagedThreadId);
            ret.Add("ThreadUser", System.Threading.Thread.CurrentPrincipal.Identity?.Name);
            return new ObjectResult(ret); 
        }

        [Route("/Auth/Logout")]
        public async Task<IActionResult> SignOut([FromQuery]string returnPath = "/")
        {
            if (this.Request.Cookies.ContainsKey("itxSingOutUser") && this.Request.Cookies["itxSingOutUser"] != this.Request.HttpContext.User.Identity.Name)
            {
                
                
                this.HttpContext.Response.Cookies.Delete("itnetx:version");
                this.HttpContext.Response.Cookies.Delete("itxSingOutUser");
                this.HttpContext.Response.Cookies.Delete(this.sessionOptions.CookieName);
                this.HttpContext.Session.Clear();

                this.HttpContext.Response.Headers.Add("Cache-control", "no-store, must-revalidate, private,no-cache");
                this.HttpContext.Response.Headers.Add("Pragma", "no-cache");
                this.HttpContext.Response.Headers.Add("Expires", "0");

                WindowsIdentity wi2 = WindowsIdentity.GetCurrent();
                Debug.WriteLine($"WindowsIdentity user: {wi2.Name}");

                System.Threading.Thread.CurrentPrincipal = this.HttpContext.User;// = new System.Security.Claims.ClaimsPrincipal(WindowsIdentity.GetCurrent());
                Debug.WriteLine($"Logged in with user: {this.Request.HttpContext.User.Identity.Name}");
                Debug.WriteLine("Thread Info:\r\nCurrentPrincipal: {0}\r\nManagedThreadId: {1}\r\n", System.Threading.Thread.CurrentPrincipal.Identity?.Name, System.Threading.Thread.CurrentThread.ManagedThreadId);

                return Redirect(returnPath);
            }

            WindowsIdentity wi = WindowsIdentity.GetCurrent();
            Debug.WriteLine("Try to signout user: {0}", this.Request.HttpContext.User.Identity.Name);
            Debug.WriteLine("WindowsIdentity user: {0}", wi.Name);
            Debug.WriteLine("Thread Info:\r\nCurrentPrincipal: {0}\r\nManagedThreadId: {1}\r\n", System.Threading.Thread.CurrentPrincipal.Identity?.Name, System.Threading.Thread.CurrentThread.ManagedThreadId);
            Debug.WriteLine("Session Info:\r\nId: {0}\r\nKeys: {1}\r\n", this.Request.HttpContext.Session.Id, string.Join(";", this.Request.HttpContext.Session.Keys.ToArray()));

            this.Response.Cookies.Append("itxSingOutUser", this.Request.HttpContext.User.Identity.Name, new Microsoft.AspNetCore.Http.CookieOptions() { Expires = DateTimeOffset.Now.AddMinutes(1) });
            return Unauthorized();
        }
    }
}
