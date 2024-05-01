using InternshipProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Net;

namespace InternshipProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _appcontext;
        private readonly IHttpContextAccessor _context;
        public HomeController(ILogger<HomeController> logger,ApplicationDbContext applicationDbContext, IHttpContextAccessor context)
        {
            _appcontext = applicationDbContext;
            _logger = logger;
            _context = context;
        }
        [HttpGet]
        public IActionResult LogIn()
        {
            return View();
        }
        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }
        public void SendMail(Guid id)
        {
            var myAppConfig = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var username = myAppConfig.GetValue<string>("EmailConfig:Username");
            var password = myAppConfig.GetValue<string>("EmailConfig:Password");
            var host = myAppConfig.GetValue<string>("EmailConfig:Host");
            var port = myAppConfig.GetValue<int>("EmailConfig:Port");
            var Frommail = myAppConfig.GetValue<string>("EmailConfig:FromEmail");
            MailMessage message = new MailMessage();
            message.From = new MailAddress(Frommail);
            var m= _appcontext.Responses.Where(a=>a.ApprovalRequestViewModelId==id).ToList();
            var n= _appcontext.Requests.Where(a=>a.Id==id);
            string x = "";
            string y = n.Select(a => a.Topic).First();
            for(var i=0;i< m.Count();i++)
            {
                if (m[i].Response == true)
                {
                    if (i == m.Count() - 1)
                    {
                        x= n.Select(a=>a.Email).First();
                        message.To.Add(x);
                        message.Subject = "Request Approved";
                        message.Body = "Dear User," + "\n" +
                                x + "your request for " + y + "\n" +
                                "created on" + n.Select(a=>a.Created).First().ToString() + "has been accepted"+ "\n" +
                                "Thanks & Regards"; 
                        break;
                    }
                    else
                    {
                        continue;
                    }
                    
                }
                else if(m[i].Response==false)
                {
                    x= n.Select(a => a.Email).First();
                    message.To.Add(x);
                    message.Subject = "Request Denied";
                    message.Body = "Dear User," + "\n" + 
                                x + "your request for " + y + "\n" +
                                "created on" + n.Select(a => a.Created).First().ToString() + "has been rejected" + "\n" +
                                "Thanks & Regards";
                    break;
                }
                else
                {
                    x = m[i].ApproverEmail;
                    message.To.Add(x);
                    message.Subject = "Approval Request for" + y;
                    message.Body = "Dear User," + "\n" +
                                n.Select(a=>a.Email).First() + " has raised request for your approval for " + y +
                                " created on" + n.Select(a => a.Created).First().ToString() + "\n"+
                                "Please visit your dashboard for Responding..."+ "\n"+
                                "Thanks & Regards";
                    break;
                }
            }
            message.IsBodyHtml = true;
            SmtpClient mailclient = new SmtpClient(host);
            try
            {

                mailclient.UseDefaultCredentials = false;
                mailclient.Credentials = new NetworkCredential(username, password);
                mailclient.Host = host;
                mailclient.Port = port;
                //mailclient.EnableSsl=true;
                mailclient.Send(message);
            }
            catch (Exception)
            {

            }
            finally
            {
                mailclient?.Dispose();
            }





        }
        [HttpPost]
        public async Task<IActionResult> SignUp(EmpLoginModel emp)
        {
            await _appcontext.UserDetails.AddAsync(emp);
            await _appcontext.SaveChangesAsync();
            return RedirectToAction("LogIn");
        }
        [HttpPost]
        public async Task<IActionResult> LogIn(EmpLoginModel emp)
        {
            var user = await _appcontext.UserDetails.FirstOrDefaultAsync(u => u.Email == emp.Email
                                                                                       && u.Password == emp.Password);
            if (user!=null)
            {
                _context.HttpContext.Session.SetString("UserEmail", user.Email);
                _context.HttpContext.Session.SetString("UserId", user.EmpId.ToString());
                return RedirectToAction("Dashboard");

            }
            else
            {
                return RedirectToAction("SignUp");
            }
            
        }
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            string id=_context.HttpContext.Session.GetString("UserId");
            if (id != null)
            {
                Guid userId = Guid.Parse(id);
                var details = await _appcontext.UserDetails.FirstOrDefaultAsync(u => u.EmpId == userId);
                return View(details);
            }
            return RedirectToAction("LogIn");
        }
        [HttpGet]
        public async Task<IActionResult> MyRequests()
        {
            var email = _context.HttpContext?.Session.GetString("UserEmail");
            if (email != null)
            {
                var requests = await _appcontext.Requests.Where(a=>a.Email==email).ToListAsync();
                foreach (var id in requests)
                {
                    var responses = _appcontext.Responses.Where(a => a.ApprovalRequestViewModelId == id.Id)
                        .ToList();
                    for(var i= 0;i< responses.Count(); i++)
                    {
                        if (responses[i].Response == false)
                        {
                            var x = _appcontext.Requests.Find(responses[i].ApprovalRequestViewModelId);
                            x.Status = false;
                            await _appcontext.SaveChangesAsync();
                            break;
                        }
                        else if (responses[i].Response == true)
                        {
                            if (i == responses.Count() - 1)
                            {
                                var x = _appcontext.Requests.Find(responses[i].ApprovalRequestViewModelId);
                                x.Status = true;
                                break;
                            }
                            else
                            {
                                continue;
                            }
                            
                        }
                        else 
                        {
                            continue;
                        }
                    }
                }
                return View(requests);
            }
            else
            {
                return RedirectToAction("LogIn");
            }    
           
        }
        [HttpGet]
        public IActionResult NewRequest()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> NewRequest(ApprovalRequestViewModel model)
        {
            var id = _context.HttpContext?.Session.GetString("UserId");
            if (id != null)
            {
                Guid userId = Guid.Parse(id);
                var details = _appcontext.UserDetails.Find(userId);
                foreach (var email in model.ApproverEmails)
                {
                    if (!model.ApproverEmails.Any(a => a.ApproverEmail == email.ApproverEmail))
                    {
                        model.ApproverEmails.Add(email);
                    }
                }
                
                model.Email = details.Email;
                model.Department = details.Department;
                await _appcontext.Requests.AddAsync(model);
                _context.HttpContext.Session.SetString("RequestId", model.Id.ToString());
                await _appcontext.SaveChangesAsync();
                SendMail(model.Id);
                return RedirectToAction("Dashboard");
            }
            else
            {
                return RedirectToAction("LogIn");
            }
            
        }
       
        [HttpGet]
        public IActionResult MyApprovals()
        {
            var email = _context.HttpContext.Session.GetString("UserEmail");
            if (email != null)
            {
                //var s = await _appcontext.Responses
                //            .Where(r => r.ApproverEmail == email && r.Response == null)
                //            .GroupBy(r => r.ApprovalRequestViewModelId)
                //            .Select(group => group.OrderBy(r => r.ResponseId).FirstOrDefault())
                //            .ToListAsync();
               var results= _appcontext.Responses
                            .Where(r1 => r1.Response == null && r1.ApproverEmail == email &&
                            _appcontext.Responses
                            .GroupBy(r2 => r2.ApprovalRequestViewModelId)
                            .Select(group => group.Min(r2 => r2.ResponseId))
                            .Contains(r1.ResponseId))
                            .ToList();

                return View(results);
            }
            else
            {
                return RedirectToAction("LogIn");
            }
            
        }
        [HttpGet]
        public IActionResult Respond(int id)
        {
            var x=_appcontext.Responses.Find(id);
            var y = _appcontext.Requests.Find(x.ApprovalRequestViewModelId);
            _context.HttpContext.Session.SetInt32("ResponseId", x.ResponseId);
            return View(y);
        }
        [HttpPost]
        public async Task<IActionResult> Accept(ResponseViewModel model)
        {
            var x= _context.HttpContext.Session.GetInt32("ResponseId");
            if (x != null)
            {
                var y=await _appcontext.Responses.FindAsync(x);
                if (y != null)
                {
                    y.Response =true;
                    await _appcontext.SaveChangesAsync();
                    SendMail(y.ApprovalRequestViewModelId);
                }
            }
            
            return RedirectToAction("MyApprovals");
        }
        [HttpPost]
        public async Task<IActionResult> Deny(ResponseViewModel model)
        {
            var x = _context.HttpContext.Session.GetInt32("ResponseId");
            if (x != null)
            {
                var y = await _appcontext.Responses.FindAsync(x);
                if (y != null)
                {
                    y.Response = false;
                    await _appcontext.SaveChangesAsync();
                    SendMail(y.ApprovalRequestViewModelId);
                }
            }
            return RedirectToAction("MyApprovals");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
